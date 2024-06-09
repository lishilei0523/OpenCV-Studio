using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.SpaceBlurContext
{
    /// <summary>
    /// 双边滤波视图模型
    /// </summary>
    public class BilateralViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public BilateralViewModel()
        {
            //默认值
            this.Diameter = 9;
            this.SigmaColor = 75;
            this.SigmaSpace = 75;
        }

        #endregion

        #region # 属性

        #region 滤波直径 —— int? Diameter
        /// <summary>
        /// 滤波直径
        /// </summary>
        [DependencyProperty]
        public int? Diameter { get; set; }
        #endregion

        #region 颜色标准差 —— double? SigmaColor
        /// <summary>
        /// 颜色标准差
        /// </summary>
        [DependencyProperty]
        public double? SigmaColor { get; set; }
        #endregion

        #region 空间标准差 —— double? SigmaSpace
        /// <summary>
        /// 空间标准差
        /// </summary>
        [DependencyProperty]
        public double? SigmaSpace { get; set; }
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

            if (!this.Diameter.HasValue)
            {
                MessageBox.Show("滤波直径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.SigmaColor.HasValue)
            {
                MessageBox.Show("颜色标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.SigmaSpace.HasValue)
            {
                MessageBox.Show("空间标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
