using OpenCvSharp;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 霍夫圆查找视图模型
    /// </summary>
    public class CircleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CircleViewModel()
        {
            //默认值
            this.HoughMode = OpenCvSharp.HoughModes.Gradient;
            this.Dp = 1;
            this.MinDistance = 200;
            this.MinRadius = 0;
            this.MaxRadius = 500;
        }

        #endregion

        #region # 属性

        #region 查找模式 —— HoughModes? HoughMode
        /// <summary>
        /// 查找模式
        /// </summary>
        [DependencyProperty]
        public HoughModes? HoughMode { get; set; }
        #endregion

        #region 分辨率 —— double? Dp
        /// <summary>
        /// 分辨率
        /// </summary>
        [DependencyProperty]
        public double? Dp { get; set; }
        #endregion

        #region 圆心最小距离 —— double? MinDistance
        /// <summary>
        /// 圆心最小距离
        /// </summary>
        [DependencyProperty]
        public double? MinDistance { get; set; }
        #endregion

        #region 最小半径 —— int? MinRadius
        /// <summary>
        /// 最小半径
        /// </summary>
        [DependencyProperty]
        public int? MinRadius { get; set; }
        #endregion

        #region 最大半径 —— int? MaxRadius
        /// <summary>
        /// 最大半径
        /// </summary>
        [DependencyProperty]
        public int? MaxRadius { get; set; }
        #endregion

        #region 查找模式字典 —— IDictionary<string, string> HoughModes
        /// <summary>
        /// 查找模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> HoughModes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            this.HoughModes = typeof(HoughModes).GetEnumMembers();
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

            if (!this.HoughMode.HasValue)
            {
                MessageBox.Show("查找模式不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Dp.HasValue)
            {
                MessageBox.Show("分辨率不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinRadius.HasValue)
            {
                MessageBox.Show("最小半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MaxRadius.HasValue)
            {
                MessageBox.Show("最大半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
