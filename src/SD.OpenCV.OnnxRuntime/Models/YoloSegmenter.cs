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

            //定义Sigmoid函数
            Func<float, float> sigmoid = a =>
            {
                float b = 1.0f / (1.0f + (float)Math.Exp(-a));
                return b;
            };

            //解析目标检测部分
            using Mat detMat = Mat.FromArray<float>(results0);
            using Mat reshapedDetMat = detMat.Reshape(1, results0.Dimensions[1]);
            using Mat transposedDetMat = reshapedDetMat.Transpose();
            IList<Detection> detections = new List<Detection>();
            IList<Mat> masks = new List<Mat>();
            for (int rowIndex = 0; rowIndex < transposedDetMat.Rows; rowIndex++)
            {
                using Mat rowMat = transposedDetMat[rowIndex, rowIndex + 1, 0, transposedDetMat.Cols];
                rowMat.GetArray(out float[] row);

                float[] boxArray = new float[4];
                float[] confidencesArray = new float[80];
                Array.Copy(row, 0, boxArray, 0, boxArray.Length);
                Array.Copy(row, 4, confidencesArray, 0, confidencesArray.Length);
                float maxConfidence = confidencesArray.Max();//获取置信度最大值
                if (maxConfidence >= minConfidence)
                {
                    Mat mask = transposedDetMat.Row(rowIndex).ColRange(84, transposedDetMat.Cols);
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
                    masks.Add(mask);
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

                //分割结果
                using Mat originalMask = masks[index] * reshapedSegMat;
                for (int col = 0; col < originalMask.Cols; col++)
                {
                    originalMask.At<float>(0, col) = sigmoid(originalMask.At<float>(0, col));
                }
                using Mat reshapedMask = originalMask.Reshape(1, 160);

                //裁剪分割区域
                int boxXMin = Math.Max(0, detection.Box.X);
                int boxYMin = Math.Max(0, detection.Box.Y);
                int boxXMax = Math.Max(0, detection.Box.BottomRight.X);
                int boxYMax = Math.Max(0, detection.Box.BottomRight.Y);
                int maskXMin = (int)Math.Ceiling((boxXMin * 1.0f / scaleX + this._paddingX) * 0.25f);
                int maskXMax = (int)Math.Ceiling((boxXMax * 1.0f / scaleX + this._paddingX) * 0.25f);
                int maskYMin = (int)Math.Ceiling((boxYMin * 1.0f / scaleY + this._paddingY) * 0.25f);
                int maskYMax = (int)Math.Ceiling((boxYMax * 1.0f / scaleY + this._paddingY) * 0.25f);
                using Mat rangedMask = new Mat(reshapedMask, new Range(maskYMin, maskYMax), new Range(maskXMin, maskXMax));

                //将分割区域转换到检测框大小
                using Mat resizedMask = rangedMask.Resize(detection.Box.Size);

                //二值化分割区域
                resizedMask.ForEachAsFloat((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    float pixel = *valuePtr;
                    if (pixel > 0.5)
                    {
                        resizedMask.At<float>(rowIndex, colIndex) = 255f;
                    }
                    else
                    {
                        resizedMask.At<float>(rowIndex, colIndex) = 0f;
                    }
                });

                //格式转换
                using Mat byteMask = new Mat();
                resizedMask.ConvertTo(byteMask, MatType.CV_8UC1);

                //修整box
                if ((boxXMin + byteMask.Width) >= this._sourceSize.Width)
                {
                    boxXMax = this._sourceSize.Width - 1;
                }
                if ((boxYMin + byteMask.Height) >= this._sourceSize.Height)
                {
                    boxYMax = this._sourceSize.Height - 1;
                }
                Rect reBox = new Rect(boxXMin, boxYMin, boxXMax - boxXMin, boxYMax - boxYMin);

                //获取分割区域
                using Mat canvas = Mat.Zeros(this._sourceSize, MatType.CV_8UC1);
                using Mat canvasRoi = new Mat(canvas, reBox);
                using Mat rangedByteMask = new Mat(byteMask, new Range(0, reBox.Height), new Range(0, reBox.Width));
                rangedByteMask.CopyTo(canvasRoi);

                //查找轮廓
                Cv2.FindContours(canvas, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);
                if (contours.Any())
                {
                    Point[] contour = contours.OrderByDescending(contour => Cv2.ArcLength(contour, false)).First();
                    Segmentation segmentation = new Segmentation(detection.Label, reBox, contour, detection.Confidence);
                    segmentations.Add(segmentation);
                }
            }

            //释放资源
            foreach (Mat mask in masks)
            {
                mask.Dispose();
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
