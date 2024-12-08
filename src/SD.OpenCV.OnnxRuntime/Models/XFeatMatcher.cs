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
    /// XFeat匹配模型
    /// </summary>
    public class XFeatMatcher : OnnxModel<(Feature[], Feature[]), DMatch[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 源特征列表
        /// </summary>
        private Feature[] _sourceFeatures;

        /// <summary>
        /// 目标特征列表
        /// </summary>
        private Feature[] _targetFeatures;

        /// <summary>
        /// 创建XFeat匹配模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public XFeatMatcher(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建XFeat匹配模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public XFeatMatcher(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建XFeat匹配模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public XFeatMatcher(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建XFeat匹配模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public XFeatMatcher(byte[] modelBytes, SessionOptions sessionOptions)
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
            this._sourceFeatures = input.Item1;
            this._targetFeatures = input.Item2;

            DenseTensor<float> kpts0Tensor = new DenseTensor<float>([input.Item1.Length, 2]);
            DenseTensor<float> kpts1Tensor = new DenseTensor<float>([input.Item2.Length, 2]);
            DenseTensor<float> desc0Tensor = new DenseTensor<float>([input.Item1.Length, 64]);
            DenseTensor<float> desc1Tensor = new DenseTensor<float>([input.Item2.Length, 64]);
            for (int index = 0; index < input.Item1.Length; index++)
            {
                Feature feature = input.Item1[index];

                //关键点
                kpts0Tensor[index, 0] = feature.OriginalKeyPoint.X;
                kpts0Tensor[index, 1] = feature.OriginalKeyPoint.Y;

                //描述子
                for (int i = 0; i < feature.Descriptor.Length; i++)
                {
                    desc0Tensor[index, i] = feature.Descriptor[i];
                }
            }
            for (int index = 0; index < input.Item2.Length; index++)
            {
                Feature feature = input.Item2[index];

                //关键点
                kpts1Tensor[index, 0] = feature.OriginalKeyPoint.X;
                kpts1Tensor[index, 1] = feature.OriginalKeyPoint.Y;

                //描述子
                for (int i = 0; i < feature.Descriptor.Length; i++)
                {
                    desc1Tensor[index, i] = feature.Descriptor[i];
                }
            }

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("kpts0", kpts0Tensor),
                NamedOnnxValue.CreateFromTensor("feats0", desc0Tensor),
                NamedOnnxValue.CreateFromTensor("kpts1", kpts1Tensor),
                NamedOnnxValue.CreateFromTensor("feats1", desc1Tensor),
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
            Tensor<float> matches0Tensor = inference[0].AsTensor<float>();
            Tensor<float> matches1Tensor = inference[1].AsTensor<float>();

            //解析推理结果
            using Mat rawMatch0Mat = Mat.FromArray(matches0Tensor);
            using Mat rawMatch1Mat = Mat.FromArray(matches1Tensor);
            using Mat match0Mat = rawMatch0Mat.Reshape(1, matches0Tensor.Dimensions[0]);
            using Mat match1Mat = rawMatch1Mat.Reshape(1, matches1Tensor.Dimensions[0]);

            IList<Point> originalKeyPoints0 = this._sourceFeatures.Select(x => x.OriginalKeyPoint).ToList();
            IList<Point> originalKeyPoints1 = this._targetFeatures.Select(x => x.OriginalKeyPoint).ToList();
            IList<DMatch> dMatches = new List<DMatch>();
            for (int rowIndex = 0; rowIndex < match0Mat.Rows; rowIndex++)
            {
                using Mat row0Mat = match0Mat[rowIndex, rowIndex + 1, 0, match0Mat.Cols];
                using Mat row1Mat = match1Mat[rowIndex, rowIndex + 1, 0, match1Mat.Cols];
                row0Mat.GetArray(out float[] row0);
                row1Mat.GetArray(out float[] row1);

                Point keyPoint0 = new Point(row0[0], row0[1]);
                Point keyPoint1 = new Point(row1[0], row1[1]);
                int index0 = originalKeyPoints0.IndexOf(keyPoint0);
                int index1 = originalKeyPoints1.IndexOf(keyPoint1);
                DMatch match = new DMatch(index0, index1, 0);
                dMatches.Add(match);
            }

            return dMatches.ToArray();
        }
        #endregion
    }
}
