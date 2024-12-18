using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using OpenCvSharp.Dnn;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Values;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// YOLO目标检测模型
    /// </summary>
    public class YoloDetector : OnnxModel<Mat, Detection[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 调整后图像边长
        /// </summary>
        private const int SideSize = 640;

        /// <summary>
        /// NMS分数阈值
        /// </summary>
        private const float ScoreThreshold = 0.25f;

        /// <summary>
        /// NMS阈值
        /// </summary>
        private const float NmsThreshold = 0.5f;

        /// <summary>
        /// 源图像尺寸
        /// </summary>
        private Size _sourceSize;

        /// <summary>
        /// 自适应图像尺寸
        /// </summary>
        private Size _adaptiveSize;

        /// <summary>
        /// X轴内边距
        /// </summary>
        private int _paddingX;

        /// <summary>
        /// Y轴内边距
        /// </summary>
        private int _paddingY;

        /// <summary>
        /// 创建YOLO目标检测模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public YoloDetector(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建YOLO目标检测模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public YoloDetector(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建YOLO目标检测模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public YoloDetector(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建YOLO目标检测模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public YoloDetector(byte[] modelBytes, SessionOptions sessionOptions)
            : base(modelBytes, sessionOptions)
        {

        }

        #endregion

        #region # 处理推理输入 —— override List<NamedOnnxValue> ProcessInput(Mat input)
        /// <summary>
        /// 处理推理输入
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <returns>ONNX键值列表</returns>
        protected override unsafe List<NamedOnnxValue> ProcessInput(Mat input)
        {
            this._sourceSize = input.Size();

            //缩放图像到 640*640
            using Mat resizedImage = input.ResizeAdaptively(SideSize, out this._adaptiveSize, out this._paddingX, out this._paddingY);

            DenseTensor<float> sourceTensor = new DenseTensor<float>([1, 3, SideSize, SideSize]);
            resizedImage.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b pixel = *valuePtr;

                //按模型要求标准化处理像素
                sourceTensor[0, 0, rowIndex, colIndex] = pixel[0] / 255f;
                sourceTensor[0, 1, rowIndex, colIndex] = pixel[1] / 255f;
                sourceTensor[0, 2, rowIndex, colIndex] = pixel[2] / 255f;
            });

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", sourceTensor)
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override Detection[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override Detection[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<float> results0 = inference[0].AsTensor<float>();

            //读取分类文本
            string[] labels = this.GetLabels();

            //计算缩放系数
            float scaleX = this._sourceSize.Width * 1.0f / this._adaptiveSize.Width;
            float scaleY = this._sourceSize.Height * 1.0f / this._adaptiveSize.Height;

            //解析推理结果
            using Mat mat = Mat.FromArray<float>(results0);
            using Mat reshapedMat = mat.Reshape(1, labels.Length + 4);//84 = Box[cx,cy,w,h] + classes_count
            using Mat transposedMat = reshapedMat.Transpose();
            IList<Detection> detections = new List<Detection>();
            for (int rowIndex = 0; rowIndex < transposedMat.Rows; rowIndex++)
            {
                using Mat rowMat = transposedMat[rowIndex, rowIndex + 1, 0, transposedMat.Cols];
                rowMat.GetArray(out float[] row);

                float[] boxArray = new float[4];
                float[] confidencesArray = new float[labels.Length];
                Array.Copy(row, 0, boxArray, 0, boxArray.Length);
                Array.Copy(row, 4, confidencesArray, 0, confidencesArray.Length);
                float maxConfidence = confidencesArray.Max();//获取置信度最大值
                if (maxConfidence >= minConfidence)
                {
                    float centerX = (boxArray[0] - this._paddingX) * scaleX;
                    float centerY = (boxArray[1] - this._paddingY) * scaleY;
                    float width = boxArray[2] * scaleX;
                    float height = boxArray[3] * scaleX;
                    float xMin = centerX - width / 2;
                    float yMin = centerY - height / 2;
                    Point location = new Point(xMin, yMin);
                    Size size = new Size(width, height);
                    int maxConfidenceIndex = Array.IndexOf(confidencesArray, maxConfidence);

                    Detection detection = new Detection(labels[maxConfidenceIndex], new Rect(location, size), maxConfidence);
                    detections.Add(detection);
                }
            }

            //NMS
            CvDnn.NMSBoxes(detections.Select(x => x.Box), detections.Select(x => x.Confidence), ScoreThreshold, NmsThreshold, out int[] indices);
            Detection[] nmsDetections = detections.Where((_, index) => indices.Contains(index)).ToArray();

            return nmsDetections;
        }
        #endregion

        #region # 获取标签列表 —— string[] GetLabels()
        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <returns>标签列表</returns>
        private string[] GetLabels()
        {
            string[] labels = AppDomain.CurrentDomain.GetData(typeof(YoloDetector).FullName!) as string[];
            if (labels == null)
            {
                labels = File.ReadAllLines("Content/Labels/yolo_labels.txt");
            }

            return labels;
        }
        #endregion 
    }
}
