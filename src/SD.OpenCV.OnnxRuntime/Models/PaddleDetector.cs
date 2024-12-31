using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Results;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// Paddle文本检测模型
    /// </summary>
    public class PaddleDetector : OnnxModel<Mat, PaddleContour[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 调整后图像边长
        /// </summary>
        private const int SideSize = 1024;

        /// <summary>
        /// 长边阈值
        /// </summary>
        private const float LongSideThreshold = 80.0f;

        /// <summary>
        /// 短边阈值
        /// </summary>
        private const float ShortSideThreshold = 3.0f;

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
        /// 创建Paddle文本检测模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public PaddleDetector(string modelPath)
            : this(modelPath, new SessionOptions())
        {
        }

        /// <summary>
        /// 创建Paddle文本检测模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public PaddleDetector(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Paddle文本检测模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleDetector(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建Paddle文本检测模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleDetector(byte[] modelBytes, SessionOptions sessionOptions)
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

            //缩放图像到 1024*1024
            using Mat resizedImage = input.ResizeAdaptively(SideSize, out this._adaptiveSize, out this._paddingX, out this._paddingY);

            //此为PaddleOCR预处理时的均值、标准差
            float[] meanValues = [0.485f, 0.456f, 0.406f];
            float[] normValues = [0.229f, 0.224f, 0.225f];

            DenseTensor<float> sourceTensor = new DenseTensor<float>([1, 3, SideSize, SideSize]);//[批次数, 通道数, 图像高, 图像宽]
            resizedImage.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b pixel = *valuePtr;

                //按模型要求标准化处理像素
                sourceTensor[0, 0, rowIndex, colIndex] = (pixel[2] / 255f - meanValues[0]) / normValues[0];
                sourceTensor[0, 1, rowIndex, colIndex] = (pixel[1] / 255f - meanValues[1]) / normValues[1];
                sourceTensor[0, 2, rowIndex, colIndex] = (pixel[0] / 255f - meanValues[2]) / normValues[2];
            });

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("x", sourceTensor)
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override PaddleContour[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override PaddleContour[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<float> results0 = inference[0].AsTensor<float>();

            //解析推理结果
            using Mat rawMat = Mat.FromArray(results0.Select(x => x * 255f));
            using Mat reshapedMat = rawMat.Reshape(1, SideSize);
            using Mat mask = new Mat();
            reshapedMat.ConvertTo(mask, MatType.CV_8UC1);

            //计算缩放系数
            float scaleX = this._sourceSize.Width * 1.0f / this._adaptiveSize.Width;
            float scaleY = this._sourceSize.Height * 1.0f / this._adaptiveSize.Height;

            //提取并处理轮廓
            Cv2.FindContours(mask, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
            int maxContoursCount = Math.Min(contours.Length, 1000);
            IList<PaddleContour> paddleContours = new List<PaddleContour>();
            for (int index = 0; index < maxContoursCount; index++)
            {
                Point[] contour = contours[index];

                //置信度过滤
                float confidence = mask.GetContourConfidence(contour);
                if (confidence < minConfidence)
                {
                    continue;
                }

                //缩放
                IEnumerable<Point> scaledContour =
                    from point in contour
                    let scaledPoint = new Point((point.X - this._paddingX) * scaleX, (point.Y - this._paddingY) * scaleY)
                    select scaledPoint;

                //边长过滤
                RotatedRect rotatedRect = Cv2.MinAreaRect(scaledContour);
                float longSide = Math.Max(rotatedRect.Size.Width, rotatedRect.Size.Height);
                float shortSide = Math.Min(rotatedRect.Size.Width, rotatedRect.Size.Height);
                if (longSide < LongSideThreshold)
                {
                    continue;
                }
                if (shortSide < ShortSideThreshold)
                {
                    continue;
                }

                //交换长宽
                bool swapSize = rotatedRect.Size.Width < rotatedRect.Size.Height || Math.Abs(rotatedRect.Angle) >= 60.0f;
                if (swapSize)
                {
                    (rotatedRect.Size.Width, rotatedRect.Size.Height) = (rotatedRect.Size.Height, rotatedRect.Size.Width);
                    if (rotatedRect.Angle < 0)
                    {
                        rotatedRect.Angle += 90;
                    }
                    else if (rotatedRect.Angle > 0)
                    {
                        rotatedRect.Angle -= 90;
                    }
                }

                //扩张轮廓
                IList<Point2f> swappedContour = rotatedRect.Points();
                IList<Point2f> expandedContour = swappedContour.ExpandContour(1.7f);

                //边长过滤
                rotatedRect = Cv2.MinAreaRect(expandedContour);
                longSide = Math.Max(rotatedRect.Size.Width, rotatedRect.Size.Height);
                shortSide = Math.Min(rotatedRect.Size.Width, rotatedRect.Size.Height);
                if (longSide < LongSideThreshold + 2.0f)
                {
                    continue;
                }
                if (shortSide < ShortSideThreshold + 2.0f)
                {
                    continue;
                }

                PaddleContour paddleContour = new PaddleContour(expandedContour.ToArray(), confidence);
                paddleContours.Add(paddleContour);
            }

            return paddleContours.ToArray();
        }
        #endregion
    }
}
