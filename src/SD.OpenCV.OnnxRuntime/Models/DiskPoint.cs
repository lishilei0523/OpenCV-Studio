using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Results;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// Disk模型
    /// </summary>
    public class DiskPoint : OnnxModel<Mat, Feature[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 调整后图像边长
        /// </summary>
        private const int SideSize = 512;

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
        /// 创建Disk模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public DiskPoint(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Disk模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public DiskPoint(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Disk模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public DiskPoint(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建Disk模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public DiskPoint(byte[] modelBytes, SessionOptions sessionOptions)
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
            using Mat resizedImage = input.ResizeAdaptively(SideSize, out this._adaptiveSize, out this._paddingX, out this._paddingY);

            DenseTensor<float> sourceTensor = new DenseTensor<float>([1, 3, SideSize, SideSize]);//[批次数, 通道数, 图像高, 图像宽]
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
                NamedOnnxValue.CreateFromTensor("image", sourceTensor)
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override Feature[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override Feature[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<long> keyPointsTensor = inference[0].AsTensor<long>();
            Tensor<float> scoresTensor = inference[1].AsTensor<float>();
            Tensor<float> descriptorsTensor = inference[2].AsTensor<float>();

            //计算缩放系数
            float scaleX = this._sourceSize.Width * 1.0f / this._adaptiveSize.Width;
            float scaleY = this._sourceSize.Height * 1.0f / this._adaptiveSize.Height;

            //解析推理结果
            using Mat rawKeyPoints = Mat.FromArray(keyPointsTensor.Select(x => (int)x));
            using Mat rawDescriptors = Mat.FromArray(descriptorsTensor);
            using Mat keyPoints = rawKeyPoints.Reshape(1, keyPointsTensor.Dimensions[1]);
            using Mat descriptors = rawDescriptors.Reshape(1, descriptorsTensor.Dimensions[1]);
            IList<Feature> features = new List<Feature>();
            for (int rowIndex = 0; rowIndex < keyPoints.Rows; rowIndex++)
            {
                float confidence = scoresTensor[0, rowIndex];
                if (confidence >= minConfidence)
                {
                    using Mat rowKeyPointMat = keyPoints[rowIndex, rowIndex + 1, 0, keyPoints.Cols];
                    using Mat rowDescriptorMat = descriptors[rowIndex, rowIndex + 1, 0, descriptors.Cols];
                    rowKeyPointMat.GetArray(out int[] rowKeyPoint);
                    rowDescriptorMat.GetArray(out float[] rowDescriptor);

                    float scaledX = (rowKeyPoint[0] - this._paddingX) * scaleX;
                    float scaledY = (rowKeyPoint[1] - this._paddingY) * scaleY;
                    if (scaledX > 0 && scaledY > 0)
                    {
                        Point originalKeyPoint = new Point(rowKeyPoint[0], rowKeyPoint[1]);
                        Point scaledKeyPoint = new Point(scaledX, scaledY);
                        Feature feature = new Feature(originalKeyPoint, scaledKeyPoint, rowDescriptor, confidence);
                        features.Add(feature);
                    }
                }
            }

            return features.ToArray();
        }
        #endregion
    }
}
