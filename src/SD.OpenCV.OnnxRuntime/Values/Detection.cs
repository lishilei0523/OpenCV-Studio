using OpenCvSharp;

namespace SD.OpenCV.OnnxRuntime.Values
{
    /// <summary>
    /// 检测结果
    /// </summary>
    public struct Detection
    {
        /// <summary>
        /// 创建检测结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="box">矩形框</param>
        /// <param name="confidence">置信度</param>
        public Detection(string label, Rect box, float confidence)
            : this()
        {
            this.Label = label;
            this.Box = box;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// 矩形框
        /// </summary>
        public Rect Box { get; private set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public float Confidence { get; private set; }
    }
}
