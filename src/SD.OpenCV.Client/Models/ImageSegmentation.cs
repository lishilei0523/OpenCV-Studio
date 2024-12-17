using OpenCvSharp;

namespace SD.OpenCV.Client.Models
{
    /// <summary>
    /// 图像分割结果
    /// </summary>
    public class ImageSegmentation
    {
        /// <summary>
        /// 创建图像分割结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="box">矩形框</param>
        /// <param name="contour">轮廓</param>
        /// <param name="confidence">置信度</param>
        public ImageSegmentation(string label, Rect box, Point[] contour, float confidence)
        {
            this.Label = label;
            this.Box = box;
            this.Contour = contour;
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
        /// 轮廓
        /// </summary>
        public Point[] Contour { get; private set; }

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
            get => $"({this.Box.X},{this.Box.Y})|{this.Box.Width}*{this.Box.Height}";
        }
    }
}
