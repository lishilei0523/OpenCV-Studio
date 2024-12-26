using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.Models;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Values;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SD.OpenCV.Client.ViewModels.DeepLearnContext
{
    /// <summary>
    /// YOLO定向目标检测视图模型
    /// </summary>
    public class YoloObbDetectViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public YoloObbDetectViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region YOLO定向目标检测模型 —— YoloObbDetector YoloObbDetector
        /// <summary>
        /// YOLO定向目标检测模型
        /// </summary>
        public YoloObbDetector YoloObbDetector { get; set; }
        #endregion

        #region 目标图像 —— BitmapSource TargetImage
        /// <summary>
        /// 目标图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource TargetImage { get; set; }
        #endregion

        #region 检测阈值 —— float Threshold
        /// <summary>
        /// 检测阈值
        /// </summary>
        [DependencyProperty]
        public float Threshold { get; set; }
        #endregion

        #region 已选择检测结果 —— ObjectObbDetection SelectedDetection
        /// <summary>
        /// 已选择检测结果
        /// </summary>
        [DependencyProperty]
        public ObjectObbDetection SelectedDetection { get; set; }
        #endregion

        #region 检测结果列表 —— ObservableCollection<ObjectObbDetection> Detections
        /// <summary>
        /// 检测结果列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ObjectObbDetection> Detections { get; set; }
        #endregion

        #region 形状集 —— ObservableCollection<Shape> Shapes
        /// <summary>
        /// 形状集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> Shapes { get; set; }
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
            this.Threshold = 50f;
            this.Shapes = new ObservableCollection<Shape>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 打开模型 —— async void OpenModel()
        /// <summary>
        /// 打开模型
        /// </summary>
        public async void OpenModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "请选择YOLO定向目标检测模型",
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.YoloObbDetector != null)
                {
                    this.YoloObbDetector.Dispose();
                    this.YoloObbDetector = null;
                }

                this.YoloObbDetector = await Task.Run(() => new YoloObbDetector(openFileDialog.FileName));
                await Task.Run(() => this.YoloObbDetector.StartSession());

                this.Idle();
                MessageBox.Show("模型已成功加载！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region 打开图像 —— async void OpenImage()
        /// <summary>
        /// 打开图像
        /// </summary>
        public async void OpenImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "请选择目标图像",
                Filter = "图片文件(*.jpg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                this.Reset();
                using Mat image = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                this.TargetImage = image.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 执行检测 —— async void ExecuteDetect()
        /// <summary>
        /// 执行检测
        /// </summary>
        public async void ExecuteDetect()
        {
            #region # 验证

            if (this.YoloObbDetector == null)
            {
                MessageBox.Show("YOLO定向目标检测模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.TargetImage == null)
            {
                MessageBox.Show("目标图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            this.Reset();
            using Mat image = this.TargetImage.ToMat();
            ObbDetection[] detections = await Task.Run(() => this.YoloObbDetector.Infer(image, this.Threshold / 100));
            IEnumerable<ObjectObbDetection> objectObbDetections =
                from detection in detections
                select new ObjectObbDetection(detection.Label, detection.RotatedBox, detection.Confidence);
            this.Detections = new ObservableCollection<ObjectObbDetection>(objectObbDetections);

            //绘制
            foreach (ObjectObbDetection detection in this.Detections)
            {
                //绘制旋转矩形
                RotatedRectangleVisual2D rotatedRectangle = new RotatedRectangleVisual2D
                {
                    Center = new Point(detection.RotatedBox.Center.X, detection.RotatedBox.Center.Y),
                    Size = new Size(detection.RotatedBox.Size.Width, detection.RotatedBox.Size.Height),
                    Angle = detection.RotatedBox.Angle,
                    Label = $"{detection.Label}: {detection.Confidence:F2}",
                    ShowCenter = false
                };
                rotatedRectangle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;

                detection.Tag = rotatedRectangle;
                this.Shapes.Add(rotatedRectangle);
            }

            this.Idle();
        }
        #endregion

        #region 重置 —— void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this.Detections?.Clear();
            this.Shapes.Clear();
        }
        #endregion


        //Events

        #region 形状选中事件 —— void OnShapeSelect()
        /// <summary>
        /// 形状选中事件
        /// </summary>
        public void OnShapeSelect()
        {
            if (this.SelectedDetection != null)
            {
                Shape shape = (Shape)this.SelectedDetection.Tag;
                shape?.BlinkStroke();
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(object sender...
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        private void OnShapeMouseLeftDown(object sender, MouseEventArgs eventArgs)
        {
            Shape shape = (Shape)sender;
            ObjectObbDetection detection = (ObjectObbDetection)shape.Tag;
            this.SelectedDetection = null;
            this.SelectedDetection = detection;
        }
        #endregion

        #region 页面失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 页面失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this.YoloObbDetector?.Dispose();
                this.YoloObbDetector = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
