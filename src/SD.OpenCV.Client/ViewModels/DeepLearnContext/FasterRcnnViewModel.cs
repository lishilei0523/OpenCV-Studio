using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.Models;
using SD.OpenCV.Client.Views.DeepLearnContext;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Values;
using SD.OpenCV.Primitives.Extensions;
using SourceChord.FluentWPF.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SD.OpenCV.Client.ViewModels.DeepLearnContext
{
    /// <summary>
    /// FasterRCNN视图模型
    /// </summary>
    public class FasterRcnnViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 画布
        /// </summary>
        private CanvasEx _canvas;

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public FasterRcnnViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region FasterRCNN模型 —— FasterRcnn FasterRcnn
        /// <summary>
        /// FasterRCNN模型
        /// </summary>
        public FasterRcnn FasterRcnn { get; set; }
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

        #region 已选择检测结果 —— ObjectDetection SelectedDetection
        /// <summary>
        /// 已选择检测结果
        /// </summary>
        [DependencyProperty]
        public ObjectDetection SelectedDetection { get; set; }
        #endregion

        #region 检测结果列表 —— ObservableCollection<ObjectDetection> Detections
        /// <summary>
        /// 检测结果列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ObjectDetection> Detections { get; set; }
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

            //获取CanvasEx对象
            FasterRcnnView view = (FasterRcnnView)this.GetView();
            this._canvas = view.CanvasEx;

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
                Title = "请选择Faster R-CNN模型",
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.FasterRcnn != null)
                {
                    this.FasterRcnn.Dispose();
                    this.FasterRcnn = null;
                }

                this.FasterRcnn = await Task.Run(() => new FasterRcnn(openFileDialog.FileName));
                await Task.Run(() => this.FasterRcnn.StartSession());

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
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
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

            if (this.FasterRcnn == null)
            {
                MessageBox.Show("FasterRCNN模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Detection[] detections = await Task.Run(() => this.FasterRcnn.Infer(image, this.Threshold / 100));
            IEnumerable<ObjectDetection> objectDetections =
                from detection in detections
                let box = image.CorrectRectangle(detection.Box)
                select new ObjectDetection(detection.Label, box, detection.Confidence);
            this.Detections = new ObservableCollection<ObjectDetection>(objectDetections);

            //绘制
            foreach (ObjectDetection detection in this.Detections)
            {
                //绘制文本
                TextVisual2D text = new TextVisual2D();
                text.Text = $"{detection.Label}: {detection.Confidence:F2}";
                text.FontSize = 12;
                text.Fill = new SolidColorBrush(Colors.Yellow);
                text.X = detection.Box.X;
                text.Y = detection.Box.Y - text.FontSize - 2;
                text.RenderTransform = this._canvas.MatrixTransform;

                //绘制矩形
                RectangleVisual2D rectangle = new RectangleVisual2D
                {
                    StrokeThickness = 1,
                    Location = new Point(detection.Box.X, detection.Box.Y),
                    Size = new Size(detection.Box.Width, detection.Box.Height),
                    RenderTransform = this._canvas.MatrixTransform,
                    Tag = detection
                };
                rectangle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;

                detection.Tag = rectangle;
                this._canvas.Children.Add(text);
                this._canvas.Children.Add(rectangle);
                this.Shapes.Add(text);
                this.Shapes.Add(rectangle);
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
            foreach (Shape shape in this.Shapes)
            {
                this._canvas.Children.Remove(shape);
            }
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
                if (shape.Stroke is SolidColorBrush brush)
                {
                    BrushAnimation brushAnimation = new BrushAnimation
                    {
                        From = new SolidColorBrush(brush.Color.Invert()),
                        To = shape.Stroke,
                        Duration = new Duration(TimeSpan.FromSeconds(2))
                    };
                    Storyboard storyboard = new Storyboard();
                    Storyboard.SetTarget(brushAnimation, shape);
                    Storyboard.SetTargetProperty(brushAnimation, new PropertyPath(Shape.StrokeProperty));
                    storyboard.Children.Add(brushAnimation);
                    storyboard.Begin();
                }
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(object sender...
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        private void OnShapeMouseLeftDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (this._canvas.Mode != CanvasMode.Draw)
            {
                Shape shape = (Shape)sender;
                ObjectDetection detection = (ObjectDetection)shape.Tag;
                this.SelectedDetection = null;
                this.SelectedDetection = detection;
            }
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
                this.FasterRcnn?.Dispose();
                this.FasterRcnn = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
