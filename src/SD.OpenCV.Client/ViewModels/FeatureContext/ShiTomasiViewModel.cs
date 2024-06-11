using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FeatureContext
{
    /// <summary>
    /// Shi-Tomasi视图模型
    /// </summary>
    public class ShiTomasiViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ShiTomasiViewModel()
        {
            //默认值
            this.MaxCorners = 1000;
            this.QualityLevel = 0.01;
            this.MinDistance = 10;
            this.BlockSize = 3;
        }

        #endregion

        #region # 属性

        #region 最大关键点数 —— int? MaxCorners
        /// <summary>
        /// 最大关键点数
        /// </summary>
        [DependencyProperty]
        public int? MaxCorners { get; set; }
        #endregion

        #region 质量级别 —— double? QualityLevel
        /// <summary>
        /// 质量级别
        /// </summary>
        /// <remarks>取值范围: [0.01, 0.1]</remarks>
        [DependencyProperty]
        public double? QualityLevel { get; set; }
        #endregion

        #region 最小距离 —— double? MinDistance
        /// <summary>
        /// 最小距离
        /// </summary>
        [DependencyProperty]
        public double? MinDistance { get; set; }
        #endregion

        #region 块尺寸 —— int? BlockSize
        /// <summary>
        /// 块尺寸
        /// </summary>
        [DependencyProperty]
        public int? BlockSize { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 提交 —— async void Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async void Submit()
        {
            #region # 验证

            if (!this.MaxCorners.HasValue)
            {
                MessageBox.Show("最大关键点数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.QualityLevel.HasValue)
            {
                MessageBox.Show("质量级别不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinDistance.HasValue)
            {
                MessageBox.Show("最小距离不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.BlockSize.HasValue)
            {
                MessageBox.Show("块尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
