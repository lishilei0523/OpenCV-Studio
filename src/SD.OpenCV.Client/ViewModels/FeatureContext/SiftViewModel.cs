using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FeatureContext
{
    /// <summary>
    /// SIFT特征视图模型
    /// </summary>
    public class SiftViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SiftViewModel()
        {
            //默认值
            this.NFeatures = 0;
            this.NOctaveLayers = 3;
            this.ContrastThreshold = 0.04;
            this.EdgeThreshold = 10;
            this.Sigma = 1.6;
        }

        #endregion

        #region # 属性

        #region 保留特征数 —— int? NFeatures
        /// <summary>
        /// 保留特征数
        /// </summary>
        [DependencyProperty]
        public int? NFeatures { get; set; }
        #endregion

        #region 每层DoG个数 —— int? NOctaveLayers
        /// <summary>
        /// 每层DoG个数
        /// </summary>
        /// <remarks>DoG: Difference of Gaussian</remarks>
        [DependencyProperty]
        public int? NOctaveLayers { get; set; }
        #endregion

        #region 对比度阈值 —— double? ContrastThreshold
        /// <summary>
        /// 对比度阈值
        /// </summary>
        [DependencyProperty]
        public double? ContrastThreshold { get; set; }
        #endregion

        #region 边缘阈值 —— double? EdgeThreshold
        /// <summary>
        /// 边缘阈值
        /// </summary>
        [DependencyProperty]
        public double? EdgeThreshold { get; set; }
        #endregion

        #region 0层高斯因子 —— double? Sigma
        /// <summary>
        /// 0层高斯因子
        /// </summary>
        [DependencyProperty]
        public double? Sigma { get; set; }
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

            if (!this.NFeatures.HasValue)
            {
                MessageBox.Show("保留特征数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.NOctaveLayers.HasValue)
            {
                MessageBox.Show("每层DoG个数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.ContrastThreshold.HasValue)
            {
                MessageBox.Show("对比度阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.EdgeThreshold.HasValue)
            {
                MessageBox.Show("边缘阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Sigma.HasValue)
            {
                MessageBox.Show("0层高斯因子不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
