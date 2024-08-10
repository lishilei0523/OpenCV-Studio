using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.OpenCV.Primitives.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 颜色分割视图模型
    /// </summary>
    public class ColorViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ColorViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region H最小值 —— double MinH
        /// <summary>
        /// H最小值
        /// </summary>
        [DependencyProperty]
        public double MinH { get; set; }
        #endregion

        #region H最大值 —— double MaxH
        /// <summary>
        /// H最大值
        /// </summary>
        [DependencyProperty]
        public double MaxH { get; set; }
        #endregion

        #region S最小值 —— double MinS
        /// <summary>
        /// S最小值
        /// </summary>
        [DependencyProperty]
        public double MinS { get; set; }
        #endregion

        #region S最大值 —— double MaxS
        /// <summary>
        /// S最大值
        /// </summary>
        [DependencyProperty]
        public double MaxS { get; set; }
        #endregion

        #region V最小值 —— double MinV
        /// <summary>
        /// V最小值
        /// </summary>
        [DependencyProperty]
        public double MinV { get; set; }
        #endregion

        #region V最大值 —— double MaxV
        /// <summary>
        /// V最大值
        /// </summary>
        [DependencyProperty]
        public double MaxV { get; set; }
        #endregion

        #region HSV图像 —— Mat ImageHSV
        /// <summary>
        /// HSV图像
        /// </summary>
        public Mat ImageHSV { get; set; }
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

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.MinH = 0;
            this.MaxH = 180;
            this.MinS = 0;
            this.MaxS = 255;
            this.MinV = 0;
            this.MaxV = 255;

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(BitmapSource bitmapSource)
        {
            using Mat imageBGR = bitmapSource.ToMat();
            this.ImageHSV = imageBGR.CvtColor(ColorConversionCodes.BGR2HSV);
            this.BitmapSource = imageBGR.ToBitmapSource();
        }
        #endregion

        #region 滑动颜色通道 —— async void SlideColorChannel()
        /// <summary>
        /// 滑动颜色通道
        /// </summary>
        public async void SlideColorChannel()
        {
            Scalar lowerScalar = new Scalar(this.MinH, this.MinS, this.MinV);
            Scalar upperScalar = new Scalar(this.MaxH, this.MaxS, this.MaxV);
            using Mat resultBGR = await Task.Run(() => this.ImageHSV.ColorSegment(lowerScalar, upperScalar));
            this.BitmapSource = resultBGR.ToBitmapSource();
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
                this.ImageHSV?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
