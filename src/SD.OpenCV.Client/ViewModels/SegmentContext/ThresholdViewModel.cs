using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 阈值分割视图模型
    /// </summary>
    public class ThresholdViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ThresholdViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 阈值 —— double Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public double Threshold { get; set; }
        #endregion

        #region 最大值 —— double MaxValue
        /// <summary>
        /// 最大值
        /// </summary>
        [DependencyProperty]
        public double MaxValue { get; set; }
        #endregion

        #region 阈值分割类型 —— ThresholdTypes ThresholdType
        /// <summary>
        /// 阈值分割类型
        /// </summary>
        [DependencyProperty]
        public ThresholdTypes ThresholdType { get; set; }
        #endregion

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

        #region 阈值分割类型字典 —— IDictionary<string, string> ThresholdTypes
        /// <summary>
        /// 阈值分割类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ThresholdTypes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.Threshold = 127;
            this.MaxValue = 255;
            this.ThresholdType = OpenCvSharp.ThresholdTypes.Binary;
            this.ThresholdTypes = typeof(ThresholdTypes).GetEnumMembers();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(BitmapSource bitmapSource)
        {
            using Mat colorImage = bitmapSource.ToMat();
            this.Image = new Mat();
            Cv2.CvtColor(colorImage, this.Image, ColorConversionCodes.BGR2GRAY);
            this.BitmapSource = this.Image.ToBitmapSource();
        }
        #endregion

        #region 滑动阈值 —— async void SlideThreshold()
        /// <summary>
        /// 滑动阈值
        /// </summary>
        public async void SlideThreshold()
        {
            using Mat result = new Mat();
            await Task.Run(() => Cv2.Threshold(this.Image, result, this.Threshold, this.MaxValue, this.ThresholdType));
            this.BitmapSource = result.ToBitmapSource();
        }
        #endregion

        #region 预览图像 —— async void PreviewImage()
        /// <summary>
        /// 预览图像
        /// </summary>
        public async void PreviewImage()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(this.BitmapSource);
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 提交 —— async void Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async void Submit()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
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
