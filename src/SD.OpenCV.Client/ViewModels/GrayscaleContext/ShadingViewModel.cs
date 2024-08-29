using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Size = OpenCvSharp.Size;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 阴影变换视图模型
    /// </summary>
    public class ShadingViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ShadingViewModel(IWindowManager windowManager)
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

        #region 增益 —— byte? Gain
        /// <summary>
        /// 增益
        /// </summary>
        [DependencyProperty]
        public byte? Gain { get; set; }
        #endregion

        #region 噪声 —— byte? Noise
        /// <summary>
        /// 噪声
        /// </summary>
        [DependencyProperty]
        public byte? Noise { get; set; }
        #endregion

        #region 亮度补偿 —— byte? Offset
        /// <summary>
        /// 亮度补偿
        /// </summary>
        [DependencyProperty]
        public byte? Offset { get; set; }
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
            this.Gain = 60;
            this.Noise = 0;
            this.Offset = 140;

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
            if (!this.Gain.HasValue)
            {
                MessageBox.Show("增益不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Noise.HasValue)
            {
                MessageBox.Show("噪声不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Offset.HasValue)
            {
                MessageBox.Show("亮度补偿不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            Size kernelSize = new Size(this.KernelSize!.Value, this.KernelSize!.Value);
            using Mat result = await Task.Run(() => this.Image.ShadingTransform(kernelSize, this.Gain!.Value, this.Noise!.Value, this.Offset!.Value));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
