using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Toolkits.OpenCV.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenCV.Client.ViewModels.CalibrationContext
{
    /// <summary>
    /// 单目标定参数视图模型
    /// </summary>
    public class MonoParamViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MonoParamViewModel()
        {
            //默认值
            this.SelectedPatternType = PatternType.Chessboard;
            this.PatternSideSize = 5;
            this.RowPointsCount = 9;
            this.ColumnPointsCount = 6;
            this.ImageWidth = 640;
            this.ImageHeight = 480;
            this.MaxCount = 30;
            this.Epsilon = 0.01;
        }

        #endregion

        #region # 属性

        #region 相机Id —— string CameraId
        /// <summary>
        /// 相机Id
        /// </summary>
        [DependencyProperty]
        public string CameraId { get; set; }
        #endregion

        #region 标定板类型 —— PatternType? SelectedPatternType
        /// <summary>
        /// 标定板类型
        /// </summary>
        [DependencyProperty]
        public PatternType? SelectedPatternType { get; set; }
        #endregion

        #region 网格边长 —— int? PatternSideSize
        /// <summary>
        /// 网格边长
        /// </summary>
        [DependencyProperty]
        public int? PatternSideSize { get; set; }
        #endregion

        #region 行角点数 —— int? RowPointsCount
        /// <summary>
        /// 行角点数
        /// </summary>
        [DependencyProperty]
        public int? RowPointsCount { get; set; }
        #endregion

        #region 列角点数 —— int? ColumnPointsCount
        /// <summary>
        /// 列角点数
        /// </summary>
        [DependencyProperty]
        public int? ColumnPointsCount { get; set; }
        #endregion

        #region 图像宽度 —— int? ImageWidth
        /// <summary>
        /// 图像宽度
        /// </summary>
        [DependencyProperty]
        public int? ImageWidth { get; set; }
        #endregion

        #region 图像高度 —— int? ImageHeight
        /// <summary>
        /// 图像高度
        /// </summary>
        [DependencyProperty]
        public int? ImageHeight { get; set; }
        #endregion

        #region 优化迭代次数 —— int? MaxCount
        /// <summary>
        /// 优化迭代次数
        /// </summary>
        [DependencyProperty]
        public int? MaxCount { get; set; }
        #endregion

        #region 优化误差 —— double? Epsilon
        /// <summary>
        /// 优化误差
        /// </summary>
        [DependencyProperty]
        public double? Epsilon { get; set; }
        #endregion

        #region 标定板类型字典 —— IDictionary<string, string> PatternTypes
        /// <summary>
        /// 标定板类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> PatternTypes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            this.PatternTypes = typeof(PatternType).GetEnumMembers();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 提交 —— async void Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async void Submit()
        {
            #region # 验证

            if (string.IsNullOrWhiteSpace(this.CameraId))
            {
                MessageBox.Show("相机Id不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.SelectedPatternType.HasValue)
            {
                MessageBox.Show("标定板类型不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.PatternSideSize.HasValue)
            {
                MessageBox.Show("网格边长不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.RowPointsCount.HasValue)
            {
                MessageBox.Show("行角点数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.ColumnPointsCount.HasValue)
            {
                MessageBox.Show("列角点数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.ImageWidth.HasValue)
            {
                MessageBox.Show("图像宽度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.ImageHeight.HasValue)
            {
                MessageBox.Show("图像高度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MaxCount.HasValue)
            {
                MessageBox.Show("优化迭代次数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Epsilon.HasValue)
            {
                MessageBox.Show("优化误差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
