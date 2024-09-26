using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 图像对比查看视图模型
    /// </summary>
    public class ImageCompareViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ImageCompareViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 标题 —— string Title
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public string Title { get; set; }
        #endregion

        #region 参考图像 —— BitmapSource SourceImage
        /// <summary>
        /// 参考图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource SourceImage { get; set; }
        #endregion

        #region 目标图像 —— BitmapSource TargetImage
        /// <summary>
        /// 目标图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource TargetImage { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 加载 —— void Load(BitmapSource sourceImage...
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sourceImage">参考图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="title">标题</param>
        public void Load(BitmapSource sourceImage, BitmapSource targetImage, string title = "查看图像")
        {
            this.SourceImage = sourceImage;
            this.TargetImage = targetImage;
            this.Title = title;
        }
        #endregion

        #region 另存为参考图像 —— async void SaveAsSourceImage()
        /// <summary>
        /// 另存为参考图像
        /// </summary>
        public async void SaveAsSourceImage()
        {
            #region # 验证

            if (this.SourceImage == null)
            {
                MessageBox.Show("参考图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = this.Title,
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = this.SourceImage.ToMat();
                await Task.Run(() => image.SaveImage(saveFileDialog.FileName));

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
        }
        #endregion

        #region 另存为目标图像 —— async void SaveAsTargetImage()
        /// <summary>
        /// 另存为目标图像
        /// </summary>
        public async void SaveAsTargetImage()
        {
            #region # 验证

            if (this.TargetImage == null)
            {
                MessageBox.Show("目标图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = this.Title,
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = this.TargetImage.ToMat();
                await Task.Run(() => image.SaveImage(saveFileDialog.FileName));

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
        }
        #endregion

        #endregion
    }
}
