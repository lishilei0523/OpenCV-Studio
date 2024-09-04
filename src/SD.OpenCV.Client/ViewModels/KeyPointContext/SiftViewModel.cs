using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.KeyPointContext
{
    /// <summary>
    /// SIFT关键点视图模型
    /// </summary>
    public class SiftViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SiftViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 保留特征数 —— int? NFeatures
        /// <summary>
        /// 保留特征数
        /// </summary>
        [DependencyProperty]
        public int? NFeatures { get; set; }
        #endregion

        #region 每层DoG个数 —— int? NOctaveLayers
        /// <summary>
        /// 每层DoG个数
        /// </summary>
        /// <remarks>DoG: Difference of Gaussian</remarks>
        [DependencyProperty]
        public int? NOctaveLayers { get; set; }
        #endregion

        #region 对比度阈值 —— double? ContrastThreshold
        /// <summary>
        /// 对比度阈值
        /// </summary>
        [DependencyProperty]
        public double? ContrastThreshold { get; set; }
        #endregion

        #region 边缘阈值 —— double? EdgeThreshold
        /// <summary>
        /// 边缘阈值
        /// </summary>
        [DependencyProperty]
        public double? EdgeThreshold { get; set; }
        #endregion

        #region 0层高斯因子 —— double? Sigma
        /// <summary>
        /// 0层高斯因子
        /// </summary>
        [DependencyProperty]
        public double? Sigma { get; set; }
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
            this.NFeatures = 0;
            this.NOctaveLayers = 3;
            this.ContrastThreshold = 0.04;
            this.EdgeThreshold = 10;
            this.Sigma = 1.6;

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 应用 —— async void Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public async void Apply()
        {
            #region # 验证

            if (!this.NFeatures.HasValue)
            {
                MessageBox.Show("保留特征数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.NOctaveLayers.HasValue)
            {
                MessageBox.Show("每层DoG个数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.ContrastThreshold.HasValue)
            {
                MessageBox.Show("对比度阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.EdgeThreshold.HasValue)
            {
                MessageBox.Show("边缘阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("0层高斯因子不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            using SIFT sift = SIFT.Create(this.NFeatures!.Value, this.NOctaveLayers!.Value, this.ContrastThreshold!.Value, this.EdgeThreshold!.Value, this.Sigma!.Value);
            using Mat descriptors = new Mat();
            KeyPoint[] keyPoints = { };
            await Task.Run(() => sift.DetectAndCompute(grayImage, null, out keyPoints, descriptors));

            //绘制关键点
            await Task.Run(() => Cv2.DrawKeypoints(colorImage, keyPoints, colorImage, Scalar.Red));
            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
