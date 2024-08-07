using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.OpenCV.Client.Views.EdgeContext;
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

namespace SD.OpenCV.Client.ViewModels.EdgeContext
{
    /// <summary>
    /// 轮廓检测视图模型
    /// </summary>
    public class ContourViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 多边形颜色列表
        /// </summary>
        private static readonly Color[] _PolyColors =
        {
            Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple
        };

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ContourViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 操作模式 —— CanvasMode CanvasMode
        /// <summary>
        /// 操作模式
        /// </summary>
        [DependencyProperty]
        public CanvasMode CanvasMode { get; set; }
        #endregion

        #region 图像源 —— BitmapSource BitmapSource
        /// <summary>
        /// 图像源
        /// </summary>
        [DependencyProperty]
        public BitmapSource BitmapSource { get; set; }
        #endregion

        #region 选中缩放 —— bool ScaleChecked
        /// <summary>
        /// 选中缩放
        /// </summary>
        [DependencyProperty]
        public bool ScaleChecked { get; set; }
        #endregion

        #region 选中拖拽 —— bool DragChecked
        /// <summary>
        /// 选中拖拽
        /// </summary>
        [DependencyProperty]
        public bool DragChecked { get; set; }
        #endregion

        #region 选中编辑 —— bool ResizeChecked
        /// <summary>
        /// 选中编辑
        /// </summary>
        [DependencyProperty]
        public bool ResizeChecked { get; set; }
        #endregion

        #region 面积阈值 —— int AreaThreshold
        /// <summary>
        /// 面积阈值
        /// </summary>
        [DependencyProperty]
        public int AreaThreshold { get; set; }
        #endregion

        #region 检测模式 —— RetrievalModes RetrievalMode
        /// <summary>
        /// 检测模式
        /// </summary>
        [DependencyProperty]
        public RetrievalModes RetrievalMode { get; set; }
        #endregion

        #region 检测模式字典 —— IDictionary<string, string> RetrievalModes
        /// <summary>
        /// 检测模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> RetrievalModes { get; set; }
        #endregion

        #region 近似模式 —— ContourApproximationModes ApproxMode
        /// <summary>
        /// 近似模式
        /// </summary>
        [DependencyProperty]
        public ContourApproximationModes ApproxMode { get; set; }
        #endregion

        #region 近似模式字典 —— IDictionary<string, string> ApproxModes
        /// <summary>
        /// 近似模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ApproxModes { get; set; }
        #endregion

        #region 已选形状数据 —— ShapeL SelectedShapeL
        /// <summary>
        /// 已选形状数据
        /// </summary>
        [DependencyProperty]
        public ShapeL SelectedShapeL { get; set; }
        #endregion

        #region 形状集 —— ObservableCollection<Shape> Shapes
        /// <summary>
        /// 形状集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> Shapes { get; set; }
        #endregion

        #region 形状数据集 —— ObservableCollection<ShapeL> ShapeLs
        /// <summary>
        /// 形状数据集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ShapeL> ShapeLs { get; set; }
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
            this.ScaleChecked = true;
            this.AreaThreshold = 100;
            this.RetrievalMode = OpenCvSharp.RetrievalModes.List;
            this.ApproxMode = ContourApproximationModes.ApproxNone;
            this.RetrievalModes = typeof(RetrievalModes).GetEnumMembers();
            this.ApproxModes = typeof(ContourApproximationModes).GetEnumMembers();
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(BitmapSource bitmapSource)
        {
            this.BitmapSource = bitmapSource;
        }
        #endregion


        //Actions

        #region 检测轮廓 —— async void DetectContours()
        /// <summary>
        /// 检测轮廓
        /// </summary>
        public async void DetectContours()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //清空轮廓
            ContourView view = (ContourView)this.GetView();
            CanvasEx canvasEx = view.CanvasEx;
            foreach (Polygon polygon in canvasEx.Children.OfType<Polygon>().ToArray())
            {
                canvasEx.Children.Remove(polygon);
            }
            this.ShapeLs.Clear();
            this.Shapes.Clear();

            //检测轮廓
            using Mat image = this.BitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC1 ? image : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Point[][] contours = { };
            await Task.Run(() => Cv2.FindContours(grayImage, out contours, out HierarchyIndex[] _, this.RetrievalMode, this.ApproxMode));

