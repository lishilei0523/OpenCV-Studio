﻿using Caliburn.Micro;
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
using SD.OpenCV.Client.ViewModels.FrequencyBlurContext;
using SD.OpenCV.Client.ViewModels.GeometryContext;
using SD.OpenCV.Client.ViewModels.GrayscaleContext;
using SD.OpenCV.Client.ViewModels.HistogramContext;
using SD.OpenCV.Client.ViewModels.MatchContext;
using SD.OpenCV.Client.ViewModels.MorphContext;
using SD.OpenCV.Client.ViewModels.RectifyContext;
using SD.OpenCV.Client.ViewModels.SegmentContext;
using SD.OpenCV.Client.ViewModels.ShapeContext;
using SD.OpenCV.Client.ViewModels.SpaceBlurContext;
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Extensions;
using SD.OpenCV.Primitives.Models;
using SD.OpenCV.Reconstructions;
using SD.OpenCV.SkiaSharp;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Colors = System.Windows.Media.Colors;

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

        #region 重置图像 —— async void ResetImage()
        /// <summary>
        /// 重置图像
        /// </summary>
        public async void ResetImage()
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
            viewModel.Load(this.OriginalImage, "原始图像");
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
            viewModel.Load(this.EffectiveImage, "效果图像");
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

        #region 技术支持 —— void Support()
        /// <summary>
        /// 技术支持
        /// </summary>
        public void Support()
        {
            Process.Start("https://gitee.com/lishilei0523/OpenCV-Studio");
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

        #region 关闭图像 —— async void CloseImage()
        /// <summary>
        /// 关闭图像
        /// </summary>
        public async void CloseImage()
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
                this.Busy();

                using Mat image = this.EffectiveImage.ToMat();
                await Task.Run(() => image.SaveImage(this.FilePath));
                await this.ReloadImage();

                this.Idle();
                this.ToastSuccess("保存成功！");
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

            MessageBoxResult result = MessageBox.Show("确定保存并覆盖源图像？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                this.Busy();

                using Mat image = this.EffectiveImage.ToMat();
                await Task.Run(() => image.SaveImage(this.FilePath));
                await this.ReloadImage();

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
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
                await Task.Run(() => image.SaveImage(saveFileDialog.FileName));

                this.Idle();
                this.ToastSuccess("保存成功！");
            }
        }
        #endregion


        //工具

        #region 仿射变换 —— async void AffineTransform()
        /// <summary>
        /// 仿射变换
        /// </summary>
        public async void AffineTransform()
        {
            this.Busy();

            AffineViewModel viewModel = ResolveMediator.Resolve<AffineViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 透视变换 —— async void PerspectiveTransform()
        /// <summary>
        /// 透视变换
        /// </summary>
        public async void PerspectiveTransform()
        {
            this.Busy();

            PerspectiveViewModel viewModel = ResolveMediator.Resolve<PerspectiveViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 曝光融合 —— async void ExposureFusion()
        /// <summary>
        /// 曝光融合
        /// </summary>
        public async void ExposureFusion()
        {
            this.Busy();

            ExposureViewModel viewModel = ResolveMediator.Resolve<ExposureViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 模板匹配 —— async void TemplateMatch()
        /// <summary>
        /// 模板匹配
        /// </summary>
        public async void TemplateMatch()
        {
            this.Busy();

            TemplateViewModel viewModel = ResolveMediator.Resolve<TemplateViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 特征匹配 —— async void FeatureMatch()
        /// <summary>
        /// 特征匹配
        /// </summary>
        public async void FeatureMatch()
        {
            this.Busy();

            FeatureViewModel viewModel = ResolveMediator.Resolve<FeatureViewModel>();
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

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


        //颜色空间

        #region BRG转换GRAY —— async void BGRToGray()
        /// <summary>
        /// BRG转换GRAY
        /// </summary>
        public async void BGRToGray()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region GRAY转换BGR —— async void GrayToBGR()
        /// <summary>
        /// GRAY转换BGR
        /// </summary>
        public async void GrayToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.GRAY2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region BGR转换HSV —— async void BGRToHSV()
        /// <summary>
        /// BGR转换HSV
        /// </summary>
        public async void BGRToHSV()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region HSV转换BGR —— async void HSVToBGR()
        /// <summary>
        /// HSV转换BGR
        /// </summary>
        public async void HSVToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为HSV三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat hsvImage = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => hsvImage.CvtColor(ColorConversionCodes.HSV2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region BGR转换HLS —— async void BGRToHLS()
        /// <summary>
        /// BGR转换HLS
        /// </summary>
        public async void BGRToHLS()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region HLS转换BGR —— async void HLSToBGR()
        /// <summary>
        /// HLS转换BGR
        /// </summary>
        public async void HLSToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为HLS三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.HLS2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region BGR转换Lab —— async void BGRToLab()
        /// <summary>
        /// BGR转换Lab
        /// </summary>
        public async void BGRToLab()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region Lab转换BGR —— async void LabToBGR()
        /// <summary>
        /// Lab转换BGR
        /// </summary>
        public async void LabToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为Lab三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.Lab2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region BGR转换Luv —— async void BGRToLuv()
        /// <summary>
        /// BGR转换Luv
        /// </summary>
        public async void BGRToLuv()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region Luv转换BGR —— async void LuvToBGR()
        /// <summary>
        /// Luv转换BGR
        /// </summary>
        public async void LuvToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为Luv三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.Luv2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region BGR转换YCrCb —— async void BGRToYCrCb()
        /// <summary>
        /// BGR转换YCrCb
        /// </summary>
        public async void BGRToYCrCb()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region YCrCb转换BGR —— async void YCrCbToBGR()
        /// <summary>
        /// YCrCb转换BGR
        /// </summary>
        public async void YCrCbToBGR()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为YCrCb三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.EffectiveImage.ToMat();
            using Mat bgrImage = await Task.Run(() => image.CvtColor(ColorConversionCodes.YCrCb2BGR));
            this.EffectiveImage = bgrImage.ToBitmapSource();

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

                #region # 验证

                if (source.Size() != target.Size())
                {
                    MessageBox.Show("源图像与目标图像尺寸不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Idle();
                    return;
                }
                if (source.Channels() != target.Channels())
                {
                    MessageBox.Show("源图像与目标图像通道数不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Idle();
                    return;
                }

                #endregion

                using Mat result = new Mat();
                await Task.Run(() => Cv2.Add(source, target, result));
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

                #region # 验证

                if (source.Size() != target.Size())
                {
                    MessageBox.Show("源图像与目标图像尺寸不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Idle();
                    return;
                }
                if (source.Channels() != target.Channels())
                {
                    MessageBox.Show("源图像与目标图像通道数不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Idle();
                    return;
                }

                #endregion

                using Mat result = new Mat();
                await Task.Run(() => Cv2.Subtract(source, target, result));
                this.EffectiveImage = result.ToBitmapSource();

                this.Idle();
            }
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

            ErodeViewModel viewModel = ResolveMediator.Resolve<ErodeViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            DilateViewModel viewModel = ResolveMediator.Resolve<DilateViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            OpenViewModel viewModel = ResolveMediator.Resolve<OpenViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            CloseViewModel viewModel = ResolveMediator.Resolve<CloseViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            TopHatViewModel viewModel = ResolveMediator.Resolve<TopHatViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            BlackHatViewModel viewModel = ResolveMediator.Resolve<BlackHatViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            GradientViewModel viewModel = ResolveMediator.Resolve<GradientViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            HitMissViewModel viewModel = ResolveMediator.Resolve<HitMissViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 形态学综合 —— async void MorphologyEx()
        /// <summary>
        /// 形态学综合
        /// </summary>
        public async void MorphologyEx()
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 均值漂移滤波 —— async void MeanShiftBlur()
        /// <summary>
        /// 均值漂移滤波
        /// </summary>
        public async void MeanShiftBlur()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            MeanShiftViewModel viewModel = ResolveMediator.Resolve<MeanShiftViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealLowViewModel viewModel = ResolveMediator.Resolve<IdealLowViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealHighViewModel viewModel = ResolveMediator.Resolve<IdealHighViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealBandPViewModel viewModel = ResolveMediator.Resolve<IdealBandPViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IdealBandRViewModel viewModel = ResolveMediator.Resolve<IdealBandRViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianLowViewModel viewModel = ResolveMediator.Resolve<GaussianLowViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianHighViewModel viewModel = ResolveMediator.Resolve<GaussianHighViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianBandPViewModel viewModel = ResolveMediator.Resolve<GaussianBandPViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianBandRViewModel viewModel = ResolveMediator.Resolve<GaussianBandRViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GaussianHomoViewModel viewModel = ResolveMediator.Resolve<GaussianHomoViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthLowViewModel viewModel = ResolveMediator.Resolve<ButterworthLowViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthHighViewModel viewModel = ResolveMediator.Resolve<ButterworthHighViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthBandPViewModel viewModel = ResolveMediator.Resolve<ButterworthBandPViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthBandRViewModel viewModel = ResolveMediator.Resolve<ButterworthBandRViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            if (this.EffectiveImage.Format != PixelFormats.Gray8)
            {
                MessageBox.Show("图像必须为单通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ButterworthHomoViewModel viewModel = ResolveMediator.Resolve<ButterworthHomoViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
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

            DistanceViewModel viewModel = ResolveMediator.Resolve<DistanceViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            using Mat image = this.EffectiveImage.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
            using Mat histImage = await Task.Run(() => grayImage.GenerateHistogramImage(1440, 870));
            BitmapSource bitmapSource = histImage.ToBitmapSource();
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(bitmapSource, "灰度直方图");
            await this._windowManager.ShowWindowAsync(viewModel);

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

            using Mat image = this.EffectiveImage.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
            using Mat result = new Mat();
            await Task.Run(() => Cv2.EqualizeHist(grayImage, result));
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 自适应直方图均衡化 —— async void AdaptiveEqualizeHist()
        /// <summary>
        /// 自适应直方图均衡化
        /// </summary>
        public async void AdaptiveEqualizeHist()
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion


        //频谱图

        #region 查看幅度谱图 —— async void LookMagnitudeSpectrum()
        /// <summary>
        /// 查看幅度谱图
        /// </summary>
        public async void LookMagnitudeSpectrum()
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
            using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
            using Mat spectrumImage = await Task.Run(() => grayImage.GenerateMagnitudeSpectrum());
            BitmapSource bitmapSource = spectrumImage.ToBitmapSource();
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(bitmapSource, "幅度谱图");
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 查看相位谱图 —— async void LookPhaseSpectrum()
        /// <summary>
        /// 查看相位谱图
        /// </summary>
        public async void LookPhaseSpectrum()
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
            using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
            using Mat spectrumImage = await Task.Run(() => grayImage.GeneratePhaseSpectrum());
            BitmapSource bitmapSource = spectrumImage.ToBitmapSource();
            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(bitmapSource, "相位谱图");
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            using Mat image = this.EffectiveImage.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
            using Mat result = await Task.Run(() => grayImage.ApplyRobert());
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region Laplacian边缘检测 —— async void ApplyLaplacian()
        /// <summary>
        /// Laplacian边缘检测
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion


        //形状查找

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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 轮廓查找 —— async void FindContours()
        /// <summary>
        /// 轮廓查找
        /// </summary>
        public async void FindContours()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ShapeContext.ContourViewModel viewModel = ResolveMediator.Resolve<ShapeContext.ContourViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 上采样 —— async void PyramidUp()
        /// <summary>
        /// 上采样
        /// </summary>
        public async void PyramidUp()
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
            using Mat result = new Mat();
            await Task.Run(() => Cv2.PyrUp(image, result));
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 下采样 —— async void PyramidDown()
        /// <summary>
        /// 下采样
        /// </summary>
        public async void PyramidDown()
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
            using Mat result = new Mat();
            await Task.Run(() => Cv2.PyrDown(image, result));
            this.EffectiveImage = result.ToBitmapSource();

            this.Idle();
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

            KeyPointContext.HarrisViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.HarrisViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            KeyPointContext.ShiTomasiViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.ShiTomasiViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            KeyPointContext.SiftViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.SiftViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

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

            KeyPointContext.SurfViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.SurfViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            KeyPointContext.FastViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.FastViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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

            KeyPointContext.OrbViewModel viewModel = ResolveMediator.Resolve<KeyPointContext.OrbViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
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
            using Mat descriptors = new Mat();
            KeyPoint[] keyPoints = { };
            await Task.Run(() => Reconstructor.Feature.DetectAndCompute(scaledImage, null, out keyPoints, descriptors));
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

            FeatureContext.SiftViewModel viewModel = ResolveMediator.Resolve<FeatureContext.SiftViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
                using SIFT sift = SIFT.Create(viewModel.NFeatures!.Value, viewModel.NOctaveLayers!.Value, viewModel.ContrastThreshold!.Value, viewModel.EdgeThreshold!.Value, viewModel.Sigma!.Value);
                using Mat descriptors = new Mat();
                await Task.Run(() => sift.DetectAndCompute(grayImage, null, out _, descriptors));

                //绘制直方图
                using Plot plot = new Plot();
                await Task.Run(() => plot.AddDescriptors(descriptors));
                using SKImage skImage = await Task.Run(() => plot.GetSKImage(1440, 870));
                BitmapSource bitmapSource = skImage.ToWriteableBitmap();

                ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
                imageViewModel.Load(bitmapSource, "SIFT特征直方图");
                await this._windowManager.ShowWindowAsync(imageViewModel);
            }

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

            FeatureContext.SurfViewModel viewModel = ResolveMediator.Resolve<FeatureContext.SurfViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
                using SURF surf = SURF.Create(viewModel.HessianThreshold!.Value);
                using Mat descriptors = new Mat();
                await Task.Run(() => surf.DetectAndCompute(grayImage, null, out _, descriptors));

                //绘制直方图
                using Plot plot = new Plot();
                await Task.Run(() => plot.AddDescriptors(descriptors));
                using SKImage skImage = await Task.Run(() => plot.GetSKImage(1440, 870));
                BitmapSource bitmapSource = skImage.ToWriteableBitmap();

                ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
                imageViewModel.Load(bitmapSource, "SURF特征直方图");
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

            FeatureContext.OrbViewModel viewModel = ResolveMediator.Resolve<FeatureContext.OrbViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                using Mat image = this.EffectiveImage.ToMat();
                using Mat grayImage = image.Type() == MatType.CV_8UC3 ? image.CvtColor(ColorConversionCodes.BGR2GRAY) : image;
                using ORB orb = ORB.Create(viewModel.Threshold!.Value);
                using Mat descriptors = new Mat();
                await Task.Run(() => orb.DetectAndCompute(grayImage, null, out _, descriptors));

                //绘制直方图
                using Plot plot = new Plot();
                await Task.Run(() => plot.AddDescriptors(descriptors));
                using SKImage skImage = await Task.Run(() => plot.GetSKImage(1440, 870));
                BitmapSource bitmapSource = skImage.ToWriteableBitmap();

                ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
                imageViewModel.Load(bitmapSource, "ORB特征直方图");
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
            using Mat descriptors = new Mat();
            await Task.Run(() => Reconstructor.Feature.DetectAndCompute(scaledImage, null, out _, descriptors));

            //绘制直方图
            using Plot plot = new Plot();
            await Task.Run(() => plot.AddDescriptors(descriptors));
            using SKImage skImage = await Task.Run(() => plot.GetSKImage(1440, 870));
            BitmapSource bitmapSource = skImage.ToWriteableBitmap();

            ImageViewModel imageViewModel = ResolveMediator.Resolve<ImageViewModel>();
            imageViewModel.Load(bitmapSource, "Super特征直方图");
            await this._windowManager.ShowWindowAsync(imageViewModel);

            this.Idle();
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


        //分割

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

        #region OTSU阈值分割 —— async void OtsuThresholdSegment()
        /// <summary>
        /// OTSU阈值分割
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

            OtsuThresholdViewModel viewModel = ResolveMediator.Resolve<OtsuThresholdViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region Triangle阈值分割 —— async void TriangleThresholdSegment()
        /// <summary>
        /// Triangle阈值分割
        /// </summary>
        public async void TriangleThresholdSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            TriangleThresholdViewModel viewModel = ResolveMediator.Resolve<TriangleThresholdViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 自适应阈值分割 —— async void AdaptiveThresholdSegment()
        /// <summary>
        /// 自适应阈值分割
        /// </summary>
        public async void AdaptiveThresholdSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            AdThresholdViewModel viewModel = ResolveMediator.Resolve<AdThresholdViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

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
            if (this.EffectiveImage.Format != PixelFormats.Bgr24)
            {
                MessageBox.Show("图像必须为BGR三通道！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region 轮廓分割 —— async void ContourSegment()
        /// <summary>
        /// 轮廓分割
        /// </summary>
        public async void ContourSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SegmentContext.ContourViewModel viewModel = ResolveMediator.Resolve<SegmentContext.ContourViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
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

        #region GrabCut分割 —— async void GrabCutSegment()
        /// <summary>
        /// GrabCut分割
        /// </summary>
        public async void GrabCutSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GrabCutViewModel viewModel = ResolveMediator.Resolve<GrabCutViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region K-Means聚类分割 —— async void KMeansSegment()
        /// <summary>
        /// K-Means聚类分割
        /// </summary>
        public async void KMeansSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            KMeansViewModel viewModel = ResolveMediator.Resolve<KMeansViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

        #region 图分割 —— async void GraphSegment()
        /// <summary>
        /// 图分割
        /// </summary>
        public async void GraphSegment()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            GraphViewModel viewModel = ResolveMediator.Resolve<GraphViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion


        //矫正

        #region 矫正污点 —— async void RectifyStains()
        /// <summary>
        /// 矫正污点
        /// </summary>
        public async void RectifyStains()
        {
            #region # 验证

            if (this.EffectiveImage == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            InpaintViewModel viewModel = ResolveMediator.Resolve<InpaintViewModel>();
            viewModel.Load(this.EffectiveImage);
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.EffectiveImage = viewModel.BitmapSource;
            }

            this.Idle();
        }
        #endregion

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
                using Mat image = this.EffectiveImage.ToMat();
                using Mat rectifiedImage = await Task.Run(() => image.RectifyDistortions(cameraIntrinsics));
                this.EffectiveImage = rectifiedImage.ToBitmapSource();

                this.Idle();
            }
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

                this.ResetImage();
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
