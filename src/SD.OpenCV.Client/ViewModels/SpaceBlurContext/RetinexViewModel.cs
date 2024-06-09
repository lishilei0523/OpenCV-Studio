using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.SpaceBlurContext
{
    /// <summary>
    /// Retinex增强视图模型
    /// </summary>
    public class RetinexViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public RetinexViewModel()
        {
            //默认值
            this.Sigma = 300;
        }

        #endregion

        #region # 属性

        #region 标准差 —— float? Sigma
        /// <summary>
        /// 标准差
        /// </summary>
        [DependencyProperty]
        public float? Sigma { get; set; }
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

            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("标准差不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
