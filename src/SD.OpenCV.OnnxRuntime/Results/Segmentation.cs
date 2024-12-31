using OpenCvSharp;
using System.Runtime.InteropServices;

namespace SD.OpenCV.OnnxRuntime.Results
{
    /// <summary>
    /// 分割结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Segmentation
    {
        /// <summary>
        /// 创建分割结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="box">矩形框</param>
        /// <param name="contour">轮廓</param>
        /// <param name="confidence">置信度</param>
        public Segmentation(string label, Rect box, Point[] contour, float confidence)
            : this()
        {
            this.Label = label;
            this.Box = box;
            this.Contour = contour;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// 矩形框
        /// </summary>
        public readonly Rect Box;

        /// <summary>
        /// 轮廓
        /// </summary>
        public readonly Point[] Contour;

        /// <summary>
        /// 置信度
        /// </summary>
        public readonly float Confidence;
    }
}
