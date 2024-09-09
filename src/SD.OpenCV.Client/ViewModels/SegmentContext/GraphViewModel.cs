using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XImgProc.Segmentation;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 图分割视图模型
    /// </summary>
    public class GraphViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GraphViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 滤波半径 —— double? Sigma
        /// <summary>
        /// 滤波半径
        /// </summary>
        [DependencyProperty]
        public double? Sigma { get; set; }
        #endregion

        #region 合并区域数量 —— float? K
        /// <summary>
        /// 合并区域数量
        /// </summary>
        [DependencyProperty]
        public float? K { get; set; }
        #endregion

        #region 最小区域尺寸 —— int? MinSize
        /// <summary>
        /// 最小区域尺寸
        /// </summary>
        [DependencyProperty]
        public int? MinSize { get; set; }
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
            this.Sigma = 0.5;
            this.K = 300;
            this.MinSize = 100;

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

            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("滤波半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.K.HasValue)
            {
                MessageBox.Show("合并区域数量不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinSize.HasValue)
            {
                MessageBox.Show("最小区域尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using GraphSegmentation graphSegmentation = GraphSegmentation.Create(this.Sigma!.Value, this.K!.Value, this.MinSize!.Value);
            using Mat result = new Mat();
            await Task.Run(() => graphSegmentation.ProcessImage(this.Image, result));
            result.Normalize(0, 255, NormTypes.MinMax);
            result.ConvertTo(result, MatType.CV_8UC1);
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
