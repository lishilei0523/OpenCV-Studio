using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 线性变换视图模型
    /// </summary>
    public class LinearViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LinearViewModel()
        {
            //默认值
            this.Alpha = 1;
            this.Beta = 30;
        }

        #endregion

        #region # 属性

        #region 对比度 —— float? Alpha
        /// <summary>
        /// 对比度
        /// </summary>
        [DependencyProperty]
        public float? Alpha { get; set; }
        #endregion

        #region 亮度 —— float? Beta
        /// <summary>
        /// 亮度
        /// </summary>
        [DependencyProperty]
        public float? Beta { get; set; }
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

            if (!this.Alpha.HasValue)
            {
                MessageBox.Show("对比度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Beta.HasValue)
            {
                MessageBox.Show("亮度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
