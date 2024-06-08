using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.EdgeContext
{
    /// <summary>
    /// Canny边缘检测视图模型
    /// </summary>
    public class CannyViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CannyViewModel()
        {
            //默认值
            this.Threshold1 = 50;
            this.Threshold2 = 150;
        }

        #endregion

        #region # 属性

        #region 滞后性阈值1 —— double? Threshold1
        /// <summary>
        /// 滞后性阈值1
        /// </summary>
        [DependencyProperty]
        public double? Threshold1 { get; set; }
        #endregion

        #region 滞后性阈值2 —— double? Threshold2
        /// <summary>
        /// 滞后性阈值2
        /// </summary>
        [DependencyProperty]
        public double? Threshold2 { get; set; }
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

            if (!this.Threshold1.HasValue)
            {
                MessageBox.Show("滞后性阈值1不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Threshold2.HasValue)
            {
                MessageBox.Show("滞后性阈值2不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
