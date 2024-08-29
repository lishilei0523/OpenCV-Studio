using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.EdgeContext
{
    /// <summary>
    /// Scharr边缘检测视图模型
    /// </summary>
    public class ScharrViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ScharrViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region X轴卷积权重 —— double? Alpha
        /// <summary>
        /// X轴卷积权重
        /// </summary>
        [DependencyProperty]
        public double? Alpha { get; set; }
        #endregion

        #region Y轴卷积权重 —— double? Beta
        /// <summary>
        /// Y轴卷积权重
        /// </summary>
        [DependencyProperty]
        public double? Beta { get; set; }
        #endregion

        #region 伽马值 —— double? Gamma
        /// <summary>
        /// 伽马值
        /// </summary>
        [DependencyProperty]
        public double? Gamma { get; set; }
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
            this.Alpha = 0.5f;
            this.Beta = 0.5f;
            this.Gamma = 0;

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
                MessageBox.Show("X轴卷积权重不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Beta.HasValue)
            {
                MessageBox.Show("Y轴卷积权重不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Gamma.HasValue)
            {
                MessageBox.Show("伽马值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat result = await Task.Run(() => this.Image.ApplyScharr(this.Alpha!.Value, this.Beta!.Value, this.Gamma!.Value));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
