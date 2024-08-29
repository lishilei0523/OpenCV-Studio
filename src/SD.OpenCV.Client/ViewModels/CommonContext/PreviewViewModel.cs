using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 图像预览视图模型基类
    /// </summary>
    public abstract class PreviewViewModel : ScreenBase
    {
        #region # 属性

        #region 图像 —— Mat Image
        /// <summary>
        /// 图像
        /// </summary>
        public Mat Image { get; set; }
        #endregion

        #region 图像源 —— BitmapSource BitmapSource
        /// <summary>
        /// 图像源
        /// </summary>
        [DependencyProperty]
        public BitmapSource BitmapSource { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 加载 —— virtual void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public virtual void Load(BitmapSource bitmapSource)
        {
            this.BitmapSource = bitmapSource;
            this.Image = bitmapSource.ToMat();
        }
        #endregion

        #region 重置 —— virtual void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            this.BitmapSource = this.Image.ToBitmapSource();
        }
        #endregion

        #region 页面失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 页面失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this.Image?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
