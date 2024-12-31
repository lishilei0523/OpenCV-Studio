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
    /// Paddle文本分类模型
    /// </summary>
    public class PaddleClassifier : OnnxModel<Mat, PaddleTextDirection>
    {
        #region # 字段及构造器

        /// <summary>
        /// 输入图像宽度
        /// </summary>
        private const int InputWidth = 192;

        /// <summary>
        /// 输入图像高度
        /// </summary>
        private const int InputHeight = 48;

        /// <summary>
        /// 创建Paddle文本分类模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public PaddleClassifier(string modelPath)
            : this(modelPath, new SessionOptions())
        {
        }

        /// <summary>
        /// 创建Paddle文本分类模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public PaddleClassifier(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Paddle文本分类模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleClassifier(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建Paddle文本分类模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleClassifier(byte[] modelBytes, SessionOptions sessionOptions)
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
            //缩放图像
            float whRatio = input.Width * 1.0f / input.Height;
            int resizedWidth = (int)Math.Ceiling(InputHeight * whRatio);
            if (resizedWidth > InputWidth)
            {
                resizedWidth = InputWidth;
            }
            using Mat resizedImage = input.ResizeAdaptively(resizedWidth, InputHeight, out _, out _, out _);

            DenseTensor<float> sourceTensor = new DenseTensor<float>([1, 3, InputHeight, InputWidth]);//[批次数, 通道数, 图像高, 图像宽]
            resizedImage.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b pixel = *valuePtr;

                //按模型要求标准化处理像素
                sourceTensor[0, 0, rowIndex, colIndex] = (pixel[0] / 255f - 0.5f) / 0.5f;
                sourceTensor[0, 1, rowIndex, colIndex] = (pixel[1] / 255f - 0.5f) / 0.5f;
                sourceTensor[0, 2, rowIndex, colIndex] = (pixel[2] / 255f - 0.5f) / 0.5f;
            });

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("x", sourceTensor)
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override PaddleTextDirection ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override PaddleTextDirection ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            float[] results = inference[0].AsTensor<float>().ToArray();

            if (results[0] > results[1])
            {
                return PaddleTextDirection.Up;
            }

            return PaddleTextDirection.Down;
        }
        #endregion
    }
}
