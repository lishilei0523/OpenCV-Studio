using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XFeatures2D;
using ScottPlot;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CalibrationContext;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Client.ViewModels.DrawContext;
using SD.OpenCV.Client.ViewModels.EdgeContext;
using SD.OpenCV.Client.ViewModels.FeatureContext;
using SD.OpenCV.Client.ViewModels.FrequencyBlurContext;
using SD.OpenCV.Client.ViewModels.GeometryContext;
using SD.OpenCV.Client.ViewModels.GrayscaleContext;
using SD.OpenCV.Client.ViewModels.HistogramContext;
using SD.OpenCV.Client.ViewModels.HoughContext;
using SD.OpenCV.Client.ViewModels.MorphContext;
using SD.OpenCV.Client.ViewModels.SegmentContext;
using SD.OpenCV.Client.ViewModels.SpaceBlurContext;
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Extensions;
using SD.OpenCV.Primitives.Models;
using SD.OpenCV.Reconstructions;
using SD.OpenCV.SkiaSharp;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Colors = System.Windows.Media.Colors;
using Image = System.Windows.Controls.Image;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

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

        #region 文件格式 —— string FileExtension
        /// <summary>
        /// 文件格式
        /// </summary>
        [DependencyProperty]
        public string FileExtension { get; set; }
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

        #region 鼠标位置 —— Point MousePosition
        /// <summary>
        /// 鼠标位置
        /// </summary>
        [DependencyProperty]
        public System.Windows.Point MousePosition { get; set; }
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
            await this._windowManager.ShowWindowAsync(viewModel);

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
            await this._windowManager.ShowWindowAsync(viewModel);

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
                this.FileExtension = Path.GetExtension(this.FilePath).Replace(".", string.Empty).ToUpper();
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
                this.FileExtension = null;
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


        //算术

        #region 图像加法 —— async void AddImage()
        /// <summary>
        /// 图像加法
        /// </summary>
        public async void AddImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat source = this.EffectiveImage.ToMat();
                using Mat target = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                using Mat result = new Mat();
                Cv2.Add(source, target, result);
                this.EffectiveImage = result.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 图像减法 —— async void MinusImage()
        /// <summary>
        /// 图像减法
        /// </summary>
        public async void MinusImage()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat source = this.EffectiveImage.ToMat();
                using Mat target = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                using Mat result = new Mat();
                Cv2.Subtract(source, target, result);
                this.EffectiveImage = result.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion


        //绘制

        #region 绘制形状 —— async void DrawShapes()
        /// <summary>
        /// 绘制形状
        /// </summary>
        public async void DrawShapes()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ShapeViewModel viewModel = ResolveMediator.Resolve<ShapeViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 绘制掩膜 —— async void DrawMask()
        /// <summary>
        /// 绘制掩膜
        /// </summary>
        public async void DrawMask()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            DrawContext.MaskViewModel viewModel = ResolveMediator.Resolve<DrawContext.MaskViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

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

        #region 击否运算 —— async void MorphHitMiss()
        /// <summary>
        /// 击否运算
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


        //空间滤波

        #region 均值滤波 —— async void MeanBlur()
        /// <summary>
        /// 均值滤波
        /// </summary>
        public async void MeanBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MeanViewModel viewModel = ResolveMediator.Resolve<MeanViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = new Mat();
                Size kernelSize = new Size(viewModel.KernelSize!.Value, viewModel.KernelSize!.Value);
                Cv2.Blur(image, resultImage, kernelSize);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯滤波 —— async void GaussianBlur()
        /// <summary>
        /// 高斯滤波
        /// </summary>
        public async void GaussianBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianViewModel viewModel = ResolveMediator.Resolve<GaussianViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = new Mat();
                Size kernelSize = new Size(viewModel.KernelSize!.Value, viewModel.KernelSize!.Value);
                Cv2.GaussianBlur(image, resultImage, kernelSize, viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 中值滤波 —— async void MedianBlur()
        /// <summary>
        /// 中值滤波
        /// </summary>
        public async void MedianBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MedianViewModel viewModel = ResolveMediator.Resolve<MedianViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = new Mat();
                Cv2.MedianBlur(image, resultImage, viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 锐化滤波 —— async void SharpBlur()
        /// <summary>
        /// 锐化滤波
        /// </summary>
        public async void SharpBlur()
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
            using Mat result = await Task.Run(() => colorImage.SharpBlur());
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 方块滤波 —— async void BoxBlur()
        /// <summary>
        /// 方块滤波
        /// </summary>
        public async void BoxBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            BoxViewModel viewModel = ResolveMediator.Resolve<BoxViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = new Mat();
                Size kernelSize = new Size(viewModel.KernelSize!.Value, viewModel.KernelSize!.Value);
                Cv2.BoxFilter(image, resultImage, viewModel.Depth!.Value, kernelSize, null, viewModel.NeedToNormalize);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 双边滤波 —— async void BilateralBlur()
        /// <summary>
        /// 双边滤波
        /// </summary>
        public async void BilateralBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            BilateralViewModel viewModel = ResolveMediator.Resolve<BilateralViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = new Mat();
                Cv2.BilateralFilter(image, resultImage, viewModel.Diameter!.Value, viewModel.SigmaColor!.Value, viewModel.SigmaSpace!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region Retinex增强 —— async void SingleScaleRetinex()
        /// <summary>
        /// Retinex增强
        /// </summary>
        public async void SingleScaleRetinex()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            RetinexViewModel viewModel = ResolveMediator.Resolve<RetinexViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.SingleScaleRetinex(viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion


        //频率滤波

        #region 理想低通滤波 —— async void IdealLPBlur()
        /// <summary>
        /// 理想低通滤波
        /// </summary>
        public async void IdealLPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealLowViewModel viewModel = ResolveMediator.Resolve<IdealLowViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.IdealLPBlur(viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 理想高通滤波 —— async void IdealHPBlur()
        /// <summary>
        /// 理想高通滤波
        /// </summary>
        public async void IdealHPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealHighViewModel viewModel = ResolveMediator.Resolve<IdealHighViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.IdealHPBlur(viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 理想带通滤波 —— async void IdealBPBlur()
        /// <summary>
        /// 理想带通滤波
        /// </summary>
        public async void IdealBPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealBandViewModel viewModel = ResolveMediator.Resolve<IdealBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.IdealBPBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 理想带阻滤波 —— async void IdealBRBlur()
        /// <summary>
        /// 理想带阻滤波
        /// </summary>
        public async void IdealBRBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealBandViewModel viewModel = ResolveMediator.Resolve<IdealBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.IdealBRBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯低通滤波 —— async void GaussianLPBlur()
        /// <summary>
        /// 高斯低通滤波
        /// </summary>
        public async void GaussianLPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianLowViewModel viewModel = ResolveMediator.Resolve<GaussianLowViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GaussianLPBlur(viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯高通滤波 —— async void GaussianHPBlur()
        /// <summary>
        /// 高斯高通滤波
        /// </summary>
        public async void GaussianHPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianHighViewModel viewModel = ResolveMediator.Resolve<GaussianHighViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GaussianHPBlur(viewModel.Sigma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯带通滤波 —— async void GaussianBPBlur()
        /// <summary>
        /// 高斯带通滤波
        /// </summary>
        public async void GaussianBPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianBandViewModel viewModel = ResolveMediator.Resolve<GaussianBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GaussianBPBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯带阻滤波 —— async void GaussianBRBlur()
        /// <summary>
        /// 高斯带阻滤波
        /// </summary>
        public async void GaussianBRBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianBandViewModel viewModel = ResolveMediator.Resolve<GaussianBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GaussianBRBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 高斯同态滤波 —— async void GaussianHomoBlur()
        /// <summary>
        /// 高斯同态滤波
        /// </summary>
        public async void GaussianHomoBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianHomoViewModel viewModel = ResolveMediator.Resolve<GaussianHomoViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GaussianHomoBlur(viewModel.GammaH!.Value, viewModel.GammaL!.Value, viewModel.Sigma!.Value, viewModel.Slope!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 巴特沃斯低通滤波 —— async void ButterworthLPBlur()
        /// <summary>
        /// 巴特沃斯低通滤波
        /// </summary>
        public async void ButterworthLPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthLowViewModel viewModel = ResolveMediator.Resolve<ButterworthLowViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ButterworthLPBlur(viewModel.Sigma!.Value, viewModel.N!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 巴特沃斯高通滤波 —— async void ButterworthHPBlur()
        /// <summary>
        /// 巴特沃斯高通滤波
        /// </summary>
        public async void ButterworthHPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthHighViewModel viewModel = ResolveMediator.Resolve<ButterworthHighViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ButterworthHPBlur(viewModel.Sigma!.Value, viewModel.N!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 巴特沃斯带通滤波 —— async void ButterworthBPBlur()
        /// <summary>
        /// 巴特沃斯带通滤波
        /// </summary>
        public async void ButterworthBPBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthBandViewModel viewModel = ResolveMediator.Resolve<ButterworthBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ButterworthBPBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value, viewModel.N!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 巴特沃斯带阻滤波 —— async void ButterworthBRBlur()
        /// <summary>
        /// 巴特沃斯带阻滤波
        /// </summary>
        public async void ButterworthBRBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthBandViewModel viewModel = ResolveMediator.Resolve<ButterworthBandViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ButterworthBRBlur(viewModel.Sigma!.Value, viewModel.BandWidth!.Value, viewModel.N!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 巴特沃斯同态滤波 —— async void ButterworthHomoBlur()
        /// <summary>
        /// 巴特沃斯同态滤波
        /// </summary>
        public async void ButterworthHomoBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthHomoViewModel viewModel = ResolveMediator.Resolve<ButterworthHomoViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ButterworthHomoBlur(viewModel.GammaH!.Value, viewModel.GammaL!.Value, viewModel.Sigma!.Value, viewModel.Slope!.Value);
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

        #region 矩形分割 —— async void RectangleSegment()
        /// <summary>
        /// 矩形分割
        /// </summary>
        public async void RectangleSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            RectangleViewModel viewModel = ResolveMediator.Resolve<RectangleViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 圆形分割 —— async void CircleSegment()
        /// <summary>
        /// 圆形分割
        /// </summary>
        public async void CircleSegment()
        {
            //TODO 实现
            MessageBox.Show("未实现", "错误", MessageBoxButton.OK);
        }
        #endregion

        #region 轮廓分割 —— async void ContourSegment()
        /// <summary>
        /// 轮廓分割
        /// </summary>
        public async void ContourSegment()
        {
            //TODO 实现
            MessageBox.Show("未实现", "错误", MessageBoxButton.OK);
        }
        #endregion

        #region 掩膜分割 —— async void MaskSegment()
        /// <summary>
        /// 掩膜分割
        /// </summary>
        public async void MaskSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SegmentContext.MaskViewModel viewModel = ResolveMediator.Resolve<SegmentContext.MaskViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion


        //图像矫正

        #region 矫正畸变 —— async void RectifyDistortions()
        /// <summary>
        /// 矫正畸变
        /// </summary>
        public async void RectifyDistortions()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.cins)|*.cins",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                #region # 验证

                if (string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    MessageBox.Show("未选择相机内参！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.Busy();

                string binaryText = await Task.Run(() => File.ReadAllText(openFileDialog.FileName));
                CameraIntrinsics cameraIntrinsics = binaryText.AsBinaryTo<CameraIntrinsics>();
                using Mat colorImage = this.EffectiveImage.ToMat();
                using Mat rectifiedImage = await Task.Run(() => colorImage.RectifyDistortions(cameraIntrinsics));
                this.EffectiveImage = rectifiedImage.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 矫正位姿 —— async void RectifyPose()
        /// <summary>
        /// 矫正位姿
        /// </summary>
        public async void RectifyPose()
        {
            //TODO 实现
            MessageBox.Show("未实现", "错误", MessageBoxButton.OK);
        }
        #endregion


        //边缘检测

        #region Sobel边缘检测 —— async void ApplySobel()
        /// <summary>
        /// Sobel边缘检测
        /// </summary>
        public async void ApplySobel()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SobelViewModel viewModel = ResolveMediator.Resolve<SobelViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ApplySobel(viewModel.KernelSize!.Value, viewModel.Alpha!.Value, viewModel.Beta!.Value, viewModel.Gamma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region Canny边缘检测 —— async void ApplyCanny()
        /// <summary>
        /// Canny边缘检测
        /// </summary>
        public async void ApplyCanny()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            CannyViewModel viewModel = ResolveMediator.Resolve<CannyViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.Canny(viewModel.Threshold1!.Value, viewModel.Threshold2!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region Scharr边缘检测 —— async void ApplyScharr()
        /// <summary>
        /// Scharr边缘检测
        /// </summary>
        public async void ApplyScharr()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ScharrViewModel viewModel = ResolveMediator.Resolve<ScharrViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ApplyScharr(viewModel.Alpha!.Value, viewModel.Beta!.Value, viewModel.Gamma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region Robert边缘检测 —— async void ApplyRobert()
        /// <summary>
        /// Robert边缘检测
        /// </summary>
        public async void ApplyRobert()
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
            await Task.Run(() => Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.BGR2GRAY));
            using Mat result = await Task.Run(() => grayImage.ApplyRobert());
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region Laplacian边缘检测 —— async void ApplyLaplacian()
        /// <summary>
        /// Sobel边缘检测
        /// </summary>
        public async void ApplyLaplacian()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            LaplacianViewModel viewModel = ResolveMediator.Resolve<LaplacianViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.ApplyLaplacian(viewModel.KernelSize!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion


        //灰度变换

        #region 灰度变换 —— async void LinearTransform()
        /// <summary>
        /// 灰度变换
        /// </summary>
        public async void LinearTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            LinearViewModel viewModel = ResolveMediator.Resolve<LinearViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.LinearTransform(viewModel.Alpha!.Value, viewModel.Beta!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 伽马变换 —— async void GammaTransform()
        /// <summary>
        /// 伽马变换
        /// </summary>
        public async void GammaTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GammaViewModel viewModel = ResolveMediator.Resolve<GammaViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.GammaTransform(viewModel.Gamma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 对数变换 —— async void LogarithmicTransform()
        /// <summary>
        /// 对数变换
        /// </summary>
        public async void LogarithmicTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            LogarithmicViewModel viewModel = ResolveMediator.Resolve<LogarithmicViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.LogarithmicTransform(viewModel.Gamma!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 阴影变换 —— async void ShadingTransform()
        /// <summary>
        /// 阴影变换
        /// </summary>
        public async void ShadingTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ShadingViewModel viewModel = ResolveMediator.Resolve<ShadingViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                Size kernelSize = new Size(viewModel.KernelSize!.Value, viewModel.KernelSize!.Value);
                using Mat resultImage = image.ShadingTransform(kernelSize, viewModel.Gain!.Value, viewModel.Noise!.Value, viewModel.Offset!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion


        //几何变换

        #region 缩放 —— async void ScaleTransform()
        /// <summary>
        /// 缩放
        /// </summary>
        public async void ScaleTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ScaleViewModel viewModel = ResolveMediator.Resolve<ScaleViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                Mat resultImage;
                switch (viewModel.SelectedScaleMode)
                {
                    case ScaleMode.Absolute:
                        resultImage = image.ResizeAbsolutely(viewModel.Width!.Value, viewModel.Height!.Value);
                        break;
                    case ScaleMode.Relative:
                        resultImage = image.ResizeRelatively(viewModel.ScaleRatio!.Value);
                        break;
                    case ScaleMode.Adaptive:
                        resultImage = image.ResizeAdaptively(viewModel.SideSize!.Value);
                        break;
                    case null:
                        throw new NotSupportedException();
                    default:
                        throw new NotSupportedException();
                }

                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 旋转 —— async void RotateTransform()
        /// <summary>
        /// 旋转
        /// </summary>
        public async void RotateTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            RotationViewModel viewModel = ResolveMediator.Resolve<RotationViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.RotateTrans(viewModel.Angle!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 平移 —— async void TranslateTransform()
        /// <summary>
        /// 平移
        /// </summary>
        public async void TranslateTransform()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            TranslationViewModel viewModel = ResolveMediator.Resolve<TranslationViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.TranslateTrans(viewModel.OffsetX!.Value, viewModel.OffsetY!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 仿射变换 —— async void AffineTransform()
        /// <summary>
        /// 仿射变换
        /// </summary>
        public async void AffineTransform()
        {
            //TODO 实现
            MessageBox.Show("未实现", "错误", MessageBoxButton.OK);
        }
        #endregion

        #region 透视变换 —— async void PerspectiveTransform()
        /// <summary>
        /// 透视变换
        /// </summary>
        public async void PerspectiveTransform()
        {
            //TODO 实现
            MessageBox.Show("未实现", "错误", MessageBoxButton.OK);
        }
        #endregion

        #region 距离变换 —— async void DistanceTransform()
        /// <summary>
        /// 距离变换
        /// </summary>
        public async void DistanceTransform()
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
            await Task.Run(() => Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.BGR2GRAY));
            using Mat result = grayImage.DistanceTrans();
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion


        //霍夫变换

        #region 霍夫线查找 —— async void HoughFindLines()
        /// <summary>
        /// 霍夫线查找
        /// </summary>
        public async void HoughFindLines()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            LineViewModel viewModel = ResolveMediator.Resolve<LineViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                LineSegmentPoint[] lines = Cv2.HoughLinesP(grayImage, viewModel.Rho!.Value, viewModel.Theta!.Value, viewModel.Threshold!.Value);
                foreach (LineSegmentPoint line in lines)
                {
                    Cv2.Line(image, line.P1, line.P2, Scalar.Red, 2);
                }
                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 霍夫圆查找 —— async void HoughFindCircles()
        /// <summary>
        /// 霍夫圆查找
        /// </summary>
        public async void HoughFindCircles()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            CircleViewModel viewModel = ResolveMediator.Resolve<CircleViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                CircleSegment[] circles = Cv2.HoughCircles(grayImage, viewModel.HoughMode!.Value, viewModel.Dp!.Value, viewModel.MinDistance!.Value, 100D, 100D, viewModel.MinRadius!.Value, viewModel.MaxRadius!.Value);
                foreach (CircleSegment circle in circles)
                {
                    Point center = circle.Center.ToPoint();
                    int radius = (int)Math.Ceiling(circle.Radius);

                    //绘制圆
                    Cv2.Circle(image, center, radius, Scalar.Red, 2);

                    //绘制圆心
                    Cv2.Circle(image, center, 2, Scalar.Red, 2);
                }
                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion


        //直方图

        #region 查看灰度直方图 —— async void LookHistogram()
        /// <summary>
        /// 查看灰度直方图
        /// </summary>
        public async void LookHistogram()
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
            await Task.Run(() => Cv2.CvtColor(colorImage, grayImage, ColorConversionCodes.BGR2GRAY));
            using Mat histImage = await Task.Run(() => grayImage.GenerateHistogramImage(1280, 800));
            BitmapSource bitmapSource = histImage.ToBitmapSource();
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(bitmapSource);
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 直方图均衡化 —— async void EqualizeHist()
        /// <summary>
        /// 直方图均衡化
        /// </summary>
        public async void EqualizeHist()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            EqualizationViewModel viewModel = ResolveMediator.Resolve<EqualizationViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = image.AdaptiveEqualizeHist(viewModel.ClipLimit!.Value);
                this.EffectiveImage = resultImage.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 直方图规定化 —— async void SpecifyHist()
        /// <summary>
        /// 直方图规定化
        /// </summary>
        public async void SpecifyHist()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat referenceImage = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                using Mat image = this.EffectiveImage.ToMat();
                using Mat resultImage = await Task.Run(() => image.SpecifyHist(referenceImage));
                this.EffectiveImage = resultImage.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion


        //关键点

        #region 检测Harris关键点 —— async void DetectHarris()
        /// <summary>
        /// 检测Harris关键点
        /// </summary>
        public async void DetectHarris()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            HarrisViewModel viewModel = ResolveMediator.Resolve<HarrisViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                Point[] points = await Task.Run(() => grayImage.DetectHarris(viewModel.BlockSize!.Value, viewModel.KernelSize!.Value, viewModel.K!.Value));

                //绘制关键点
                foreach (Point point in points)
                {
                    Cv2.Circle(image, point, 2, Scalar.Red);
                }

                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 检测Shi-Tomasi关键点 —— async void DetectShiTomasi()
        /// <summary>
        /// 检测Shi-Tomasi关键点
        /// </summary>
        public async void DetectShiTomasi()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ShiTomasiViewModel viewModel = ResolveMediator.Resolve<ShiTomasiViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                Point2f[] points = await Task.Run(() => Cv2.GoodFeaturesToTrack(grayImage, viewModel.MaxCorners!.Value, viewModel.QualityLevel!.Value, viewModel.MinDistance!.Value, null!, viewModel.BlockSize!.Value, false, 0));

                //绘制关键点
                foreach (Point point in points)
                {
                    Cv2.Circle(image, point, 2, Scalar.Red);
                }

                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 检测SIFT关键点 —— async void DetectSIFT()
        /// <summary>
        /// 检测SIFT关键点
        /// </summary>
        public async void DetectSIFT()
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
            using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            using SIFT sift = SIFT.Create();
            using Mat descriptors = new Mat();
            KeyPoint[] keyPoints = { };
            await Task.Run(() => sift.DetectAndCompute(grayImage, null, out keyPoints, descriptors));

            //绘制关键点
            await Task.Run(() => Cv2.DrawKeypoints(image, keyPoints, image, Scalar.Red));
            this.EffectiveImage = image.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 检测SURF关键点 —— async void DetectSURF()
        /// <summary>
        /// 检测SURF关键点
        /// </summary>
        public async void DetectSURF()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SurfViewModel viewModel = ResolveMediator.Resolve<SurfViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                using SURF surf = SURF.Create(viewModel.HessianThreshold!.Value);
                using Mat descriptors = new Mat();
                KeyPoint[] keyPoints = { };
                await Task.Run(() => surf.DetectAndCompute(grayImage, null, out keyPoints, descriptors));

                //绘制关键点
                await Task.Run(() => Cv2.DrawKeypoints(image, keyPoints, image, Scalar.Red));
                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 检测FAST关键点 —— async void DetectFAST()
        /// <summary>
        /// 检测FAST关键点
        /// </summary>
        public async void DetectFAST()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            FastViewModel viewModel = ResolveMediator.Resolve<FastViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                using FastFeatureDetector fast = FastFeatureDetector.Create(viewModel.Threshold!.Value, viewModel.NonmaxSuppression);
                KeyPoint[] keyPoints = await Task.Run(() => fast.Detect(grayImage));

                //绘制关键点
                await Task.Run(() => Cv2.DrawKeypoints(image, keyPoints, image, Scalar.Red));
                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 检测ORB关键点 —— async void DetectORB()
        /// <summary>
        /// 检测ORB关键点
        /// </summary>
        public async void DetectORB()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            OrbViewModel viewModel = ResolveMediator.Resolve<OrbViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                using ORB orb = ORB.Create(viewModel.Threshold!.Value);
                using Mat descriptors = new Mat();
                KeyPoint[] keyPoints = { };
                await Task.Run(() => orb.DetectAndCompute(grayImage, null, out keyPoints, descriptors));

                //绘制关键点
                await Task.Run(() => Cv2.DrawKeypoints(image, keyPoints, image, Scalar.Red));
                this.EffectiveImage = image.ToBitmapSource();
            }

            this.Idle();
        }
        #endregion

        #region 检测Super关键点 —— async void DetectSuper()
        /// <summary>
        /// 检测Super关键点
        /// </summary>
        public async void DetectSuper()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            const int scaledSize = 512;
            using Mat image = this.EffectiveImage.ToMat();
            using Mat scaledImage = image.ResizeAdaptively(scaledSize);
            using SuperFeature superFeature = new SuperFeature();
            using Mat descriptors = new Mat();
            KeyPoint[] keyPoints = { };
            await Task.Run(() => superFeature.DetectAndCompute(scaledImage, null, out keyPoints, descriptors));
            IList<KeyPoint> scaledSrcKpts = keyPoints.ScaleKeyPoints(image.Width, image.Height, scaledSize);

            //绘制关键点
            await Task.Run(() => Cv2.DrawKeypoints(image, scaledSrcKpts, image, Scalar.Red));
            this.EffectiveImage = image.ToBitmapSource();

            this.Idle();
        }
        #endregion


        //特征

        #region 计算SIFT特征 —— async void ComputeSIFT()
        /// <summary>
        /// 计算SIFT特征
        /// </summary>
        public async void ComputeSIFT()
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
            using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
            using SIFT sift = SIFT.Create();
            using Mat descriptors = new Mat();
            await Task.Run(() => sift.DetectAndCompute(grayImage, null, out _, descriptors));

            //绘制直方图
            using Plot plot = new Plot();
            plot.AddDescriptors(descriptors);
            using SKImage skImage = plot.GetSKImage(1280, 800);
            BitmapSource bitmapSource = skImage.ToWriteableBitmap();

            ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
            imageViewModel.Load(bitmapSource);
            await this._windowManager.ShowWindowAsync(imageViewModel);

            this.Idle();
        }
        #endregion

        #region 计算SURF特征 —— async void ComputeSURF()
        /// <summary>
        /// 计算SURF特征
        /// </summary>
        public async void ComputeSURF()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SurfViewModel viewModel = ResolveMediator.Resolve<SurfViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                using SURF surf = SURF.Create(viewModel.HessianThreshold!.Value);
                using Mat descriptors = new Mat();
                await Task.Run(() => surf.DetectAndCompute(grayImage, null, out _, descriptors));

                //绘制直方图
                using Plot plot = new Plot();
                plot.AddDescriptors(descriptors);
                using SKImage skImage = plot.GetSKImage(1280, 800);
                BitmapSource bitmapSource = skImage.ToWriteableBitmap();

                ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
                imageViewModel.Load(bitmapSource);
                await this._windowManager.ShowWindowAsync(imageViewModel);
            }

            this.Idle();
        }
        #endregion

        #region 计算ORB特征 —— async void ComputeORB()
        /// <summary>
        /// 计算ORB特征
        /// </summary>
        public async void ComputeORB()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            FastViewModel viewModel = ResolveMediator.Resolve<FastViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                using ORB orb = ORB.Create(viewModel.Threshold!.Value);
                using Mat descriptors = new Mat();
                await Task.Run(() => orb.DetectAndCompute(grayImage, null, out _, descriptors));

                //绘制直方图
                using Plot plot = new Plot();
                plot.AddDescriptors(descriptors);
                using SKImage skImage = plot.GetSKImage(1280, 800);
                BitmapSource bitmapSource = skImage.ToWriteableBitmap();

                ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
                imageViewModel.Load(bitmapSource);
                await this._windowManager.ShowWindowAsync(imageViewModel);
            }

            this.Idle();
        }
        #endregion

        #region 计算Super特征 —— async void ComputeSuper()
        /// <summary>
        /// 计算Super特征
        /// </summary>
        public async void ComputeSuper()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            const int scaledSize = 512;
            using Mat image = this.EffectiveImage.ToMat();
            using Mat scaledImage = image.ResizeAdaptively(scaledSize);
            using SuperFeature superFeature = new SuperFeature();
            using Mat descriptors = new Mat();
            await Task.Run(() => superFeature.DetectAndCompute(scaledImage, null, out _, descriptors));

            //绘制直方图
            using Plot plot = new Plot();
            plot.AddDescriptors(descriptors);
            using SKImage skImage = plot.GetSKImage(1280, 800);
            BitmapSource bitmapSource = skImage.ToWriteableBitmap();

            ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
            imageViewModel.Load(bitmapSource);
            await this._windowManager.ShowWindowAsync(imageViewModel);

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

        #region 鼠标移动事件 —— void OnImageMouseMove(Image image, MouseEventArgs eventArgs)
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public void OnImageMouseMove(Image image, MouseEventArgs eventArgs)
        {
            System.Windows.Point mousePosition = eventArgs.GetPosition(image);
            double scaleX = image.ActualWidth / image.Source.Width;
            double scaleY = image.ActualHeight / image.Source.Height;
            int x = (int)Math.Ceiling(mousePosition.X / scaleX);
            int y = (int)Math.Ceiling(mousePosition.Y / scaleY);
            this.MousePosition = new System.Windows.Point(x, y);
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
