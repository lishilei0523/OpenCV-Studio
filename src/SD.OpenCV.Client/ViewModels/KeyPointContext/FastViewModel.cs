using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.KeyPointContext
{
    /// <summary>
    /// FAST关键点视图模型
    /// </summary>
    public class FastViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public FastViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 阈值 —— int? Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public int? Threshold { get; set; }
        #endregion

        #region 非极大值抑制 —— bool NonmaxSuppression
        /// <summary>
        /// 非极大值抑制
        /// </summary>
        [DependencyProperty]
        public bool NonmaxSuppression { get; set; }
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
            this.Threshold = 100;
            this.NonmaxSuppression = true;

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

            if (!this.Threshold.HasValue)
            {
                MessageBox.Show("阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            using FastFeatureDetector fast = FastFeatureDetector.Create(this.Threshold!.Value, this.NonmaxSuppression);
            KeyPoint[] keyPoints = await Task.Run(() => fast.Detect(grayImage));

            //绘制关键点
            await Task.Run(() => Cv2.DrawKeypoints(colorImage, keyPoints, colorImage, Scalar.Red));

            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