            //绘制轮廓
            contours = contours.OrderByDescending(contour => Cv2.ContourArea(contour)).ToArray();
            for (int index = 0; index < contours.Length; index++)
            {
                OpenCvSharp.Point[] contour = contours[index];

                //过滤面积
                double area = Cv2.ContourArea(contour);
                if (area < this.AreaThreshold)
                {
                    continue;
                }

                IList<PointL> polyPointLs = new List<PointL>();
                PointCollection polyPoints = new PointCollection();
                foreach (OpenCvSharp.Point point in contour)
                {
                    PointL polyPointL = new PointL(point.X, point.Y);
                    Point polyPoint = new Point(point.X, point.Y);
                    polyPointLs.Add(polyPointL);
                    polyPoints.Add(polyPoint);
                }
                PolygonL polygonL = new PolygonL(polyPointLs);
                Polygon polygon = new Polygon
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(_PolyColors[index % 5]),
                    StrokeThickness = 2,
                    Points = polyPoints,
                    RenderTransform = canvasEx.MatrixTransform,
                    Tag = polygonL
                };
                polygonL.Tag = polygon;
                this.ShapeLs.Add(polygonL);
                this.Shapes.Add(polygon);
                canvasEx.Children.Add(polygon);
                polygon.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
            }

            this.Idle();
        }
        #endregion

        #region 复制形状 —— void CopyShape()
        /// <summary>
        /// 复制形状
        /// </summary>
        public void CopyShape()
        {
            if (this.SelectedShapeL != null)
            {
                Clipboard.SetText(this.SelectedShapeL.Text);
                base.ToastSuccess("已复制剪贴板！");
            }
        }
        #endregion

        #region 删除形状 —— void RemoveShape()
        /// <summary>
        /// 删除形状
        /// </summary>
        public void RemoveShape()
        {
            if (this.SelectedShapeL != null)
            {
                Shape shape = (Shape)this.SelectedShapeL.Tag;
                CanvasEx canvas = (CanvasEx)shape.Parent;

                this.ShapeLs.Remove(this.SelectedShapeL);
                this.Shapes.Remove(shape);
                canvas.Children.Remove(shape);
            }
        }
        #endregion

        #region 提交 —— async void Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async void Submit()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.BitmapSource.ToMat();
            using Mat colorImage = image.Type() == MatType.CV_8UC1 ? image.CvtColor(ColorConversionCodes.GRAY2BGR) : image;
            foreach (Shape shape in this.Shapes)
            {
                int thickness = (int)Math.Ceiling(shape.StrokeThickness);
                SolidColorBrush borderBrush = (SolidColorBrush)shape.Stroke;
                Scalar borderColor = new Scalar(borderBrush.Color.B, borderBrush.Color.G, borderBrush.Color.R);
                if (shape is Polygon polygon)
                {
                    PolygonL polygonL = (PolygonL)polygon.Tag;
                    OpenCvSharp.Point[] contour = new OpenCvSharp.Point[polygonL.Points.Count];
                    for (int index = 0; index < polygonL.Points.Count; index++)
                    {
                        PointL pointL = polygonL.Points.ElementAt(index);
                        contour[index] = new OpenCvSharp.Point(pointL.X, pointL.Y);
                    }
                    await Task.Run(() => colorImage.DrawContours(new[] { contour }, -1, borderColor, thickness));
                }
            }
            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();

            await base.TryCloseAsync(true);
        }
        #endregion


        //Events

        #region 缩放点击事件 —— void OnScaleClick()
        /// <summary>
        /// 缩放点击事件
        /// </summary>
        public void OnScaleClick()
        {
            if (this.ScaleChecked)
            {
                this.CanvasMode = CanvasMode.Scale;

                this.DragChecked = false;
                this.ResizeChecked = false;
            }
        }
        #endregion

        #region 拖拽点击事件 —— void OnDragClick()
        /// <summary>
        /// 拖拽点击事件
        /// </summary>
        public void OnDragClick()
        {
            if (this.DragChecked)
            {
                this.CanvasMode = CanvasMode.Drag;

                this.ScaleChecked = false;
                this.ResizeChecked = false;
            }
        }
        #endregion

        #region 编辑点击事件 —— void OnResizeClick()
        /// <summary>
        /// 编辑点击事件
        /// </summary>
        public void OnResizeClick()
        {
            if (this.ResizeChecked)
            {
                this.CanvasMode = CanvasMode.Resize;

                this.ScaleChecked = false;
                this.DragChecked = false;
            }
        }
        #endregion

        #region 拖拽元素事件 —— void OnDragElement(CanvasEx canvas)
        /// <summary>
        /// 拖拽元素事件
        /// </summary>
        public void OnDragElement(CanvasEx canvas)
        {
            double leftMargin = canvas.GetRectifiedLeft(canvas.SelectedVisual);
            double topMargin = canvas.GetRectifiedTop(canvas.SelectedVisual);
            if (canvas.SelectedVisual is Polygon polygon)
            {
                this.RebuildPolygon(polygon, leftMargin, topMargin);
            }
        }
        #endregion

        #region 改变元素尺寸事件 —— void OnResizeElement(CanvasEx canvas)
        /// <summary>
        /// 改变元素尺寸事件
        /// </summary>
        public void OnResizeElement(CanvasEx canvas)
        {
            double leftMargin = canvas.GetRectifiedLeft(canvas.SelectedVisual);
            double topMargin = canvas.GetRectifiedTop(canvas.SelectedVisual);

            if (canvas.SelectedVisual is Polygon polygon)
            {
                double minDistance = int.MaxValue;
                Point? nearestPoint = null;
                foreach (Point point in polygon.Points)
                {
                    Vector vector = canvas.RectifiedMousePosition!.Value - new Point(point.X + leftMargin, point.Y + topMargin);
                    if (vector.Length < minDistance)
                    {
                        minDistance = vector.Length;
                        nearestPoint = point;
                    }
                }
                if (nearestPoint.HasValue)
                {
                    int index = polygon.Points.IndexOf(nearestPoint.Value);
                    polygon.Points.Remove(nearestPoint.Value);
                    Point newPoint = new Point(canvas.RectifiedMousePosition!.Value.X - leftMargin, canvas.RectifiedMousePosition!.Value.Y - topMargin);
                    polygon.Points.Insert(index, newPoint);
                }

                this.RebuildPolygon(polygon, leftMargin, topMargin);
            }
        }
        #endregion

        #region 选中形状事件 —— void OnSelectShape()
        /// <summary>
        /// 选中形状事件
        /// </summary>
        public void OnSelectShape()
        {
            if (this.SelectedShapeL != null)
            {
                Shape shape = (Shape)this.SelectedShapeL.Tag;
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


        //Private

        #region 重建多边形 —— void RebuildPolygon(Polygon polygon, double leftMargin, double topMargin)
        /// <summary>
        /// 重建多边形
        /// </summary>
        /// <param name="polygon">多边形</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildPolygon(Polygon polygon, double leftMargin, double topMargin)
        {
            PolygonL polygonL = (PolygonL)polygon.Tag;
            int index = this.ShapeLs.IndexOf(polygonL);
            if (index != -1)
            {
                this.ShapeLs.Remove(polygonL);

                IList<PointL> pointIs = new List<PointL>();
                foreach (Point point in polygon.Points)
                {
                    int x = (int)Math.Ceiling(point.X + leftMargin);
                    int y = (int)Math.Ceiling(point.Y + topMargin);
                    PointL pointI = new PointL(x, y);
                    pointIs.Add(pointI);
                }
                PolygonL newPolygonL = new PolygonL(pointIs);

                polygon.Tag = newPolygonL;
                newPolygonL.Tag = polygon;
                this.ShapeLs.Insert(index, newPolygonL);
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(object sender...
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        private void OnShapeMouseLeftDown(object sender, MouseButtonEventArgs eventArgs)
        {
            if (this.CanvasMode != CanvasMode.Draw)
            {
                Shape shape = (Shape)sender;
                ShapeL shapeL = (ShapeL)shape.Tag;
                this.SelectedShapeL = null;
                this.SelectedShapeL = shapeL;
            }
        }
        #endregion

        #endregion
    }
}
