using Caliburn.Micro;
using OpenCvSharp;

namespace SD.OpenCV.Client.Models
{
    /// <summary>
    /// 定向目标检测结果
    /// </summary>
    public class ObjectObbDetection : PropertyChangedBase
    {
        /// <summary>
        /// 创建定向目标检测结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="rotatedBox">矩形框</param>
        /// <param name="confidence">置信度</param>
        public ObjectObbDetection(string label, RotatedRect rotatedBox, float confidence)
        {
            this.Label = label;
            this.RotatedBox = rotatedBox;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 标签
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// 旋转矩形框
        /// </summary>
        public RotatedRect RotatedBox { get; private set; }

        /// <summary>
        /// 置信度
        /// </summary>
        public float Confidence { get; private set; }

        /// <summary>
        /// 标记
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 旋转矩形框文本
        /// </summary>
        public string RotatedBoxText
        {
            get => $"O({this.RotatedBox.Center.X},{this.RotatedBox.Center.Y})|{this.RotatedBox.Size.Width}*{this.RotatedBox.Size.Height}|{this.RotatedBox.Angle:F3}";
        }
    }
}
