using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;

namespace SD.OpenCV.OnnxRuntime.Base
{
    /// <summary>
    /// ONNX模型基类
    /// </summary>
    public abstract class OnnxModel<TInput, TOutput> : IOnnxModel<TInput, TOutput>
    {
        #region # 字段及构造器

        /// <summary>
        /// 推理会话
        /// </summary>
        private InferenceSession _session;

        /// <summary>
        /// 模型字节数组
        /// </summary>
        private readonly byte[] _modelBytes;

        /// <summary>
        /// 会话选项
        /// </summary>
        private readonly SessionOptions _sessionOptions;

        /// <summary>
        /// 创建ONNX模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        protected OnnxModel(byte[] modelBytes, SessionOptions sessionOptions)
        {
            this._modelBytes = modelBytes;
            this._sessionOptions = sessionOptions;
        }

        #endregion


        //Implements

        #region # 启动会话 —— void StartSession()
        /// <summary>
        /// 启动会话
        /// </summary>
        public void StartSession()
        {
            #region # 验证

            if (this._session != null)
            {
                throw new InvalidOperationException("推理会话已启动！");
            }

            #endregion

            this._session = new InferenceSession(this._modelBytes, this._sessionOptions);

        }
        #endregion

        #region # 运行推理 —— TOutput Infer(TInput input, float minConfidence)
        /// <summary>
        /// 运行推理
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        public TOutput Infer(TInput input, float minConfidence = 0.5f)
        {
            #region # 验证

            if (this._session == null)
            {
                throw new InvalidOperationException("推理会话未启动!");
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input), "推理输入不可为空！");
            }

            #endregion

            List<NamedOnnxValue> onnxValues = this.ProcessInput(input);
            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> inference = this._session.Run(onnxValues);
            TOutput output = this.ProcessInference(inference, minConfidence);

            //释放资源
            foreach (DisposableNamedOnnxValue disposable in inference)
            {
                disposable.Dispose();
            }
            inference.Dispose();

            return output;
        }
        #endregion

        #region # 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._session?.Dispose();
            this._session = null;
        }
        #endregion


        //abstract

        #region # 处理推理输入 —— abstract List<NamedOnnxValue> ProcessInput(TInput input)
        /// <summary>
        /// 处理推理输入
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <returns>ONNX键值列表</returns>
        protected abstract List<NamedOnnxValue> ProcessInput(TInput input);
        #endregion

        #region # 处理推理结果 —— abstract TOutput ProcessInference(IReadOnlyList...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected abstract TOutput ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence);
        #endregion
    }
}
