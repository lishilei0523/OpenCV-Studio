using OpenCvSharp;

namespace SD.OpenCV.Client.Models
{
    /// <summary>
    /// 目标检测结果
    /// </summary>
    public class ObjectDetection
    {
        /// <summary>
        /// 创建目标检测结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="box">矩形框</param>
        /// <param name="confidence">置信度</param>
        public ObjectDetection(string label, Rect box, float confidence)
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

        /// <summary>
        /// 标记
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 矩形框文本
        /// </summary>
        public string BoxText
        {
            get => this.Box.ToString();
        }
    }
}
