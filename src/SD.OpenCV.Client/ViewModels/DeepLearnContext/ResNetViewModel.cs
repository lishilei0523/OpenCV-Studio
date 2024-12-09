using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Values;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.DeepLearnContext
{
    /// <summary>
    /// ResNet视图模型
    /// </summary>
    public class ResNetViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ResNetViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 目标图像 —— BitmapSource TargetImage
        /// <summary>
        /// 目标图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource TargetImage { get; set; }
        #endregion

        #region ResNet模型 —— ResNet ResNet
        /// <summary>
        /// ResNet模型
        /// </summary>
        public ResNet ResNet { get; set; }
        #endregion

        #region 匹配阈值 —— Rectangle Threshold
        /// <summary>
        /// 匹配阈值
        /// </summary>
        [DependencyProperty]
        public float Threshold { get; set; }
        #endregion

        #region 预测结果列表 —— ObservableCollection<Prediction> Predictions
        /// <summary>
        /// 预测结果列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Prediction> Predictions { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.Threshold = 0;

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 打开目标图像 —— async void OpenTargetImage()
        /// <summary>
        /// 打开目标图像
        /// </summary>
        public async void OpenTargetImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                this.TargetImage = image.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 打开ResNet模型 —— async void OpenResNetModel()
        /// <summary>
        /// 打开ResNet模型
        /// </summary>
        public async void OpenResNetModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.ResNet != null)
                {
                    this.ResNet.Dispose();
                    this.ResNet = null;
                }

                this.ResNet = await Task.Run(() => new ResNet(openFileDialog.FileName));
                await Task.Run(() => this.ResNet.StartSession());

                this.Idle();
            }
        }
        #endregion

        #region 执行分类 —— async void ExecuteClassify()
        /// <summary>
        /// 执行分类
        /// </summary>
        public async void ExecuteClassify()
        {
            #region # 验证

            if (this.TargetImage == null)
            {
                MessageBox.Show("目标图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.ResNet == null)
            {
                MessageBox.Show("ResNet模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.TargetImage.ToMat();
            Prediction[] predictions = await Task.Run(() => this.ResNet.Infer(image, this.Threshold / 100));
            this.Predictions = new ObservableCollection<Prediction>(predictions);

            this.Idle();
        }
        #endregion

        #region 重置 —— void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this.Predictions.Clear();
        }
        #endregion


        //Events

        #region 页面失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 页面失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this.ResNet?.Dispose();
                this.ResNet = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
