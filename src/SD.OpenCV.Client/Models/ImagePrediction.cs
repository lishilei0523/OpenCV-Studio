namespace SD.OpenCV.Client.Models
{
    /// <summary>
    /// 图像分类结果
    /// </summary>
    public class ImagePrediction
    {
        /// <summary>
        /// 创建图像分类结果构造器
        /// </summary>
        /// <param name="label">标签</param>
        /// <param name="confidence">置信度</param>
        public ImagePrediction(string label, float confidence)
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
