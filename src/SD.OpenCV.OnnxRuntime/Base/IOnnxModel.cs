using System;

namespace SD.OpenCV.OnnxRuntime.Base
{
    /// <summary>
    /// ONNX模型基接口
    /// </summary>
    public interface IOnnxModel<in TInput, out TOutput> : IDisposable
    {
        /// <summary>
        /// 启动会话
        /// </summary>
        void StartSession();

        /// <summary>
        /// 运行推理
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        TOutput Infer(TInput input, float minConfidence = 0.5f);
    }
}
