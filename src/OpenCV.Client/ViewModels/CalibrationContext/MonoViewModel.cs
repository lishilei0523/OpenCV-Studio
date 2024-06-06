using Caliburn.Micro;
using SD.Infrastructure.WPF.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCV.Client.ViewModels.CalibrationContext
{
    /// <summary>
    /// 单目标定视图模型
    /// </summary>
    public class MonoViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MonoViewModel(IWindowManager windowManager, IEventAggregator eventAggregator)
        {
            this._windowManager = windowManager;
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性



        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— override async Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {

        }
        #endregion

        #region 加载 —— void Load()
        /// <summary>
        /// 加载
        /// </summary>
        public void Load()
        {

        }
        #endregion


        //Actions


        //Private


        #endregion
    }
}
