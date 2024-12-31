using OpenCvSharp;
using System.Runtime.InteropServices;

namespace SD.OpenCV.OnnxRuntime.Results
{
    /// <summary>
    /// 定向检测结果
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ObbDetection
    {
        /// <summary>
        /// 创建定向检测结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="rotatedBox">旋转矩形框</param>
        /// <param name="confidence">置信度</param>
        public ObbDetection(string label, RotatedRect rotatedBox, float confidence)
            : this()
        {
            this.Label = label;
            this.RotatedBox = rotatedBox;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public readonly string Label;

        /// <summary>
        /// 旋转矩形框
        /// </summary>
        public readonly RotatedRect RotatedBox;

        /// <summary>
        /// 置信度
        /// </summary>
        public readonly float Confidence;
    }
}
