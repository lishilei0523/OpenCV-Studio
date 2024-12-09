namespace SD.OpenCV.OnnxRuntime.Values
{
    /// <summary>
    /// 预测结果
    /// </summary>
    public struct Prediction
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
        public string Label { get; private set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public float Confidence { get; private set; }
    }
}
