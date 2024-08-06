using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using SourceChord.FluentWPF.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 掩膜分割视图模型
    /// </summary>
    public class MaskViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 矩形
        /// </summary>
        private Rectangle _rectangle;

        /// <summary>
        /// 圆形
        /// </summary>
        private CircleVisual2D _circle;

        /// <summary>
        /// 椭圆形
        /// </summary>
        private EllipseVisual2D _ellipse;

        /// <summary>
        /// 锚点集
        /// </summary>
        private IList<PointVisual2D> _polyAnchors;

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MaskViewModel(IWindowManager windowManager)
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

        #region 边框颜色 —— Color? BorderColor
        /// <summary>
        /// 边框颜色
        /// </summary>
        [DependencyProperty]
        public Color? BorderColor { get; set; }
        #endregion

        #region 粗细 —— int? Thickness
        /// <summary>
        /// 粗细
        /// </summary>
        [DependencyProperty]
        public int? Thickness { get; set; }
        #endregion

        #region 显示网格线 —— bool ShowGridLines
        /// <summary>
        /// 显示网格线
        /// </summary>
        [DependencyProperty]
        public bool ShowGridLines { get; set; }
        #endregion

        #region 网格线可见性 —— Visibility GridLinesVisibility
        /// <summary>
        /// 网格线可见性
        /// </summary>
        [DependencyProperty]
        public Visibility GridLinesVisibility { get; set; }
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

        #region 选中矩形 —— bool RectangleChecked
        /// <summary>
        /// 选中矩形
        /// </summary>
        [DependencyProperty]
        public bool RectangleChecked { get; set; }
        #endregion

        #region 选中圆形 —— bool CircleChecked
        /// <summary>
        /// 选中圆形
        /// </summary>
        [DependencyProperty]
        public bool CircleChecked { get; set; }
        #endregion

        #region 选中椭圆形 —— bool EllipseChecked
        /// <summary>
        /// 选中椭圆形
        /// </summary>
        [DependencyProperty]
        public bool EllipseChecked { get; set; }
        #endregion

        #region 选中多边形 —— bool PolygonChecked
        /// <summary>
        /// 选中多边形
        /// </summary>
        [DependencyProperty]
        public bool PolygonChecked { get; set; }
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
            this._polyAnchors = new List<PointVisual2D>();
            this.BorderColor = Colors.Red;
            this.Thickness = 2;
            this.ShowGridLines = true;
            this.GridLinesVisibility = Visibility.Visible;
            this.ScaleChecked = true;
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

        #region 切换显示网格线 —— void SwitchGridLines()
        /// <summary>
        /// 切换显示网格线
        /// </summary>
        public void SwitchGridLines()
        {
            this.GridLinesVisibility = this.ShowGridLines ? Visibility.Visible : Visibility.Collapsed;
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
            using Mat mask = Mat.Zeros(image.Size(), MatType.CV_8UC1);
            foreach (Shape shape in this.Shapes)
            {
                int thickness = -1;
                if (shape is Rectangle rectangle)
                {
                    RectangleL rectangleL = (RectangleL)rectangle.Tag;
                    Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                    mask.Rectangle(rect, Scalar.White, thickness);
                }
                if (shape is CircleVisual2D circle)
                {
                    CircleL circleL = (CircleL)circle.Tag;
                    mask.Circle(circleL.X, circleL.Y, circleL.Radius, Scalar.White, thickness);
                }
                if (shape is EllipseVisual2D ellipse)
                {
                    EllipseL ellipseL = (EllipseL)ellipse.Tag;
                    Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                    Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                    RotatedRect rect = new RotatedRect(center, size, 0);
                    mask.Ellipse(rect, Scalar.White, thickness);
                }
                if (shape is Polygon polygon)
                {
                    PolygonL polygonL = (PolygonL)polygon.Tag;

                    OpenCvSharp.Point[] contour = new OpenCvSharp.Point[polygonL.Points.Count];
                    for (int index = 0; index < polygonL.Points.Count; index++)
                    {
                        PointL pointL = polygonL.Points.ElementAt(index);
                        contour[index] = new OpenCvSharp.Point(pointL.X, pointL.Y);
                    }
                    mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness);
                }
            }

            //提取有效区域
            using Mat result = new Mat();
            image.CopyTo(result, mask);
            this.BitmapSource = result.ToBitmapSource();

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
                this.RectangleChecked = false;
                this.CircleChecked = false;
                this.EllipseChecked = false;
                this.PolygonChecked = false;
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
                this.RectangleChecked = false;
                this.CircleChecked = false;
                this.EllipseChecked = false;
                this.PolygonChecked = false;
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
                this.RectangleChecked = false;
                this.CircleChecked = false;
                this.EllipseChecked = false;
                this.PolygonChecked = false;
            }
        }
        #endregion

        #region 矩形点击事件 —— void OnRectangleClick()
        /// <summary>
        /// 矩形点击事件
        /// </summary>
        public void OnRectangleClick()
        {
            if (this.RectangleChecked)
            {
                this.CanvasMode = CanvasMode.Draw;

                this.ScaleChecked = false;
                this.DragChecked = false;
                this.ResizeChecked = false;
                this.CircleChecked = false;
                this.EllipseChecked = false;
                this.PolygonChecked = false;
            }
        }
        #endregion

        #region 圆形点击事件 —— void OnCircleClick()
        /// <summary>
        /// 圆形点击事件
        /// </summary>
        public void OnCircleClick()
        {
            if (this.CircleChecked)
            {
                this.CanvasMode = CanvasMode.Draw;

                this.ScaleChecked = false;
                this.DragChecked = false;
                this.ResizeChecked = false;
                this.RectangleChecked = false;
                this.EllipseChecked = false;
                this.PolygonChecked = false;
            }
        }
        #endregion

        #region 椭圆形点击事件 —— void OnEllipseClick()
        /// <summary>
        /// 椭圆形点击事件
        /// </summary>
        public void OnEllipseClick()
        {
            if (this.EllipseChecked)
            {
                this.CanvasMode = CanvasMode.Draw;

                this.ScaleChecked = false;
                this.DragChecked = false;
                this.ResizeChecked = false;
                this.RectangleChecked = false;
                this.CircleChecked = false;
                this.PolygonChecked = false;
            }
        }
        #endregion

        #region 多边形点击事件 —— void OnPolygonClick()
        /// <summary>
        /// 多边形点击事件
        /// </summary>
        public void OnPolygonClick()
        {
            if (this.PolygonChecked)
            {
                this.CanvasMode = CanvasMode.Draw;

                this.ScaleChecked = false;
                this.DragChecked = false;
                this.ResizeChecked = false;
                this.RectangleChecked = false;
                this.CircleChecked = false;
                this.EllipseChecked = false;
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

            if (canvas.SelectedVisual is Rectangle rectangle)
            {
                this.RebuildRectangle(rectangle, leftMargin, topMargin);
            }
            if (canvas.SelectedVisual is CircleVisual2D circle)
            {
                this.RebuildCircle(circle, leftMargin, topMargin);
            }
            if (canvas.SelectedVisual is EllipseVisual2D ellipse)
            {
                this.RebuildEllipse(ellipse, leftMargin, topMargin);
            }
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

            if (canvas.SelectedVisual is Rectangle rectangle)
            {
                Point retifiedVertex = new Point(leftMargin, topMargin);
                double width = canvas.RectifiedMousePosition!.Value.X - retifiedVertex.X;
                double height = canvas.RectifiedMousePosition!.Value.Y - retifiedVertex.Y;

                if (width > 0)
                {
                    rectangle.Width = width;
                }
                if (height > 0)
                {
                    rectangle.Height = height;
                }

                this.RebuildRectangle(rectangle, leftMargin, topMargin);

                return;
            }
            if (canvas.SelectedVisual is CircleVisual2D circle)
            {
                Point retifiedCenter = new Point(circle.Center.X + leftMargin, circle.Center.Y + topMargin);
                Vector vector = retifiedCenter - canvas.RectifiedMousePosition!.Value;
                circle.Radius = Math.Abs(vector.Length);

                this.RebuildCircle(circle, leftMargin, topMargin);

                return;
            }
            if (canvas.SelectedVisual is EllipseVisual2D ellipse)
            {
                Point retifiedCenter = new Point(ellipse.Center.X + leftMargin, ellipse.Center.Y + topMargin);
                ellipse.RadiusX = Math.Abs(retifiedCenter.X - canvas.RectifiedMousePosition!.Value.X);
                ellipse.RadiusY = Math.Abs(retifiedCenter.Y - canvas.RectifiedMousePosition!.Value.Y);

                this.RebuildEllipse(ellipse, leftMargin, topMargin);

                return;
            }
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

                return;
            }
        }
        #endregion

        #region 绘制开始事件 —— void OnDraw(CanvasEx canvas)
        /// <summary>
        /// 绘制开始事件
        /// </summary>
        public void OnDraw(CanvasEx canvas)
        {
            if (this.PolygonChecked)
            {
                if (canvas.SelectedVisual is PointVisual2D element && this._polyAnchors.Any() && element == this._polyAnchors[0])
                {
                    //多边形
                    if (this.PolygonChecked)
                    {
                        this.DrawPolygon(canvas);
                    }
                }
                else
                {
                    //锚点
                    this.DrawPolyAnchor(canvas);
                }
            }
        }
        #endregion

        #region 绘制中事件 —— void OnDrawing(CanvasEx canvas)
        /// <summary>
        /// 绘制中事件
        /// </summary>
        public void OnDrawing(CanvasEx canvas)
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                return;
            }

            #endregion

            if (this.RectangleChecked)
            {
                this.DrawRectangle(canvas);
            }
            if (this.CircleChecked)
            {
                this.DrawCircle(canvas);
            }
            if (this.EllipseChecked)
            {
                this.DrawEllipse(canvas);
            }
        }
        #endregion

        #region 绘制完成事件 —— void OnDrawn(CanvasEx canvas)
        /// <summary>
        /// 绘制完成事件
        /// </summary>
        public void OnDrawn(CanvasEx canvas)
        {
            if (this._rectangle != null)
            {
                int x = (int)Math.Ceiling(canvas.GetRectifiedLeft(this._rectangle));
                int y = (int)Math.Ceiling(canvas.GetRectifiedTop(this._rectangle));
                int width = (int)Math.Ceiling(this._rectangle.Width);
                int height = (int)Math.Ceiling(this._rectangle.Height);
                RectangleL rectangleL = new RectangleL(x, y, width, height);

                this._rectangle.Tag = rectangleL;
                rectangleL.Tag = this._rectangle;
                this.ShapeLs.Add(rectangleL);
                this.Shapes.Add(this._rectangle);
            }
            if (this._circle != null)
            {
                int x = (int)Math.Ceiling(this._circle.Center.X);
                int y = (int)Math.Ceiling(this._circle.Center.Y);
                int radius = (int)Math.Ceiling(this._circle.Radius);
                CircleL circleL = new CircleL(x, y, radius);

                this._circle.Tag = circleL;
                circleL.Tag = this._circle;
                this.ShapeLs.Add(circleL);
                this.Shapes.Add(this._circle);
            }
            if (this._ellipse != null)
            {
                int x = (int)Math.Ceiling(this._ellipse.Center.X);
                int y = (int)Math.Ceiling(this._ellipse.Center.Y);
                int radiusX = (int)Math.Ceiling(this._ellipse.RadiusX);
                int radiusY = (int)Math.Ceiling(this._ellipse.RadiusY);
                EllipseL ellipseL = new EllipseL(x, y, radiusX, radiusY);

                this._ellipse.Tag = ellipseL;
                ellipseL.Tag = this._ellipse;
                this.ShapeLs.Add(ellipseL);
                this.Shapes.Add(this._ellipse);
            }

            this._rectangle = null;
            this._circle = null;
            this._ellipse = null;
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

        #region 重建矩形 —— void RebuildRectangle(Rectangle rectangle, double leftMargin, double topMargin)
        /// <summary>
        /// 重建矩形
        /// </summary>
        /// <param name="rectangle">矩形</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildRectangle(Rectangle rectangle, double leftMargin, double topMargin)
        {
            RectangleL rectangleL = (RectangleL)rectangle.Tag;
            int index = this.ShapeLs.IndexOf(rectangleL);
            if (index != -1)
            {
                this.ShapeLs.Remove(rectangleL);

                int x = (int)Math.Ceiling(leftMargin);
                int y = (int)Math.Ceiling(topMargin);
                int width = (int)Math.Ceiling(rectangle.Width);
                int height = (int)Math.Ceiling(rectangle.Height);
                RectangleL newRectangleL = new RectangleL(x, y, width, height);

                rectangle.Tag = newRectangleL;
                this.ShapeLs.Insert(index, newRectangleL);
            }
        }
        #endregion

        #region 重建圆形 —— void RebuildCircle(CircleVisual2D circle, double leftMargin, double topMargin)
        /// <summary>
        /// 重建圆形
        /// </summary>
        /// <param name="circle">圆形</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildCircle(CircleVisual2D circle, double leftMargin, double topMargin)
        {
            CircleL circleL = (CircleL)circle.Tag;
            int index = this.ShapeLs.IndexOf(circleL);
            if (index != -1)
            {
                this.ShapeLs.Remove(circleL);

                int x = (int)Math.Ceiling(circle.Center.X + leftMargin);
                int y = (int)Math.Ceiling(circle.Center.Y + topMargin);
                int radius = (int)Math.Ceiling(circle.Radius);
                CircleL newCircleL = new CircleL(x, y, radius);

                circle.Tag = newCircleL;
                this.ShapeLs.Insert(index, newCircleL);
            }
        }
        #endregion

        #region 重建椭圆形 —— void RebuildEllipse(EllipseVisual2D ellipse, double leftMargin, double topMargin)
        /// <summary>
        /// 重建椭圆形
        /// </summary>
        /// <param name="ellipse">椭圆形</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildEllipse(EllipseVisual2D ellipse, double leftMargin, double topMargin)
        {
            EllipseL ellipseL = (EllipseL)ellipse.Tag;
            int index = this.ShapeLs.IndexOf(ellipseL);
            if (index != -1)
            {
                this.ShapeLs.Remove(ellipseL);

                int x = (int)Math.Ceiling(ellipse.Center.X + leftMargin);
                int y = (int)Math.Ceiling(ellipse.Center.Y + topMargin);
                int radiusX = (int)Math.Ceiling(ellipse.RadiusX);
                int radiusY = (int)Math.Ceiling(ellipse.RadiusY);
                EllipseL newEllipseL = new EllipseL(x, y, radiusX, radiusY);

                ellipse.Tag = newEllipseL;
                this.ShapeLs.Insert(index, newEllipseL);
            }
        }
        #endregion

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
                this.ShapeLs.Insert(index, newPolygonL);
            }
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle(CanvasEx canvas)
        /// <summary>
        /// 绘制矩形
        /// </summary>
        private void DrawRectangle(CanvasEx canvas)
        {
            if (this._rectangle == null)
            {
                this._rectangle = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(this.BorderColor!.Value),
                    StrokeThickness = this.Thickness!.Value
                };
                canvas.Children.Add(this._rectangle);
            }

            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
            int width = (int)Math.Round(Math.Abs(rectifiedPosition.X - rectifiedVertex.X));
            int height = (int)Math.Round(Math.Abs(rectifiedPosition.Y - rectifiedVertex.Y));
            this._rectangle.Width = width;
            this._rectangle.Height = height;
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this._rectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this._rectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this._rectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this._rectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this._rectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this._rectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this._rectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this._rectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            this._rectangle.RenderTransform = canvas.MatrixTransform;
            this._rectangle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
        }
        #endregion

        #region 绘制圆形 —— void DrawCircle(CanvasEx canvas)
        /// <summary>
        /// 绘制圆形
        /// </summary>
        private void DrawCircle(CanvasEx canvas)
        {
            if (this._circle == null)
            {
                this._circle = new CircleVisual2D
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(this.BorderColor!.Value),
                    StrokeThickness = this.Thickness!.Value
                };
                canvas.Children.Add(this._circle);
            }

            Point rectifiedCenter = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
            Vector vector = Point.Subtract(rectifiedPosition, rectifiedCenter);

            this._circle.Center = rectifiedCenter;
            this._circle.Radius = vector.Length;
            this._circle.RenderTransform = canvas.MatrixTransform;
            this._circle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
        }
        #endregion

        #region 绘制椭圆形 —— void DrawEllipse(CanvasEx canvas)
        /// <summary>
        /// 绘制椭圆形
        /// </summary>
        private void DrawEllipse(CanvasEx canvas)
        {
            if (this._ellipse == null)
            {
                this._ellipse = new EllipseVisual2D()
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(this.BorderColor!.Value),
                    StrokeThickness = this.Thickness!.Value
                };
                canvas.Children.Add(this._ellipse);
            }

            Point rectifiedCenter = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;

            this._ellipse.Center = rectifiedCenter;
            this._ellipse.RadiusX = Math.Abs(rectifiedPosition.X - rectifiedCenter.X);
            this._ellipse.RadiusY = Math.Abs(rectifiedPosition.Y - rectifiedCenter.Y);
            this._ellipse.RenderTransform = canvas.MatrixTransform;
            this._ellipse.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
        }
        #endregion

        #region 绘制锚点 —— void DrawPolyAnchor(CanvasEx canvas)
        /// <summary>
        /// 绘制锚点
        /// </summary>
        private void DrawPolyAnchor(CanvasEx canvas)
        {
            Point rectifiedPoint = canvas.RectifiedStartPosition!.Value;
            Brush fillBrush = new SolidColorBrush(Colors.Black);
            Brush borderBrush = this._polyAnchors.Any()
                ? new SolidColorBrush(this.BorderColor!.Value)
                : new SolidColorBrush(Colors.Yellow);
            PointVisual2D anchor = new PointVisual2D
            {
                X = rectifiedPoint.X,
                Y = rectifiedPoint.Y,
                Fill = fillBrush,
                Stroke = borderBrush,
                RenderTransform = canvas.MatrixTransform
            };
            canvas.Children.Add(anchor);
            this._polyAnchors.Add(anchor);
        }
        #endregion

        #region 绘制多边形 —— void DrawPolygon(CanvasEx canvas)
        /// <summary>
        /// 绘制多边形
        /// </summary>
        private void DrawPolygon(CanvasEx canvas)
        {
            //点集排序
            IEnumerable<Point> point2ds = this._polyAnchors.Select(point => new Point(point.X, point.Y));
            PointCollection points = new PointCollection(point2ds);
            points = points.Sequentialize();

            //构建点集
            IEnumerable<PointL> pointIs =
                from point in points
                let x = (int)Math.Ceiling(point.X)
                let y = (int)Math.Ceiling(point.Y)
                select new PointL(x, y);

            PolygonL polygonL = new PolygonL(pointIs);
            Polygon polygon = new Polygon
            {
                Fill = new SolidColorBrush(Colors.Transparent),
                Stroke = new SolidColorBrush(this.BorderColor!.Value),
                StrokeThickness = this.Thickness!.Value,
                Points = points,
                RenderTransform = canvas.MatrixTransform,
                Tag = polygonL
            };

            polygonL.Tag = polygon;
            this.ShapeLs.Add(polygonL);
            this.Shapes.Add(polygon);
            canvas.Children.Add(polygon);

            //清空锚点
            foreach (PointVisual2D anchor in this._polyAnchors)
            {
                canvas.Children.Remove(anchor);
            }
            this._polyAnchors.Clear();
            polygon.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
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
