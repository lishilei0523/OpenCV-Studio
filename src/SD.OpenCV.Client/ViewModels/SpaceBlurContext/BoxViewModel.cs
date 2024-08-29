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
    /// 方块滤波视图模型
    /// </summary>
    public class BoxViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public BoxViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 图像深度 —— int? Depth
        /// <summary>
        /// 图像深度
        /// </summary>
        [DependencyProperty]
        public int? Depth { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region 是否归一化 —— bool NeedToNormalize
        /// <summary>
        /// 是否归一化
        /// </summary>
        [DependencyProperty]
        public bool NeedToNormalize { get; set; }
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
            this.Depth = -1;
            this.KernelSize = 3;
            this.NeedToNormalize = true;

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

            if (!this.Depth.HasValue)
            {
                MessageBox.Show("图像深度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            await Task.Run(() => Cv2.BoxFilter(this.Image, result, this.Depth!.Value, kernelSize, null, this.NeedToNormalize));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
