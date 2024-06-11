using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.FeatureContext
{
    /// <summary>
    /// Harris视图模型
    /// </summary>
    public class HarrisViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public HarrisViewModel()
        {
            //默认值
            this.BlockSize = 2;
            this.KernelSize = 3;
            this.K = 0.04;
        }

        #endregion

        #region # 属性

        #region 块尺寸 —— int? BlockSize
        /// <summary>
        /// 块尺寸
        /// </summary>
        [DependencyProperty]
        public int? BlockSize { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int? KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int? KernelSize { get; set; }
        #endregion

        #region 自由参数 —— double? K
        /// <summary>
        /// 自由参数
        /// </summary>
        /// <remarks>取值范围: [0.04, 0.06]</remarks>
        [DependencyProperty]
        public double? K { get; set; }
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

            if (!this.BlockSize.HasValue)
            {
                MessageBox.Show("块尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.KernelSize.HasValue)
            {
                MessageBox.Show("核矩阵尺寸不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.K.HasValue)
            {
                MessageBox.Show("自由参数不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
