using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Size = OpenCvSharp.Size;

namespace SD.OpenCV.Client.ViewModels.SpaceBlurContext
{
    /// <summary>
    /// 高斯滤波视图模型
    /// </summary>
    public class GaussianViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GaussianViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region 标准差 —— int? Sigma
        /// <summary>
        /// 标准差
        /// </summary>
        [DependencyProperty]
        public int? Sigma { get; set; }
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
            this.KernelSize = 3;
            this.Sigma = 1;

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

            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Size kernelSize = new Size(this.KernelSize!.Value, this.KernelSize!.Value);
            await Task.Run(() => Cv2.GaussianBlur(this.Image, result, kernelSize, this.Sigma!.Value));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
