using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCV.Client.ViewModels.CommonContext;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenCV.Client.ViewModels.HomeContext
{
    /// <summary>
    /// 首页视图模型
    /// </summary>
    public class IndexViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public IndexViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 文件路径 —— string FilePath
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; set; }
        #endregion

        #region 图像颜色类型 —— ImreadModes? ImageColorType
        /// <summary>
        /// 图像颜色类型
        /// </summary>
        public ImreadModes? ImageColorType { get; set; }
        #endregion

        #region 原始图像 —— BitmapSource OriginalImage
        /// <summary>
        /// 原始图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource OriginalImage { get; set; }
        #endregion

        #region 效果图像 —— BitmapSource EffectiveImage
        /// <summary>
        /// 效果点云
        /// </summary>
        [DependencyProperty]
        public BitmapSource EffectiveImage { get; set; }
        #endregion

        #region 背景颜色 —— Brush BackgroundColor
        /// <summary>
        /// 背景颜色
        /// </summary>
        [DependencyProperty]
        public Brush BackgroundColor { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.BackgroundColor = new SolidColorBrush(Colors.Black);

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //常用

        #region 刷新图像 —— async void RefreshImage()
        /// <summary>
        /// 刷新图像
        /// </summary>
        public async void RefreshImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            await this.ReloadImage();

            this.Idle();
        }
        #endregion

        #region 查看原始图像 —— async void LookOriginalImage()
        /// <summary>
        /// 查看原始图像
        /// </summary>
        public async void LookOriginalImage()
        {
            #region # 验证

            if (this.OriginalImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(this.OriginalImage);
            await this._windowManager.ShowDialogAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 查看效果图像 —— async void LookEffectiveImage()
        /// <summary>
        /// 查看效果图像
        /// </summary>
        public async void LookEffectiveImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(this.EffectiveImage);
            await this._windowManager.ShowDialogAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 设置背景颜色 —— async void SetBackgroundColor()
        /// <summary>
        /// 设置背景颜色
        /// </summary>
        public async void SetBackgroundColor()
        {
            SelectColorViewModel viewModel = ResolveMediator.Resolve<SelectColorViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.BackgroundColor = new SolidColorBrush(viewModel.Color!.Value);
            }
        }
        #endregion


        //文件

        #region 打开灰度图像 —— async void OpenGrayImage()
        /// <summary>
        /// 打开灰度图像
        /// </summary>
        public async void OpenGrayImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                this.FilePath = openFileDialog.FileName;
                this.ImageColorType = ImreadModes.Grayscale;
                await this.ReloadImage();

                this.Idle();
            }
        }
        #endregion

        #region 打开彩色图像 —— async void OpenColorImage()
        /// <summary>
        /// 打开彩色图像
        /// </summary>
        public async void OpenColorImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                this.FilePath = openFileDialog.FileName;
                this.ImageColorType = ImreadModes.Color;
                await this.ReloadImage();

                this.Idle();
            }
        }
        #endregion

        #region 关闭图像 —— void CloseImage()
        /// <summary>
        /// 关闭图像
        /// </summary>
        public void CloseImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            MessageBoxResult result = MessageBox.Show("是否保存？", "提示", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                this.SaveImage();
            }
            if (result == MessageBoxResult.Yes || result == MessageBoxResult.No)
            {
                this.FilePath = null;
                this.OriginalImage = null;
                this.EffectiveImage = null;
                this.ImageColorType = null;
            }
        }
        #endregion

        #region 保存图像 —— async void SaveImage()
        /// <summary>
        /// 保存图像
        /// </summary>
        public async void SaveImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            await Task.Run(() => image.SaveImage(this.FilePath));
            await this.ReloadImage();

            this.Idle();
            this.ToastSuccess("保存成功！");
        }
        #endregion

        #region 另存为图像 —— async void SaveAsImage()
        /// <summary>
        /// 另存为图像
        /// </summary>
        public async void SaveAsImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                FileName = $"{Path.GetFileNameWithoutExtension(this.FilePath)} - 副本",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = this.EffectiveImage.ToMat();
                await Task.Run(() => image.SaveImage(this.FilePath));

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
        }
        #endregion


        //Private

        #region 加载图像 —— async Task ReloadImage()
        /// <summary>
        /// 加载图像
        /// </summary>
        public async Task ReloadImage()
        {
            if (this.ImageColorType == ImreadModes.Grayscale)
            {
                await this.ReloadMonoImage();
            }
            if (this.ImageColorType == ImreadModes.Color)
            {
                await this.ReloadColorImage();
            }
        }
        #endregion

        #region 加载灰度图像 —— async Task ReloadMonoImage()
        /// <summary>
        /// 加载灰度图像
        /// </summary>
        private async Task ReloadMonoImage()
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                return;
            }

            #endregion

            using Mat image = await Task.Run(() => Cv2.ImRead(this.FilePath, ImreadModes.Grayscale));
            BitmapSource bitmapSource = image.ToBitmapSource();
            this.OriginalImage = bitmapSource;
            this.EffectiveImage = bitmapSource;
        }
        #endregion

        #region 加载彩色图像 —— async Task ReloadColorImage()
        /// <summary>
        /// 加载彩色图像
        /// </summary>
        private async Task ReloadColorImage()
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                return;
            }

            #endregion

            using Mat image = await Task.Run(() => Cv2.ImRead(this.FilePath, ImreadModes.Color));
            BitmapSource bitmapSource = image.ToBitmapSource();
            this.OriginalImage = bitmapSource;
            this.EffectiveImage = bitmapSource;
        }
        #endregion

        #endregion
    }
}
