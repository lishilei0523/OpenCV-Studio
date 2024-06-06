using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.Toolkits.OpenCV.Reconstructions
{
    /// <summary>
    /// Super张量匹配器
    /// </summary>
    /// <remarks>LightGlue描述子匹配器</remarks>
    public class SuperMatcher : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认ONNX模型路径
        /// </summary>
        private const string DefaultOnnxPath = "Content/superpoint_lightglue.onnx";

        /// <summary>
        /// ONNX模型路径
        /// </summary>
        private readonly string _onnxPath;

        /// <summary>
        /// SuperPoint会话
        /// </summary>
        private readonly InferenceSession _lightGlue;

        /// <summary>
        /// 创建Super张量匹配器构造器
        /// </summary>
        public SuperMatcher()
        {
            this._onnxPath = DefaultOnnxPath;
            this._lightGlue = new InferenceSession(this._onnxPath);
        }

        /// <summary>
        /// 创建Super张量匹配器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        public SuperMatcher(string onnxPath)
        {
            this._onnxPath = onnxPath;
            this._lightGlue = new InferenceSession(this._onnxPath);
        }

        /// <summary>
        /// 创建Super张量匹配器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        /// <param name="sessionOptions">会话设置</param>
        public SuperMatcher(string onnxPath, SessionOptions sessionOptions)
        {
            this._onnxPath = onnxPath;
            this._lightGlue = new InferenceSession(this._onnxPath, sessionOptions);
        }

        /// <summary>
        /// 创建Super张量匹配器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        /// <param name="sessionOptions">会话设置</param>
        /// <param name="prepackedWeightsContainer">预打包权重容器</param>
        public SuperMatcher(string onnxPath, SessionOptions sessionOptions, PrePackedWeightsContainer prepackedWeightsContainer)
        {
            this._onnxPath = onnxPath;
            this._lightGlue = new InferenceSession(this._onnxPath, sessionOptions, prepackedWeightsContainer);
        }

        #endregion

        #region # 方法

        #region 匹配张量 —— DMatch[] Match(float threshold, long[] sourceKptsArray...
        /// <summary>
        /// 匹配张量
        /// </summary>
        /// <param name="threshold">打分阈值</param>
        /// <param name="sourceKptsArray">原关键点张量数组</param>
        /// <param name="sourceKptsDims">原关键点维度数组</param>
        /// <param name="sourceDescArray">原描述子张量数组</param>
        /// <param name="sourceDescDims">原描述子维度数组</param>
        /// <param name="targetKptsArray">目标关键点张量数组</param>
        /// <param name="targetKptsDims">目标关键点维度数组</param>
        /// <param name="targetDescArray">目标描述子张量数组</param>
        /// <param name="targetDescDims">目标描述子维度数组</param>
        /// <returns>匹配结果列表</returns>
        public DMatch[] Match(float threshold, long[] sourceKptsArray, int[] sourceKptsDims, float[] sourceDescArray, int[] sourceDescDims, long[] targetKptsArray, int[] targetKptsDims, float[] targetDescArray, int[] targetDescDims)
        {
            //适用特征工程
            float[] sourceKptsFeatures = SuperEngineer.GetKeyPointsFeatures(sourceKptsArray);
            float[] targetKptsFeatures = SuperEngineer.GetKeyPointsFeatures(targetKptsArray);

            //定义输入张量
            DenseTensor<float> kpts0 = new DenseTensor<float>(sourceKptsFeatures, sourceKptsDims);
            DenseTensor<float> kpts1 = new DenseTensor<float>(targetKptsFeatures, targetKptsDims);
            DenseTensor<float> desc0 = new DenseTensor<float>(sourceDescArray, sourceDescDims);
            DenseTensor<float> desc1 = new DenseTensor<float>(targetDescArray, targetDescDims);
            List<NamedOnnxValue> namedOnnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(nameof(kpts0), kpts0),
                NamedOnnxValue.CreateFromTensor(nameof(kpts1), kpts1),
                NamedOnnxValue.CreateFromTensor(nameof(desc0), desc0),
                NamedOnnxValue.CreateFromTensor(nameof(desc1), desc1)
            };

            //运行推理
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> inferResults = this._lightGlue.Run(namedOnnxValues);
            Tensor<long> matches0Tensor = inferResults.Single(x => x.Name == "matches0").AsTensor<long>();
            Tensor<long> matches1Tensor = inferResults.Single(x => x.Name == "matches1").AsTensor<long>();
            Tensor<float> scores0Tensor = inferResults.Single(x => x.Name == "mscores0").AsTensor<float>();
            Tensor<float> scores1Tensor = inferResults.Single(x => x.Name == "mscores1").AsTensor<float>();

            //解析推理结果
            IList<long> matches0 = matches0Tensor.ToList();
            IList<long> matches1 = matches1Tensor.ToList();
            IList<float> scores0 = scores0Tensor.ToList();
            IList<float> scores1 = scores1Tensor.ToList();
            ICollection<DMatch> matches = new HashSet<DMatch>();
            for (int index = 0; index < matches0.Count; index++)
            {
                if (matches0[index] > -1 && scores0[index] > threshold && matches1[(int)matches0[index]] == index)
                {
                    DMatch match = new DMatch(index, (int)matches0[index], 0);
                    matches.Add(match);
                }
            }
            for (int index = 0; index < matches1.Count; index++)
            {
                if (matches1[index] > -1 && scores1[index] > threshold && matches0[(int)matches1[index]] == index)
                {
                    DMatch match = new DMatch((int)matches1[index], index, 0);
                    matches.Add(match);
                }
            }

            //释放资源
            inferResults.Dispose();

            return matches.ToArray();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._lightGlue?.Dispose();
        }
        #endregion 

        #endregion
    }
}
