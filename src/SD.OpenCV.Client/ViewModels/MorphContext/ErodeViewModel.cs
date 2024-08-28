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

namespace SD.OpenCV.Client.ViewModels.MorphContext
{
    /// <summary>
    /// 腐蚀视图模型
    /// </summary>
    public class ErodeViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ErodeViewModel(IWindowManager windowManager)
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
            this.KernelSize = 3;

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

            using Mat result = await Task.Run(() => this.Image.MorphErode(this.KernelSize!.Value));
            this.BitmapSource = result.ToBitmapSource();

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
