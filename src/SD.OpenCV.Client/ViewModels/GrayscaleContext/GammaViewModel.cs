using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 伽马变换视图模型
    /// </summary>
    public class GammaViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GammaViewModel()
        {
            //默认值
            this.Gamma = 1 / 2.0f;
        }

        #endregion

        #region # 属性

        #region 伽马值 —— float? Gamma
        /// <summary>
        /// 伽马值
        /// </summary>
        [DependencyProperty]
        public float? Gamma { get; set; }
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

            if (!this.Gamma.HasValue)
            {
                MessageBox.Show("伽马值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
