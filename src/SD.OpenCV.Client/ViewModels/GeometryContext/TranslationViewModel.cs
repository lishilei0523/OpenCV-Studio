using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 平移视图模型
    /// </summary>
    public class TranslationViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public TranslationViewModel()
        {
            //默认值
            this.OffsetX = 0;
            this.OffsetY = 0;
        }

        #endregion

        #region # 属性

        #region X轴平移量 —— float? OffsetX
        /// <summary>
        /// X轴平移量
        /// </summary>
        [DependencyProperty]
        public float? OffsetX { get; set; }
        #endregion

        #region Y轴平移量 —— float? OffsetY
        /// <summary>
        /// Y轴平移量
        /// </summary>
        [DependencyProperty]
        public float? OffsetY { get; set; }
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

            if (!this.OffsetX.HasValue)
            {
                MessageBox.Show("X轴平移量不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.OffsetY.HasValue)
            {
                MessageBox.Show("Y轴平移量不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
