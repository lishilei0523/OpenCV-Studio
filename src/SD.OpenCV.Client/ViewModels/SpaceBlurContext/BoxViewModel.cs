using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.SpaceBlurContext
{
    /// <summary>
    /// 方块滤波视图模型
    /// </summary>
    public class BoxViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public BoxViewModel()
        {
            //默认值
            this.Depth = -1;
            this.KernelSize = 3;
            this.NeedToNormalize = true;
        }

        #endregion

        #region # 属性

        #region 图像深度 —— int? Depth
        /// <summary>
        /// 图像深度
        /// </summary>
        [DependencyProperty]
        public int? Depth { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region 是否归一化 —— bool NeedToNormalize
        /// <summary>
        /// 是否归一化
        /// </summary>
        [DependencyProperty]
        public bool NeedToNormalize { get; set; }
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

            if (!this.Depth.HasValue)
            {
                MessageBox.Show("图像深度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
