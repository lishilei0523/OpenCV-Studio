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

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// K-Means聚类分割视图模型
    /// </summary>
    public class KMeansViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public KMeansViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 簇数量 —— int? ClustersCount
        /// <summary>
        /// 簇数量
        /// </summary>
        [DependencyProperty]
        public int? ClustersCount { get; set; }
        #endregion

        #region 优化迭代次数 —— int? CriteriaMaxCount
        /// <summary>
        /// 优化迭代次数
        /// </summary>
        [DependencyProperty]
        public int? CriteriaMaxCount { get; set; }
        #endregion

        #region 优化误差 —— double? CriteriaEpsilon
        /// <summary>
        /// 优化误差
        /// </summary>
        [DependencyProperty]
        public double? CriteriaEpsilon { get; set; }
        #endregion

        #region 重复试验次数 —— int? AttemptsCount
        /// <summary>
        /// 重复试验次数
        /// </summary>
        [DependencyProperty]
        public int? AttemptsCount { get; set; }
        #endregion

        #region 初始中心类型 —— KMeansFlags KMeansFlag
        /// <summary>
        /// 初始中心类型
        /// </summary>
        [DependencyProperty]
        public KMeansFlags KMeansFlag { get; set; }
        #endregion

        #region 初始中心类型字典 —— IDictionary<string, string> KMeansFlags
        /// <summary>
        /// 初始中心类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> KMeansFlags { get; set; }
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
            this.ClustersCount = 5;
            this.CriteriaMaxCount = 10;
            this.CriteriaEpsilon = 0.1;
            this.AttemptsCount = 3;
            this.KMeansFlag = OpenCvSharp.KMeansFlags.PpCenters;
            this.KMeansFlags = typeof(KMeansFlags).GetEnumMembers();

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

            if (!this.ClustersCount.HasValue)
            {
                MessageBox.Show("簇数量不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.CriteriaMaxCount.HasValue)
            {
                MessageBox.Show("优化迭代次数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.CriteriaEpsilon.HasValue)
            {
                MessageBox.Show("优化误差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.AttemptsCount.HasValue)
            {
                MessageBox.Show("重复试验次数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat result = await Task.Run(() => this.Image.KMeansSegment(this.ClustersCount!.Value, this.CriteriaMaxCount!.Value, this.CriteriaEpsilon!.Value, this.AttemptsCount!.Value, this.KMeansFlag));
            this.BitmapSource = result.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #endregion
    }
}
