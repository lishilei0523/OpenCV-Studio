using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.Models;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Values;
using SD.OpenCV.Primitives.Extensions;
using SourceChord.FluentWPF.Animations;
using System;
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
using Rect = OpenCvSharp.Rect;
using Size = System.Windows.Size;

namespace SD.OpenCV.Client.ViewModels.DeepLearnContext
{
    /// <summary>
    /// PaddleOCR视图模型
    /// </summary>
    public class PaddleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public PaddleViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region Canvas模式 —— CanvasMode CanvasMode
        /// <summary>
        /// Canvas模式
        /// </summary>
        [DependencyProperty]
        public CanvasMode CanvasMode { get; set; }
        #endregion

        #region Paddle文本检测模型 —— PaddleDetector Detector
        /// <summary>
        /// Paddle文本检测模型
        /// </summary>
        public PaddleDetector Detector { get; set; }
        #endregion

        #region Paddle文本识别模型 —— PaddleRecognizer Recognizer
        /// <summary>
        /// Paddle文本识别模型
        /// </summary>
        public PaddleRecognizer Recognizer { get; set; }
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

        #region 已选择检测结果 —— TextDetection SelectedDetection
        /// <summary>
        /// 已选择检测结果
        /// </summary>
        [DependencyProperty]
        public TextDetection SelectedDetection { get; set; }
        #endregion

        #region 检测结果列表 —— ObservableCollection<TextDetection> Detections
        /// <summary>
        /// 检测结果列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<TextDetection> Detections { get; set; }
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
            this.CanvasMode = CanvasMode.Scale;
            this.Threshold = 50f;
            this.Detections = new ObservableCollection<TextDetection>();
            this.Shapes = new ObservableCollection<Shape>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 打开检测模型 —— async void OpenDetectorModel()
        /// <summary>
        /// 打开检测模型
        /// </summary>
        public async void OpenDetectorModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "请选择Paddle文本检测模型",
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.Detector != null)
                {
                    this.Detector.Dispose();
                    this.Detector = null;
                }

                this.Detector = await Task.Run(() => new PaddleDetector(openFileDialog.FileName));
                await Task.Run(() => this.Detector.StartSession());

                this.Idle();
                MessageBox.Show("Paddle文本检测模型已成功加载！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion

        #region 打开识别模型 —— async void OpenRecognizerModel()
        /// <summary>
        /// 打开识别模型
        /// </summary>
        public async void OpenRecognizerModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "请选择Paddle文本识别模型",
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.Recognizer != null)
                {
                    this.Recognizer.Dispose();
                    this.Recognizer = null;
                }

                this.Recognizer = await Task.Run(() => new PaddleRecognizer(openFileDialog.FileName));
                await Task.Run(() => this.Recognizer.StartSession());

                this.Idle();
                MessageBox.Show("Paddle文本识别模型已成功加载！", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
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

            if (this.Detector == null)
            {
                MessageBox.Show("Paddle文本检测模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.Recognizer == null)
            {
                MessageBox.Show("Paddle文本识别模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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

            PaddleContour[] contours = await Task.Run(() => this.Detector.Infer(image, this.Threshold / 100));
            foreach (PaddleContour contour in contours)
            {
                //识别文本
                using Mat roi = image.ExtractMatrixInContour(contour.Points.Select(x => x.ToPoint()));
                string text = await Task.Run(() => this.Recognizer.Infer(roi));

                //整理检测结果
                Rect boundingBox = Cv2.BoundingRect(contour.Points);
                Rect correctBox = image.CorrectRectangle(boundingBox);
                TextDetection detection = new TextDetection(text, correctBox, contour.Confidence);

                //绘制文本
                TextVisual2D textVisual2D = new TextVisual2D();
                textVisual2D.Text = $"{detection.Text}: {detection.Confidence:F2}";
                textVisual2D.FontSize = 12;
                textVisual2D.Fill = new SolidColorBrush(Colors.Yellow);
                textVisual2D.X = detection.Box.X;
                textVisual2D.Y = detection.Box.Y - textVisual2D.FontSize - 2;

                //绘制矩形
                RectangleVisual2D rectangle = new RectangleVisual2D
                {
                    StrokeThickness = 1,
                    Location = new Point(detection.Box.X, detection.Box.Y),
                    Size = new Size(detection.Box.Width, detection.Box.Height),
                    Tag = detection
                };
                rectangle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;

                detection.Tag = rectangle;
                this.Detections.Add(detection);
                this.Shapes.Add(textVisual2D);
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
            Shape shape = (Shape)sender;
            TextDetection detection = (TextDetection)shape.Tag;
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
                this.Detector?.Dispose();
                this.Recognizer?.Dispose();
                this.Detector = null;
                this.Recognizer = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
