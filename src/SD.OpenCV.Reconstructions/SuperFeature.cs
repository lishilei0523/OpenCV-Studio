using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.Toolkits.OpenCV.Reconstructions
{
    /// <summary>
    /// Super特征提取器
    /// </summary>
    /// <remarks>SuperPoint特征提取器</remarks>
    public class SuperFeature : Feature2D
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认ONNX模型路径
        /// </summary>
        private const string DefaultOnnxPath = "Content/superpoint.onnx";

        /// <summary>
        /// ONNX模型路径
        /// </summary>
        private readonly string _onnxPath;

        /// <summary>
        /// SuperPoint会话
        /// </summary>
        private readonly InferenceSession _superpoint;

        /// <summary>
        /// 创建Super特征提取器构造器
        /// </summary>
        public SuperFeature()
        {
            this._onnxPath = DefaultOnnxPath;
            this._superpoint = new InferenceSession(this._onnxPath);
        }

        /// <summary>
        /// 创建Super特征提取器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        public SuperFeature(string onnxPath)
        {
            this._onnxPath = onnxPath;
            this._superpoint = new InferenceSession(this._onnxPath);
        }

        /// <summary>
        /// 创建Super特征提取器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        /// <param name="sessionOptions">会话设置</param>
        public SuperFeature(string onnxPath, SessionOptions sessionOptions)
        {
            this._onnxPath = onnxPath;
            this._superpoint = new InferenceSession(this._onnxPath, sessionOptions);
        }

        /// <summary>
        /// 创建Super特征提取器构造器
        /// </summary>
        /// <param name="onnxPath">ONNX模型路径</param>
        /// <param name="sessionOptions">会话设置</param>
        /// <param name="prepackedWeightsContainer">预打包权重容器</param>
        public SuperFeature(string onnxPath, SessionOptions sessionOptions, PrePackedWeightsContainer prepackedWeightsContainer)
        {
            this._onnxPath = onnxPath;
            this._superpoint = new InferenceSession(this._onnxPath, sessionOptions, prepackedWeightsContainer);
        }

        #endregion

        #region # 方法

        #region 计算张量、关键点与描述子 —— void ComputeAll(Mat image, Mat mask, out long[] keyPointsArray...
        /// <summary>
        /// 计算张量、关键点与描述子
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="mask">掩膜</param>
        /// <param name="keyPointsArray">关键点张量数组</param>
        /// <param name="keyPointsDims">关键点维度数组</param>
        /// <param name="descriptorsArray">描述子张量数组</param>
        /// <param name="descriptorsDims">描述子维度数组</param>
        /// <param name="keyPoints">关键点列表</param>
        /// <param name="descriptors">描述子列表</param>
        public void ComputeAll(Mat image, Mat mask, out long[] keyPointsArray, out int[] keyPointsDims, out float[] descriptorsArray, out int[] descriptorsDims, out KeyPoint[] keyPoints, out Mat descriptors)
        {
            bool needDispose;
            Mat grayImage;

            #region # 验证

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "图像不可为空！");
            }
            if (image.Type() == MatType.CV_8UC1)
            {
                grayImage = image;
                needDispose = false;
            }
            else
            {
                grayImage = new Mat();
                Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
                needDispose = true;
            }
            if (mask != null)
            {
                Mat result = new Mat();
                grayImage.CopyTo(result, mask);
                grayImage.Dispose();
                grayImage = result;
                needDispose = true;
            }

            #endregion

            //特征工程
            float[] imageFeatures = grayImage.GetImageFeatures();

            //定义输入张量
            int[] sourceDims = { 1, 1, grayImage.Width, grayImage.Height };
            DenseTensor<float> sourceTensor = new DenseTensor<float>(imageFeatures, sourceDims);
            List<NamedOnnxValue> namedOnnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("image", sourceTensor)
            };

            //运行推理
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> inferResults = this._superpoint.Run(namedOnnxValues);
            Tensor<long> keyPointsTensor = inferResults.Single(o => o.Name == "keypoints").AsTensor<long>();
            Tensor<float> descriptorsTensor = inferResults.Single(x => x.Name == "descriptors").AsTensor<float>();
            Tensor<float> scoresTensor = inferResults.Single(x => x.Name == "scores").AsTensor<float>();

            //关键点
            keyPointsArray = keyPointsTensor.ToArray();
            keyPointsDims = keyPointsTensor.Dimensions.ToArray();

            //描述子
            descriptorsArray = descriptorsTensor.ToArray();
            descriptorsDims = descriptorsTensor.Dimensions.ToArray();

            //解析关键点
            keyPoints = new KeyPoint[keyPointsDims[1]];
            for (int index = 0; index < keyPointsDims[1]; index++)
            {
                KeyPoint keyPoint = new KeyPoint();
                int i = index * 2;
                keyPoint.Pt.X = keyPointsArray[i];
                keyPoint.Pt.Y = keyPointsArray[i + 1];
                keyPoints[index] = keyPoint;
            }

            //解析描述子
            descriptors = new Mat();
            descriptors.Create(new Size(descriptorsDims[2], descriptorsDims[1]), MatType.CV_32FC1);
            for (int rowIndex = 0; rowIndex < descriptorsDims[1]; rowIndex++)
            {
                for (int colIndex = 0; colIndex < descriptorsDims[2]; colIndex++)
                {
                    int index = rowIndex * descriptorsDims[2] + colIndex;
                    descriptors.At<float>(rowIndex, colIndex) = descriptorsArray[index];
                }
            }

            //释放资源
            inferResults.Dispose();
            if (needDispose)
            {
                grayImage.Dispose();
            }
        }
        #endregion

        #region 检测关键点和计算描述子 —— override void DetectAndCompute(InputArray image, InputArray mask...
        /// <summary>
        /// 检测关键点和计算描述子
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="mask">掩膜</param>
        /// <param name="keyPoints">关键点列表</param>
        /// <param name="descriptors">描述子列表</param>
        /// <param name="useProvidedKeypoints"></param>
        public override void DetectAndCompute(InputArray image, InputArray mask, out KeyPoint[] keyPoints, OutputArray descriptors, bool useProvidedKeypoints = false)
        {
            //计算
            this.ComputeAll(image.GetMat(), mask?.GetMat(), out long[] keyPointsArray, out int[] keyPointsDims, out float[] descriptorsArray, out int[] descriptorsDims, out keyPoints, out Mat descriptorsMat);

            descriptorsMat.CopyTo(descriptors);
            descriptorsMat.Dispose();
        }
        #endregion

        #region 释放资源 —— override void Dispose(bool disposing)
        /// <summary>
        /// 释放资源
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this._superpoint?.Dispose();
            base.Dispose(disposing);
        }
        #endregion 

        #endregion
    }
}
