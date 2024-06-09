using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FrequencyBlurContext
{
    /// <summary>
    /// 高斯带通/阻滤波视图模型
    /// </summary>
    public class GaussianBandViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GaussianBandViewModel()
        {
            //默认值
            this.Sigma = 18;
            this.BandWidth = 23;
        }

        #endregion

        #region # 属性

        #region 滤波半径 —— float? Sigma
        /// <summary>
        /// 滤波半径
        /// </summary>
        [DependencyProperty]
        public float? Sigma { get; set; }
        #endregion

        #region 滤波带宽 —— float? BandWidth
        /// <summary>
        /// 滤波带宽
        /// </summary>
        [DependencyProperty]
        public float? BandWidth { get; set; }
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
                MessageBox.Show("滤波半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.BandWidth.HasValue)
            {
                MessageBox.Show("滤波带宽不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
