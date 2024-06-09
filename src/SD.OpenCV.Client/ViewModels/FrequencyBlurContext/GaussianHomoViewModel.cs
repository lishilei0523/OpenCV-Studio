using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FrequencyBlurContext
{
    /// <summary>
    /// 高斯同态滤波视图模型
    /// </summary>
    public class GaussianHomoViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GaussianHomoViewModel()
        {
            //默认值
            this.GammaH = 2.2f;
            this.GammaL = 0.5f;
            this.Sigma = 0.01f;
            this.Slope = 1;
        }

        #endregion

        #region # 属性

        #region 高频增益 —— float? GammaH
        /// <summary>
        /// 高频增益
        /// </summary>
        [DependencyProperty]
        public float? GammaH { get; set; }
        #endregion

        #region 低频增益 —— float? GammaL
        /// <summary>
        /// 低频增益
        /// </summary>
        [DependencyProperty]
        public float? GammaL { get; set; }
        #endregion

        #region 滤波半径 —— float? Sigma
        /// <summary>
        /// 滤波半径
        /// </summary>
        [DependencyProperty]
        public float? Sigma { get; set; }
        #endregion

        #region 滤波斜率 —— float? Slope
        /// <summary>
        /// 滤波斜率
        /// </summary>
        [DependencyProperty]
        public float? Slope { get; set; }
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

            if (!this.GammaH.HasValue)
            {
                MessageBox.Show("高频增益不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.GammaL.HasValue)
            {
                MessageBox.Show("低频增益不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("滤波半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Slope.HasValue)
            {
                MessageBox.Show("滤波斜率不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
