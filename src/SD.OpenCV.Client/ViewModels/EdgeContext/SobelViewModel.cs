using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.EdgeContext
{
    /// <summary>
    /// Sobel边缘检测视图模型
    /// </summary>
    public class SobelViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SobelViewModel()
        {
            //默认值
            this.KernelSize = 3;
            this.Alpha = 0.5f;
            this.Beta = 0.5f;
            this.Gamma = 0;
        }

        #endregion

        #region # 属性

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region X轴卷积权重 —— double? Alpha
        /// <summary>
        /// X轴卷积权重
        /// </summary>
        [DependencyProperty]
        public double? Alpha { get; set; }
        #endregion

        #region Y轴卷积权重 —— double? Beta
        /// <summary>
        /// Y轴卷积权重
        /// </summary>
        [DependencyProperty]
        public double? Beta { get; set; }
        #endregion

        #region 伽马值 —— double? Gamma
        /// <summary>
        /// 伽马值
        /// </summary>
        [DependencyProperty]
        public double? Gamma { get; set; }
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

            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Alpha.HasValue)
            {
                MessageBox.Show("X轴卷积权重不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Beta.HasValue)
            {
                MessageBox.Show("Y轴卷积权重不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
