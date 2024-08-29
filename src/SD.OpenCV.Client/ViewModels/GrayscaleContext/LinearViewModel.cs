using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 线性变换视图模型
    /// </summary>
    public class LinearViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LinearViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 对比度 —— float? Alpha
        /// <summary>
        /// 对比度
        /// </summary>
        [DependencyProperty]
        public float? Alpha { get; set; }
        #endregion

        #region 亮度 —— float? Beta
        /// <summary>
        /// 亮度
        /// </summary>
        [DependencyProperty]
        public float? Beta { get; set; }
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
            this.Alpha = 1;
            this.Beta = 30;

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

            if (!this.Alpha.HasValue)
            {
                MessageBox.Show("对比度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Beta.HasValue)
            {
                MessageBox.Show("亮度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat result = await Task.Run(() => this.Image.LinearTransform(this.Alpha!.Value, this.Beta!.Value));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
