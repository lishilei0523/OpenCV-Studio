using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 霍夫线查找视图模型
    /// </summary>
    public class LineViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LineViewModel()
        {
            //默认值
            this.Rho = 1;
            this.Theta = Math.PI / 180;
            this.Threshold = 100;
        }

        #endregion

        #region # 属性

        #region 距离精度 —— double? Rho
        /// <summary>
        /// 距离精度
        /// </summary>
        [DependencyProperty]
        public double? Rho { get; set; }
        #endregion

        #region 角度精度 —— double? Theta
        /// <summary>
        /// 角度精度
        /// </summary>
        [DependencyProperty]
        public double? Theta { get; set; }
        #endregion

        #region 阈值 —— int? Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public int? Threshold { get; set; }
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

            if (!this.Rho.HasValue)
            {
                MessageBox.Show("距离精度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Theta.HasValue)
            {
                MessageBox.Show("角度精度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Threshold.HasValue)
            {
                MessageBox.Show("阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
