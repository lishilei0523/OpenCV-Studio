using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 自适应阈值分割视图模型
    /// </summary>
    public class AdThresholdViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public AdThresholdViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 最大值 —— double MaxValue
        /// <summary>
        /// 最大值
        /// </summary>
        [DependencyProperty]
        public double MaxValue { get; set; }
        #endregion

        #region 块大小 —— int BlockSize
        /// <summary>
        /// 块大小
        /// </summary>
        [DependencyProperty]
        public int BlockSize { get; set; }
        #endregion

        #region 阈值常数 —— double C
        /// <summary>
        /// 阈值常数
        /// </summary>
        [DependencyProperty]
        public double C { get; set; }
        #endregion

        #region 阈值分割类型 —— ThresholdTypes ThresholdType
        /// <summary>
        /// 阈值分割类型
        /// </summary>
        [DependencyProperty]
        public ThresholdTypes ThresholdType { get; set; }
        #endregion

        #region 阈值分割类型字典 —— IDictionary<string, string> ThresholdTypes
        /// <summary>
        /// 阈值分割类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ThresholdTypes { get; set; }
        #endregion

        #region 自适应阈值分割类型 —— AdaptiveThresholdTypes AdaptiveThresholdType
        /// <summary>
        /// 自适应阈值分割类型
        /// </summary>
        [DependencyProperty]
        public AdaptiveThresholdTypes AdaptiveThresholdType { get; set; }
        #endregion

        #region 自适应阈值分割类型字典 —— IDictionary<string, string> AdaptiveThresholdTypes
        /// <summary>
        /// 自适应阈值分割类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> AdaptiveThresholdTypes { get; set; }
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
            this.MaxValue = 255;
            this.BlockSize = 3;
            this.C = 1;
            this.ThresholdType = OpenCvSharp.ThresholdTypes.Binary;
            this.AdaptiveThresholdType = OpenCvSharp.AdaptiveThresholdTypes.GaussianC;
            this.ThresholdTypes = typeof(ThresholdTypes).GetEnumMembers();
            this.AdaptiveThresholdTypes = typeof(AdaptiveThresholdTypes).GetEnumMembers();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— override void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public override void Load(BitmapSource bitmapSource)
        {
            Mat image = bitmapSource.ToMat();
            if (image.Type() == MatType.CV_8UC3)
            {
                this.Image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                image.Dispose();
            }
            else
            {
                this.Image = image;
            }
            this.BitmapSource = bitmapSource;
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

            using Mat result = new Mat();
            await Task.Run(() => Cv2.AdaptiveThreshold(this.Image, result, this.MaxValue, this.AdaptiveThresholdType, this.ThresholdType, this.BlockSize, this.C));
            this.BitmapSource = result.ToBitmapSource();
        }
        #endregion

        #endregion
    }
}
