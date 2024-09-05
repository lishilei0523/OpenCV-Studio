using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.EdgeContext
{
    /// <summary>
    /// Canny边缘检测视图模型
    /// </summary>
    public class CannyViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CannyViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 滞后性阈值1 —— double Threshold1
        /// <summary>
        /// 滞后性阈值1
        /// </summary>
        [DependencyProperty]
        public double Threshold1 { get; set; }
        #endregion

        #region 滞后性阈值2 —— double Threshold2
        /// <summary>
        /// 滞后性阈值2
        /// </summary>
        [DependencyProperty]
        public double Threshold2 { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int KernelSize { get; set; }
        #endregion

        #region 是否使用L2范式 —— bool L2Gradient
        /// <summary>
        /// 是否使用L2范式
        /// </summary>
        [DependencyProperty]
        public bool L2Gradient { get; set; }
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
            this.Threshold1 = 50;
            this.Threshold2 = 150;
            this.KernelSize = 3;
            this.L2Gradient = false;

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

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat result = await Task.Run(() => this.Image.Canny(this.Threshold1, this.Threshold2, this.KernelSize, this.L2Gradient));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 滑动阈值 —— async void SlideThreshold()
        /// <summary>
        /// 滑动阈值
        /// </summary>
        public async void SlideThreshold()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            using Mat result = await Task.Run(() => this.Image.Canny(this.Threshold1, this.Threshold2, this.KernelSize, this.L2Gradient));
            this.BitmapSource = result.ToBitmapSource();
        }
        #endregion

        #endregion
    }
}
