using SD.Infrastructure.Shapes;

namespace SD.OpenCV.Client.Models
{
    /// <summary>
    /// 文本检测结果
    /// </summary>
    public class TextDetection
    {
        /// <summary>
        /// 创建文本检测结果构造器
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="box">矩形框</param>
        /// <param name="confidence">置信度</param>
        public TextDetection(string text, RectangleL box, float confidence)
        {
            this.Text = text;
            this.Box = box;
            this.Confidence = confidence;
        }

        /// <summary>
        /// 文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 矩形框
        /// </summary>
        public RectangleL Box { get; private set; }

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
