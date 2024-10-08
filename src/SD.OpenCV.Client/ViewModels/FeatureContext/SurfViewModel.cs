﻿using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FeatureContext
{
    /// <summary>
    /// SURF特征视图模型
    /// </summary>
    public class SurfViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public SurfViewModel()
        {
            //默认值
            this.HessianThreshold = 100;
        }

        #endregion

        #region # 属性

        #region Hessian阈值 —— double? HessianThreshold
        /// <summary>
        /// Hessian阈值
        /// </summary>
        [DependencyProperty]
        public double? HessianThreshold { get; set; }
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

            if (!this.HessianThreshold.HasValue)
            {
                MessageBox.Show("Hessian阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
