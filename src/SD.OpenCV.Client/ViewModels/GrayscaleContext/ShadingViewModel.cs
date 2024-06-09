using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GrayscaleContext
{
    /// <summary>
    /// 阴影变换视图模型
    /// </summary>
    public class ShadingViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ShadingViewModel()
        {
            //默认值
            this.KernelSize = 3;
            this.Gain = 60;
            this.Noise = 0;
            this.Offset = 140;
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

        #region 增益 —— byte? Gain
        /// <summary>
        /// 增益
        /// </summary>
        [DependencyProperty]
        public byte? Gain { get; set; }
        #endregion

        #region 噪声 —— byte? Noise
        /// <summary>
        /// 噪声
        /// </summary>
        [DependencyProperty]
        public byte? Noise { get; set; }
        #endregion

        #region 亮度补偿 —— byte? Offset
        /// <summary>
        /// 亮度补偿
        /// </summary>
        [DependencyProperty]
        public byte? Offset { get; set; }
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
            if (!this.Gain.HasValue)
            {
                MessageBox.Show("增益不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Noise.HasValue)
            {
                MessageBox.Show("噪声不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Offset.HasValue)
            {
                MessageBox.Show("亮度补偿不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
