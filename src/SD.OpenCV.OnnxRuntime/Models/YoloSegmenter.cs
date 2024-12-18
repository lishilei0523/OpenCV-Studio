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
    /// YOLO图像分割模型
    /// </summary>
    public class YoloSegmenter : OnnxModel<Mat, Segmentation[]>
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
        /// 创建YOLO图像分割模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public YoloSegmenter(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建YOLO图像分割模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public YoloSegmenter(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建YOLO图像分割模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public YoloSegmenter(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建YOLO图像分割模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public YoloSegmenter(byte[] modelBytes, SessionOptions sessionOptions)
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

        #region # 处理推理结果 —— override Segmentation[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override unsafe Segmentation[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<float> results0 = inference[0].AsTensor<float>();
            Tensor<float> results1 = inference[1].AsTensor<float>();

            //读取分类文本
            string[] labels = this.GetLabels();

            //计算缩放系数
            float scaleX = this._sourceSize.Width * 1.0f / this._adaptiveSize.Width;
            float scaleY = this._sourceSize.Height * 1.0f / this._adaptiveSize.Height;
            float scaleMask = SideSize * 1.0f / results1.Dimensions[2];

            //定义Sigmoid函数
            Func<float, float> sigmoid = x =>
            {
                float y = 1.0f / (1.0f + (float)Math.Exp(-x));
                return y;
            };

            //解析目标检测部分
            using Mat detMat = Mat.FromArray<float>(results0);
            using Mat reshapedDetMat = detMat.Reshape(1, results0.Dimensions[1]);
            using Mat transposedDetMat = reshapedDetMat.Transpose();
            IList<Detection> detections = new List<Detection>();
            IList<Mat> partialMasks = new List<Mat>();
            for (int rowIndex = 0; rowIndex < transposedDetMat.Rows; rowIndex++)
            {
                using Mat rowMat = transposedDetMat[rowIndex, rowIndex + 1, 0, transposedDetMat.Cols];
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

                    Mat partialMask = transposedDetMat.Row(rowIndex).ColRange(labels.Length + 4, transposedDetMat.Cols);
                    partialMasks.Add(partialMask);
                }
            }

            //NMS
            CvDnn.NMSBoxes(detections.Select(x => x.Box), detections.Select(x => x.Confidence), ScoreThreshold, NmsThreshold, out int[] indices);

            //解析图像分割部分
            using Mat segMat = Mat.FromArray<float>(results1);
            using Mat reshapedSegMat = segMat.Reshape(1, results1.Dimensions[1]);
            IList<Segmentation> segmentations = new List<Segmentation>();
            foreach (int index in indices)
            {
                Detection detection = detections[index];

                //计算分割结果
                using Mat mask = partialMasks[index] * reshapedSegMat;
                mask.ForEachAsFloat((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    float pixel = *valuePtr;
                    mask.At<float>(rowIndex, colIndex) = sigmoid(pixel);
                });
                using Mat reshapedMask = mask.Reshape(1, results1.Dimensions[2]);

                //裁剪掩膜
                int boxXMin = Math.Max(0, detection.Box.X);
                int boxYMin = Math.Max(0, detection.Box.Y);
                int boxXMax = Math.Max(0, detection.Box.BottomRight.X);
                int boxYMax = Math.Max(0, detection.Box.BottomRight.Y);
                int maskXMin = (int)Math.Floor((boxXMin * 1.0f / scaleX + this._paddingX) / scaleMask);
                int maskYMin = (int)Math.Floor((boxYMin * 1.0f / scaleY + this._paddingY) / scaleMask);
                int maskXMax = (int)Math.Floor((boxXMax * 1.0f / scaleX + this._paddingX) / scaleMask);
                int maskYMax = (int)Math.Floor((boxYMax * 1.0f / scaleY + this._paddingY) / scaleMask);
                using Mat rangedMask = new Mat(reshapedMask, new Range(maskYMin, maskYMax), new Range(maskXMin, maskXMax));

                //调整掩膜到检测框尺寸
                using Mat resizedMask = rangedMask.Resize(detection.Box.Size);

                //二值化掩膜
                resizedMask.ForEachAsFloat((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    float pixel = *valuePtr;
                    resizedMask.At<float>(rowIndex, colIndex) = pixel > 0.5f ? 255f : 0f;
                });

                //转换8UC1格式
                using Mat byteMask = new Mat();
                resizedMask.ConvertTo(byteMask, MatType.CV_8UC1);

                //修整Box
                if ((boxXMin + byteMask.Width) >= this._sourceSize.Width)
                {
                    boxXMax = this._sourceSize.Width - 1;
                }
                if ((boxYMin + byteMask.Height) >= this._sourceSize.Height)
                {
                    boxYMax = this._sourceSize.Height - 1;
                }
                Rect correctBox = new Rect(boxXMin, boxYMin, boxXMax - boxXMin, boxYMax - boxYMin);

                //生成最终掩膜
                using Mat canvas = Mat.Zeros(this._sourceSize, MatType.CV_8UC1);
                using Mat canvasRoi = new Mat(canvas, correctBox);
                using Mat rangedByteMask = new Mat(byteMask, new Range(0, correctBox.Height), new Range(0, correctBox.Width));
                rangedByteMask.CopyTo(canvasRoi);

                //查找轮廓
                Cv2.FindContours(canvas, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                if (contours.Any())
                {
                    Point[] contour = contours.OrderByDescending(contour => Cv2.ArcLength(contour, false)).First();
                    Segmentation segmentation = new Segmentation(detection.Label, correctBox, contour, detection.Confidence);
                    segmentations.Add(segmentation);
                }
            }

            //释放资源
            foreach (Mat partialMask in partialMasks)
            {
                partialMask.Dispose();
            }

            return segmentations.ToArray();
        }
        #endregion

        #region # 获取标签列表 —— string[] GetLabels()
        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <returns>标签列表</returns>
        private string[] GetLabels()
        {
            string[] labels = AppDomain.CurrentDomain.GetData(typeof(YoloSegmenter).FullName!) as string[];
            if (labels == null)
            {
                labels = File.ReadAllLines("Content/Labels/yolo_labels.txt");
            }

            return labels;
        }
        #endregion 
    }
}
