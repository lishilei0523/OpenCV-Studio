using System.Runtime.InteropServices;

namespace SD.OpenCV.OnnxRuntime.Values
{
    /// <summary>
    /// 预测结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Prediction
    {
        /// <summary>
        /// 创建预测结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="confidence">置信度</param>
        public Prediction(string label, float confidence)
            : this()
        {
            this.Label = label;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// 置信度
        /// </summary>
        public readonly float Confidence;
    }
}
