using OpenCvSharp;
using System.Runtime.InteropServices;

namespace SD.OpenCV.OnnxRuntime.Results
{
    /// <summary>
    /// 特征
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Feature
    {
        /// <summary>
        /// 创建特征构造器
        /// </summary>
        /// <param name="originalKeyPoint">原始关键点</param>
        /// <param name="scaledKeyPoint">缩放关键点</param>
        /// <param name="descriptor">描述子</param>
        /// <param name="confidence">置信度</param>
        public Feature(Point originalKeyPoint, Point scaledKeyPoint, float[] descriptor, float confidence)
            : this()
        {
            this.OriginalKeyPoint = originalKeyPoint;
            this.ScaledKeyPoint = scaledKeyPoint;
            this.Descriptor = descriptor;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 原始关键点
        /// </summary>
        public readonly Point OriginalKeyPoint;

        /// <summary>
        /// 缩放关键点
        /// </summary>
        public readonly Point ScaledKeyPoint;

        /// <summary>
        /// 描述子
        /// </summary>
        public readonly float[] Descriptor;

        /// <summary>
        /// 置信度
        /// </summary>
        public readonly float Confidence;
    }
}
