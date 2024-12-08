using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// Paddle文本识别模型
    /// </summary>
    public class PaddleRecognizer : OnnxModel<Mat, string>
    {
        #region # 字段及构造器

        /// <summary>
        /// 输入图像宽度
        /// </summary>
        private const int InputWidth = 320;

        /// <summary>
        /// 输入图像高度
        /// </summary>
        private const int InputHeight = 48;

        /// <summary>
        /// 创建Paddle文本识别模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public PaddleRecognizer(string modelPath)
            : this(modelPath, new SessionOptions())
        {
        }

        /// <summary>
        /// 创建Paddle文本识别模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public PaddleRecognizer(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Paddle文本识别模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleRecognizer(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建Paddle文本识别模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public PaddleRecognizer(byte[] modelBytes, SessionOptions sessionOptions)
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

        #region # 处理推理结果 —— override string ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override string ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<float> results0 = inference[0].AsTensor<float>();

            //读取分类文本
            string[] labels = this.GetLabels();

            using Mat rawMat = Mat.FromArray(results0);
            using Mat reshapedMat = rawMat.Reshape(1, results0.Dimensions[1]);
            int[] labelIndices = new int[reshapedMat.Rows];
            for (int rowIndex = 0; rowIndex < reshapedMat.Rows; rowIndex++)
            {
                using Mat rowMat = reshapedMat[rowIndex, rowIndex + 1, 0, reshapedMat.Cols];
                rowMat.GetArray(out float[] row);

                float maxConfidence = row.Max();
                if (maxConfidence >= minConfidence)
                {
                    int maxConfidenceIndex = Array.IndexOf(row, maxConfidence);
                    labelIndices[rowIndex] = maxConfidenceIndex;
                }
            }

            StringBuilder stringBuilder = new StringBuilder();
            foreach (int index in labelIndices.Where(x => x > 0))
            {
                stringBuilder.Append(labels[index - 1]);
            }

            return stringBuilder.ToString();
        }
        #endregion

        #region # 获取标签列表 —— string[] GetLabels()
        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <returns>标签列表</returns>
        private string[] GetLabels()
        {
            string[] lines = File.ReadAllLines("Assets/Labels/paddle_ocr_labels.txt");
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
