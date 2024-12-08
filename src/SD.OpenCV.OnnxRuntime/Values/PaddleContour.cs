using OpenCvSharp;
using System.Runtime.InteropServices;

namespace SD.OpenCV.OnnxRuntime.Values
{
    /// <summary>
    /// Paddle轮廓
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct PaddleContour
    {
        /// <summary>
        /// 创建Paddle轮廓构造器
        /// </summary>
        /// <param name="points">点集</param>
        /// <param name="confidence">置信度</param>
        public PaddleContour(Point2f[] points, float confidence)
            : this()
        {
            this.Points = points;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 点集
        /// </summary>
        public readonly Point2f[] Points;

        /// <summary>
        /// 置信度
        /// </summary>
        public readonly float Confidence;
    }
}
