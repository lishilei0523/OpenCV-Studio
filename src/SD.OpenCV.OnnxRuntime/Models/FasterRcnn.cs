using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Values;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// FasterRCNN模型
    /// </summary>
    public class FasterRcnn : OnnxModel<Mat, Detection[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 调整后图像宽度
        /// </summary>
        private const int ResizedWidth = 1312;

        /// <summary>
        /// 调整后图像高度
        /// </summary>
        private const int ResizedHeight = 800;

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
        /// 创建FasterRCNN模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public FasterRcnn(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建FasterRCNN模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public FasterRcnn(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建FasterRCNN模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public FasterRcnn(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建FasterRCNN模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public FasterRcnn(byte[] modelBytes, SessionOptions sessionOptions)
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

            //缩放图像
            using Mat resizedImage = input.ResizeAdaptively(ResizedWidth, ResizedHeight, out this._adaptiveSize, out this._paddingX, out this._paddingY);

            //此为FasterRCNN预处理时的均值
            float[] mean = [102.9801f, 115.9465f, 122.7717f];

            DenseTensor<float> sourceTensor = new DenseTensor<float>([3, ResizedHeight, ResizedWidth]);//[通道数, 图像高, 图像宽]
            resizedImage.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b pixel = *valuePtr;

                //按模型要求标准化处理像素
                sourceTensor[0, rowIndex, colIndex] = pixel[0] - mean[0];
                sourceTensor[1, rowIndex, colIndex] = pixel[1] - mean[1];
                sourceTensor[2, rowIndex, colIndex] = pixel[2] - mean[2];
            });

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", sourceTensor)
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
            Tensor<float> boxesTensor = inference[0].AsTensor<float>();
            Tensor<long> labelsTensor = inference[1].AsTensor<long>();
            Tensor<float> confidencesTensor = inference[2].AsTensor<float>();

            //读取分类文本
            string[] labels = this.GetLabels();

            //计算缩放系数
            float scaleX = this._sourceSize.Width * 1.0f / this._adaptiveSize.Width;
            float scaleY = this._sourceSize.Height * 1.0f / this._adaptiveSize.Height;

            //解析推理结果
            IList<Detection> detections = new List<Detection>();
            for (int index = 0; index < labelsTensor.Length; index++)
            {
                if (confidencesTensor[index] >= minConfidence)
                {
                    float xMin = (boxesTensor[index, 0] - this._paddingX) * scaleX;
                    float yMin = (boxesTensor[index, 1] - this._paddingY) * scaleY;
                    float xMax = (boxesTensor[index, 2] - this._paddingX) * scaleX;
                    float yMax = (boxesTensor[index, 3] - this._paddingY) * scaleY;
                    Point location = new Point(xMin, yMin);
                    Size size = new Size(xMax - xMin, yMax - yMin);

                    Detection detection = new Detection(labels[labelsTensor[index]], new Rect(location, size), confidencesTensor[index]);
                    detections.Add(detection);
                }
            }

            return detections.ToArray();
        }
        #endregion

        #region # 获取标签列表 —— string[] GetLabels()
        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <returns>标签列表</returns>
        private string[] GetLabels()
        {
            string[] lines = File.ReadAllLines("Assets/Labels/faster_rcnn_labels.txt");
            string[] labels = new string[lines.Length];
            for (int index = 0; index < lines.Length; index++)
            {
                labels[index] = lines[index];
            }

            return labels;
        }
        #endregion 
    }
}
