using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Values;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// Disk-LightGlue模型
    /// </summary>
    public class DiskLightGlue : OnnxModel<(Feature[], Feature[]), DMatch[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建Disk-LightGlue模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public DiskLightGlue(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Disk-LightGlue模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public DiskLightGlue(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建Disk-LightGlue模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public DiskLightGlue(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建Disk-LightGlue模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public DiskLightGlue(byte[] modelBytes, SessionOptions sessionOptions)
            : base(modelBytes, sessionOptions)
        {

        }

        #endregion

        #region # 处理推理输入 —— override List<NamedOnnxValue> ProcessInput((Feature[], Feature[]) input)
        /// <summary>
        /// 处理推理输入
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <returns>ONNX键值列表</returns>
        protected override unsafe List<NamedOnnxValue> ProcessInput((Feature[], Feature[]) input)
        {
            DenseTensor<float> kpts0Tensor = new DenseTensor<float>([1, input.Item1.Length, 2]);
            DenseTensor<float> kpts1Tensor = new DenseTensor<float>([1, input.Item2.Length, 2]);
            DenseTensor<float> desc0Tensor = new DenseTensor<float>([1, input.Item1.Length, 128]);
            DenseTensor<float> desc1Tensor = new DenseTensor<float>([1, input.Item2.Length, 128]);
            for (int index = 0; index < input.Item1.Length; index++)
            {
                Feature feature = input.Item1[index];

                //关键点
                kpts0Tensor[0, index, 0] = (feature.OriginalKeyPoint.X - 256f) / 256f;
                kpts0Tensor[0, index, 1] = (feature.OriginalKeyPoint.Y - 256f) / 256f;

                //描述子
                for (int i = 0; i < feature.Descriptor.Length; i++)
                {
                    desc0Tensor[0, index, i] = feature.Descriptor[i];
                }
            }
            for (int index = 0; index < input.Item2.Length; index++)
            {
                Feature feature = input.Item2[index];

                //关键点
                kpts1Tensor[0, index, 0] = (feature.OriginalKeyPoint.X - 256f) / 256f;
                kpts1Tensor[0, index, 1] = (feature.OriginalKeyPoint.Y - 256f) / 256f;

                //描述子
                for (int i = 0; i < feature.Descriptor.Length; i++)
                {
                    desc1Tensor[0, index, i] = feature.Descriptor[i];
                }
            }

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("kpts0", kpts0Tensor),
                NamedOnnxValue.CreateFromTensor("kpts1", kpts1Tensor),
                NamedOnnxValue.CreateFromTensor("desc0", desc0Tensor),
                NamedOnnxValue.CreateFromTensor("desc1", desc1Tensor),
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override DMatch[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override DMatch[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<long> matches0Tensor = inference[0].AsTensor<long>();
            Tensor<float> mscores0Tensor = inference[1].AsTensor<float>();

            //解析推理结果
            using Mat rawMatchMat = Mat.FromArray(matches0Tensor.Select(x => (int)x));
            using Mat matchMat = rawMatchMat.Reshape(1, matches0Tensor.Dimensions[0]);
            IList<DMatch> dMatches = new List<DMatch>();
            for (int rowIndex = 0; rowIndex < matchMat.Rows; rowIndex++)
            {
                using Mat rowMat = matchMat[rowIndex, rowIndex + 1, 0, matchMat.Cols];
                rowMat.GetArray(out int[] row);

                float score = mscores0Tensor[rowIndex];
                DMatch match = new DMatch(row[0], row[1], score);
                dMatches.Add(match);
            }

            return dMatches.ToArray();
        }
        #endregion
    }
}
