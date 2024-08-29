using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.SpaceBlurContext
{
    /// <summary>
    /// 双边滤波视图模型
    /// </summary>
    public class BilateralViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public BilateralViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 滤波直径 —— int? Diameter
        /// <summary>
        /// 滤波直径
        /// </summary>
        [DependencyProperty]
        public int? Diameter { get; set; }
        #endregion

        #region 颜色标准差 —— double? SigmaColor
        /// <summary>
        /// 颜色标准差
        /// </summary>
        [DependencyProperty]
        public double? SigmaColor { get; set; }
        #endregion

        #region 空间标准差 —— double? SigmaSpace
        /// <summary>
        /// 空间标准差
        /// </summary>
        [DependencyProperty]
        public double? SigmaSpace { get; set; }
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
            this.Diameter = 9;
            this.SigmaColor = 75;
            this.SigmaSpace = 75;

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

            if (!this.Diameter.HasValue)
            {
                MessageBox.Show("滤波直径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.SigmaColor.HasValue)
            {
                MessageBox.Show("颜色标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.SigmaSpace.HasValue)
            {
                MessageBox.Show("空间标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat result = new Mat();
            await Task.Run(() => Cv2.BilateralFilter(this.Image, result, this.Diameter!.Value, this.SigmaColor!.Value, this.SigmaSpace!.Value));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
