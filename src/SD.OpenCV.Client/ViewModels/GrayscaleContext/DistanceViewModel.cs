using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 距离变换视图模型
    /// </summary>
    public class DistanceViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public DistanceViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 距离类型 —— DistanceTypes DistanceType
        /// <summary>
        /// 距离类型
        /// </summary>
        [DependencyProperty]
        public DistanceTypes DistanceType { get; set; }
        #endregion

        #region 距离类型字典 —— IDictionary<string, string> DistanceTypes
        /// <summary>
        /// 距离类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> DistanceTypes { get; set; }
        #endregion

        #region 掩膜尺寸类型 —— DistanceTransformMasks DistanceTransformMask
        /// <summary>
        /// 掩膜尺寸类型
        /// </summary>
        [DependencyProperty]
        public DistanceTransformMasks DistanceTransformMask { get; set; }
        #endregion

        #region 掩膜尺寸类型字典 —— IDictionary<string, string> DistanceTransformMasks
        /// <summary>
        /// 掩膜尺寸类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> DistanceTransformMasks { get; set; }
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
            this.DistanceType = OpenCvSharp.DistanceTypes.L2;
            this.DistanceTransformMask = OpenCvSharp.DistanceTransformMasks.Mask3;
            this.DistanceTypes = typeof(DistanceTypes).GetEnumMembers();
            this.DistanceTransformMasks = typeof(DistanceTransformMasks).GetEnumMembers();

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

            this.Busy();

            using Mat result = await Task.Run(() => this.Image.DistanceTrans(this.DistanceType, this.DistanceTransformMask));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
