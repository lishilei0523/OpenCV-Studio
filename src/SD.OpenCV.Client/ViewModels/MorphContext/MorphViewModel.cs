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

namespace SD.OpenCV.Client.ViewModels.MorphContext
{
    /// <summary>
    /// 形态学操作视图模型
    /// </summary>
    public class MorphViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MorphViewModel(IWindowManager windowManager)
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

        #region 形态学类型 —— MorphTypes MorphType
        /// <summary>
        /// 形态学类型
        /// </summary>
        [DependencyProperty]
        public MorphTypes MorphType { get; set; }
        #endregion

        #region 形态学类型字典 —— IDictionary<string, string> MorphTypes
        /// <summary>
        /// 形态学类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> MorphTypes { get; set; }
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
            this.MorphType = OpenCvSharp.MorphTypes.Erode;
            this.MorphTypes = typeof(MorphTypes).GetEnumMembers();

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
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.MorphType == OpenCvSharp.MorphTypes.HitMiss
                ? this.Image.Type() == MatType.CV_8UC3 ? this.Image.CvtColor(ColorConversionCodes.BGR2GRAY) : this.Image.Clone()
                : this.Image.Clone();
            using Mat kernel = Mat.Ones(this.KernelSize!.Value, this.KernelSize!.Value, MatType.CV_8UC1);
            using Mat result = new Mat();
            await Task.Run(() => Cv2.MorphologyEx(image, result, this.MorphType, kernel));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
