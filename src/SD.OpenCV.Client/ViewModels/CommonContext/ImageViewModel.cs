using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows.Media.Imaging;

namespace OpenCV.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 图像查看视图模型
    /// </summary>
    public class ImageViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ImageViewModel()
        {

        }

        #endregion

        #region # 属性

        #region 图像源 —— BitmapSource Image
        /// <summary>
        /// 图像源
        /// </summary>
        [DependencyProperty]
        public BitmapSource Image { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 加载 —— void Load(BitmapSource image)
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="image">图像</param>
        public void Load(BitmapSource image)
        {
            this.Image = image;
        }
        #endregion

        #endregion
    }
}
