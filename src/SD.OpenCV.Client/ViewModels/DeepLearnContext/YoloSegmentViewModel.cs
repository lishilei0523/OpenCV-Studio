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
using SD.OpenCV.OnnxRuntime.Results;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace SD.OpenCV.Client.ViewModels.DeepLearnContext
{
    /// <summary>
    /// YOLO图像分割视图模型
    /// </summary>
    public class YoloSegmentViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public YoloSegmentViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region YOLO图像分割模型 —— YoloSegmenter YoloSegmenter
        /// <summary>
        /// YOLO图像分割模型
        /// </summary>
        public YoloSegmenter YoloSegmenter { get; set; }
        #endregion

        #region 目标图像 —— BitmapSource TargetImage
        /// <summary>
        /// 目标图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource TargetImage { get; set; }
        #endregion

        #region 分割阈值 —— float Threshold
        /// <summary>
        /// 分割阈值
        /// </summary>
        [DependencyProperty]
        public float Threshold { get; set; }
        #endregion

        #region 已选择分割结果 —— ImageSegmentation SelectedSegmentation
        /// <summary>
        /// 已选择分割结果
        /// </summary>
        [DependencyProperty]
        public ImageSegmentation SelectedSegmentation { get; set; }
        #endregion

        #region 分割结果列表 —— ObservableCollection<ImageSegmentation> Segmentations
        /// <summary>
        /// 分割结果列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ImageSegmentation> Segmentations { get; set; }
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
                Title = "请选择YOLO图像分割模型",
                Filter = "(*.onnx)|*.onnx",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                if (this.YoloSegmenter != null)
                {
                    this.YoloSegmenter.Dispose();
                    this.YoloSegmenter = null;
                }

                this.YoloSegmenter = await Task.Run(() => new YoloSegmenter(openFileDialog.FileName));
                await Task.Run(() => this.YoloSegmenter.StartSession());

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

        #region 执行分割 —— async void ExecuteSegment()
        /// <summary>
        /// 执行分割
        /// </summary>
        public async void ExecuteSegment()
        {
            #region # 验证

            if (this.YoloSegmenter == null)
            {
                MessageBox.Show("YOLO图像分割模型未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
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
            Segmentation[] segmentations = await Task.Run(() => this.YoloSegmenter.Infer(image, this.Threshold / 100));
            IEnumerable<ImageSegmentation> imageSegmentations =
                from segmentation in segmentations
                let box = image.CorrectRectangle(segmentation.Box)
                select new ImageSegmentation(segmentation.Label, box, segmentation.Contour, segmentation.Confidence);
            this.Segmentations = new ObservableCollection<ImageSegmentation>(imageSegmentations);

            //绘制
            foreach (ImageSegmentation segmentation in this.Segmentations)
            {
                //绘制矩形
                RectangleVisual2D rectangle = new RectangleVisual2D
                {
                    Location = new Point(segmentation.Box.X, segmentation.Box.Y),
                    Size = new Size(segmentation.Box.Width, segmentation.Box.Height),
                    Label = $"{segmentation.Label}: {segmentation.Confidence:F2}",
                    Tag = segmentation
                };
                rectangle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;

                //绘制轮廓
                IEnumerable<Point> points =
                    from point in segmentation.Contour
                    select new Point(point.X, point.Y);
                PointCollection pointCollection = new PointCollection(points);
                Polygon polygon = new Polygon
                {
                    Fill = new SolidColorBrush(Colors.Red),
                    Opacity = 0.4,
                    Points = pointCollection,
                    Tag = segmentation
                };
                polygon.MouseLeftButtonDown += this.OnShapeMouseLeftDown;

                segmentation.Tag = polygon;
                this.Shapes.Add(rectangle);
                this.Shapes.Add(polygon);
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
            this.Segmentations?.Clear();
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
            if (this.SelectedSegmentation != null)
            {
                Shape shape = (Shape)this.SelectedSegmentation.Tag;
                shape?.BlinkFill();
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
            ImageSegmentation segmentation = (ImageSegmentation)shape.Tag;
            this.SelectedSegmentation = null;
            this.SelectedSegmentation = segmentation;
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
                this.YoloSegmenter?.Dispose();
                this.YoloSegmenter = null;
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
