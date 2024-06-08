using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CalibrationContext;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Client.ViewModels.MorphContext;
using SD.OpenCV.Client.ViewModels.SegmentContext;
using SD.OpenCV.Primitives.Extensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.HomeContext
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
            this.BackgroundColor = new SolidColorBrush(Colors.LightGray);

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

        #region 打开图像 —— async void OpenImage()
        /// <summary>
        /// 打开图像
        /// </summary>
        public async void OpenImage()
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


        //颜色空间

        #region 转换灰度空间 —— async void ConvertGray()
        /// <summary>
        /// 转换灰度空间
        /// </summary>
        public async void ConvertGray()
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
            using Mat grayImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2GRAY));
            this.EffectiveImage = grayImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 转换HSV空间 —— async void ConvertHSV()
        /// <summary>
        /// 转换HSV空间
        /// </summary>
        public async void ConvertHSV()
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
            using Mat hsvImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2HSV));
            this.EffectiveImage = hsvImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 转换HLS空间 —— async void ConvertHLS()
        /// <summary>
        /// 转换HLS空间
        /// </summary>
        public async void ConvertHLS()
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
            using Mat hlsImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2HLS));
            this.EffectiveImage = hlsImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 转换Lab空间 —— async void ConvertLab()
        /// <summary>
        /// 转换Lab空间
        /// </summary>
        public async void ConvertLab()
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
            using Mat labImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2Lab));
            this.EffectiveImage = labImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 转换Luv空间 —— async void ConvertLuv()
        /// <summary>
        /// 转换Luv空间
        /// </summary>
        public async void ConvertLuv()
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
            using Mat luvImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2Luv));
            this.EffectiveImage = luvImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 转换YCrCb空间 —— async void ConvertYCrCb()
        /// <summary>
        /// 转换YCrCb空间
        /// </summary>
        public async void ConvertYCrCb()
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
            using Mat yCrCbImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.BGR2YCrCb));
            this.EffectiveImage = yCrCbImage.ToBitmapSource();

            this.Idle();
        }
        #endregion


        //形态学

        #region 腐蚀 —— async void MorphErode()
        /// <summary>
        /// 腐蚀
        /// </summary>
        public async void MorphErode()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphErode(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 膨胀 —— async void MorphDilate()
        /// <summary>
        /// 膨胀
        /// </summary>
        public async void MorphDilate()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphDilate(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 开运算 —— async void MorphOpen()
        /// <summary>
        /// 开运算
        /// </summary>
        public async void MorphOpen()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphOpen(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 闭运算 —— async void MorphClose()
        /// <summary>
        /// 闭运算
        /// </summary>
        public async void MorphClose()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphClose(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 梯度运算 —— async void MorphGradient()
        /// <summary>
        /// 梯度运算
        /// </summary>
        public async void MorphGradient()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphGradient(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 击中运算 —— async void MorphHitMiss()
        /// <summary>
        /// 击中运算
        /// </summary>
        public async void MorphHitMiss()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphHitMiss(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 礼帽运算 —— async void MorphTopHat()
        /// <summary>
        /// 礼帽运算
        /// </summary>
        public async void MorphTopHat()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphTopHat(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 黑帽运算 —— async void MorphBlackHat()
        /// <summary>
        /// 黑帽运算
        /// </summary>
        public async void MorphBlackHat()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MorphViewModel viewModel = ResolveMediator.Resolve<MorphViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.MorphBlackHat(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion


        //图像分割

        #region 阈值分割 —— async void ThresholdSegment()
        /// <summary>
        /// 阈值分割
        /// </summary>
        public async void ThresholdSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ThresholdViewModel viewModel = ResolveMediator.Resolve<ThresholdViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region Otsu阈值分割 —— async void OtsuThresholdSegment()
        /// <summary>
        /// Otsu阈值分割
        /// </summary>
        public async void OtsuThresholdSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat colorImage = this.EffectiveImage.ToMat();
            using Mat grayImage = new Mat();
            using Mat result = new Mat();
            await Task.Run(() => Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.BGR2GRAY));
            await Task.Run(() => Cv2.Threshold(grayImage, result, 0, 255, ThresholdTypes.Otsu));
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 颜色分割 —— async void ColorSegment()
        /// <summary>
        /// 颜色分割
        /// </summary>
        public async void ColorSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ColorViewModel viewModel = ResolveMediator.Resolve<ColorViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion


        //标定

        #region 单目标定 —— async void MonoCalibrate()
        /// <summary>
        /// 单目标定
        /// </summary>
        public async void MonoCalibrate()
        {
            MonoViewModel viewModel = ResolveMediator.Resolve<MonoViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
        }
        #endregion

        #region 手眼标定 —— async void CalibrateHandEye()
        /// <summary>
        /// 手眼标定
        /// </summary>
        public async void CalibrateHandEye()
        {
            HandEyeViewModel viewModel = ResolveMediator.Resolve<HandEyeViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);
        }
        #endregion


        //事件

        #region 键盘按下事件 —— void OnKeyDown()
        /// <summary>
        /// 键盘按下事件
        /// </summary>
        public void OnKeyDown()
        {
            if (Keyboard.IsKeyDown(Key.F5))
            {
                #region # 验证

                if (this.EffectiveImage == null)
                {
                    MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.RefreshImage();
            }
            if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.S))
            {
                #region # 验证

                if (this.EffectiveImage == null)
                {
                    MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.SaveImage();
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
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                return;
            }

            #endregion

            using Mat image = await Task.Run(() => Cv2.ImRead(this.FilePath));
            BitmapSource bitmapSource = image.ToBitmapSource();
            this.OriginalImage = bitmapSource;
            this.EffectiveImage = bitmapSource;
        }
        #endregion

        #endregion
    }
}
