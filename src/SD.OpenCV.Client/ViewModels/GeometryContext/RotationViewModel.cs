using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 旋转视图模型
    /// </summary>
    public class RotationViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public RotationViewModel()
        {
            //默认值
            this.Angle = 45;
        }

        #endregion

        #region # 属性

        #region 旋转角度 —— float? Angle
        /// <summary>
        /// 旋转角度
        /// </summary>
        [DependencyProperty]
        public float? Angle { get; set; }
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

            if (!this.Angle.HasValue)
            {
                MessageBox.Show("旋转角度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
