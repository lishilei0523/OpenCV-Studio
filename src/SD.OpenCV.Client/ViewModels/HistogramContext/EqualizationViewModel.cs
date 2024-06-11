using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.HistogramContext
{
    /// <summary>
    /// 直方图均衡化视图模型
    /// </summary>
    public class EqualizationViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public EqualizationViewModel()
        {
            //默认值
            this.ClipLimit = 2.0d;
        }

        #endregion

        #region # 属性

        #region 裁剪限制 —— double? ClipLimit
        /// <summary>
        /// 裁剪限制
        /// </summary>
        [DependencyProperty]
        public double? ClipLimit { get; set; }
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

            if (!this.ClipLimit.HasValue)
            {
                MessageBox.Show("裁剪限制不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
