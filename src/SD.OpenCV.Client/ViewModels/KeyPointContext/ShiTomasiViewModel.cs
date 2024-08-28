using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Point = OpenCvSharp.Point;

namespace SD.OpenCV.Client.ViewModels.KeyPointContext
{
    /// <summary>
    /// Shi-Tomasi关键点视图模型
    /// </summary>
    public class ShiTomasiViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ShiTomasiViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 最大关键点数 —— int? MaxCorners
        /// <summary>
        /// 最大关键点数
        /// </summary>
        [DependencyProperty]
        public int? MaxCorners { get; set; }
        #endregion

        #region 质量级别 —— double? QualityLevel
        /// <summary>
        /// 质量级别
        /// </summary>
        /// <remarks>取值范围: [0.01, 0.1]</remarks>
        [DependencyProperty]
        public double? QualityLevel { get; set; }
        #endregion

        #region 最小距离 —— double? MinDistance
        /// <summary>
        /// 最小距离
        /// </summary>
        [DependencyProperty]
        public double? MinDistance { get; set; }
        #endregion

        #region 块尺寸 —— int? BlockSize
        /// <summary>
        /// 块尺寸
        /// </summary>
        [DependencyProperty]
        public int? BlockSize { get; set; }
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

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.MaxCorners = 1000;
            this.QualityLevel = 0.01;
            this.MinDistance = 10;
            this.BlockSize = 3;

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(BitmapSource bitmapSource)
        {
            this.BitmapSource = bitmapSource;
            this.Image = bitmapSource.ToMat();
        }
        #endregion

        #region 应用 —— async void Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public async void Apply()
        {
            #region # 验证

            if (!this.MaxCorners.HasValue)
            {
                MessageBox.Show("最大关键点数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.QualityLevel.HasValue)
            {
                MessageBox.Show("质量级别不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinDistance.HasValue)
            {
                MessageBox.Show("最小距离不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.BlockSize.HasValue)
            {
                MessageBox.Show("块尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat colorImage = this.Image.Clone();
            using Mat grayImage = this.Image.Type() == MatType.CV_8UC3
                ? this.Image.CvtColor(ColorConversionCodes.BGR2GRAY)
                : this.Image.Clone();
            Point2f[] points = await Task.Run(() => Cv2.GoodFeaturesToTrack(grayImage, this.MaxCorners!.Value, this.QualityLevel!.Value, this.MinDistance!.Value, null!, this.BlockSize!.Value, false, 0));

            //绘制关键点
            foreach (Point point in points)
            {
                Cv2.Circle(colorImage, point, 2, Scalar.Red);
            }
            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 提交 —— async void Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async void Submit()
        {
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
