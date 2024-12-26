using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Base;
using SD.OpenCV.OnnxRuntime.Values;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SD.OpenCV.OnnxRuntime.Models
{
    /// <summary>
    /// ResNet模型
    /// </summary>
    public class ResNet : OnnxModel<Mat, Prediction[]>
    {
        #region # 字段及构造器

        /// <summary>
        /// 调整后图像边长
        /// </summary>
        private const int SideSize = 224;

        /// <summary>
        /// 创建ResNet模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        public ResNet(string modelPath)
            : this(modelPath, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建ResNet模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        public ResNet(byte[] modelBytes)
            : this(modelBytes, new SessionOptions())
        {

        }

        /// <summary>
        /// 创建ResNet模型构造器
        /// </summary>
        /// <param name="modelPath">模型路径</param>
        /// <param name="sessionOptions">会话选项</param>
        public ResNet(string modelPath, SessionOptions sessionOptions)
            : this(File.ReadAllBytes(modelPath), sessionOptions)
        {

        }

        /// <summary>
        /// 创建ResNet模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public ResNet(byte[] modelBytes, SessionOptions sessionOptions)
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
            //缩放图像 224*224
            using Mat resizedImage = input.Resize(new Size(SideSize, SideSize));

            //此为ResNet预处理时的均值、标准差
            float[] mean = [0.485f, 0.456f, 0.406f];
            float[] stddev = [0.229f, 0.224f, 0.225f];

            //[批次数, 通道数, 图像高, 图像宽]
            DenseTensor<float> sourceTensor = new DenseTensor<float>([1, 3, SideSize, SideSize]);
            resizedImage.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b pixel = *valuePtr;

                //按模型要求标准化处理像素
                sourceTensor[0, 0, rowIndex, colIndex] = ((pixel[2] / 255f) - mean[0]) / stddev[0];
                sourceTensor[0, 1, rowIndex, colIndex] = ((pixel[1] / 255f) - mean[1]) / stddev[1];
                sourceTensor[0, 2, rowIndex, colIndex] = ((pixel[0] / 255f) - mean[2]) / stddev[2];
            });

            List<NamedOnnxValue> onnxValues = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("data", sourceTensor)
            };

            return onnxValues;
        }
        #endregion

        #region # 处理推理结果 —— override Prediction[] ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override Prediction[] ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            Tensor<float> outputs = inference[0].AsTensor<float>();

            //定义softmax函数
            double sum = outputs.Sum(output => Math.Exp(output));
            Func<float, float> softmax = output => (float)(Math.Exp(output) / sum);

            //读取分类文本
            string[] labels = this.GetLabels();

            //整理前10结果
            Prediction[] predictions = outputs
                .Select((output, index) => new { Label = labels[index], Confidence = softmax(output) })
                .Where(x => x.Confidence >= minConfidence)
                .OrderByDescending(x => x.Confidence)
                .Take(10)
                .Select(x => new Prediction(x.Label, x.Confidence))
                .ToArray();

            return predictions;
        }
        #endregion

        #region # 获取标签列表 —— virtual string[] GetLabels()
        /// <summary>
        /// 获取标签列表
        /// </summary>
        /// <returns>标签列表</returns>
        protected virtual string[] GetLabels()
        {
            string[] labels = AppDomain.CurrentDomain.GetData(typeof(ResNet).FullName!) as string[];
            if (labels == null)
            {
                labels = File.ReadAllLines("Content/Labels/image_net_labels.txt");
            }

            return labels;
        }
        #endregion 
    }
}
