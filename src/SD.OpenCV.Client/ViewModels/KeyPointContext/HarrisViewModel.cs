using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Point = OpenCvSharp.Point;

namespace SD.OpenCV.Client.ViewModels.KeyPointContext
{
    /// <summary>
    /// Harris关键点视图模型
    /// </summary>
    public class HarrisViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public HarrisViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 块尺寸 —— int? BlockSize
        /// <summary>
        /// 块尺寸
        /// </summary>
        [DependencyProperty]
        public int? BlockSize { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region 自由参数 —— double? K
        /// <summary>
        /// 自由参数
        /// </summary>
        /// <remarks>取值范围: [0.04, 0.06]</remarks>
        [DependencyProperty]
        public double? K { get; set; }
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
            this.BlockSize = 2;
            this.KernelSize = 3;
            this.K = 0.04;

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

            if (!this.BlockSize.HasValue)
            {
                MessageBox.Show("块尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.K.HasValue)
            {
                MessageBox.Show("自由参数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Point[] points = await Task.Run(() => grayImage.DetectHarris(this.BlockSize!.Value, this.KernelSize!.Value, this.K!.Value));

            //绘制关键点
            foreach (Point point in points)
            {
                Cv2.Circle(colorImage, point, 2, Scalar.Red);
            }
            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
