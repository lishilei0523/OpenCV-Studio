using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.DrawContext
{
    /// <summary>
    /// 绘制形状视图模型
    /// </summary>
    public class ShapeViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ShapeViewModel(IWindowManager windowManager)
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

        #region 边框颜色 —— Color BorderColor
        /// <summary>
        /// 边框颜色
        /// </summary>
        private Color _borderColor;

        /// <summary>
        /// 边框颜色
        /// </summary>
        public Color BorderColor
        {
            get => this._borderColor;
            set
            {
                this.Set(ref this._borderColor, value);
                this.BorderBrush = new SolidColorBrush(value);
            }
        }
        #endregion

        #region 边框画刷 —— SolidColorBrush BorderBrush
        /// <summary>
        /// 边框画刷
        /// </summary>
        [DependencyProperty]
        public SolidColorBrush BorderBrush { get; set; }
        #endregion

        #region 边框粗细 —— int BorderThickness
        /// <summary>
        /// 边框粗细
        /// </summary>
        [DependencyProperty]
        public int BorderThickness { get; set; }
        #endregion

        #region 显示参考线 —— bool ShowGuideLines
        /// <summary>
        /// 显示参考线
        /// </summary>
        [DependencyProperty]
        public bool ShowGuideLines { get; set; }
        #endregion

        #region 参考线可见性 —— Visibility GuideLinesVisibility
        /// <summary>
        /// 参考线可见性
        /// </summary>
        [DependencyProperty]
        public Visibility GuideLinesVisibility { get; set; }
        #endregion

        #region 已选形状数据 —— ShapeL SelectedShapeL
        /// <summary>
        /// 已选形状数据
        /// </summary>
        [DependencyProperty]
        public ShapeL SelectedShapeL { get; set; }
        #endregion

        #region 形状列表 —— ObservableCollection<Shape> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> Shapes { get; set; }
        #endregion

        #region 形状数据列表 —— ObservableCollection<ShapeL> ShapeLs
        /// <summary>
        /// 形状数据列表
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
            this.BorderColor = Colors.Red;
            this.BorderThickness = 2;
            this.ShowGuideLines = true;
            this.GuideLinesVisibility = Visibility.Visible;
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 切换显示参考线 —— void SwitchGuideLines()
        /// <summary>
        /// 切换显示参考线
        /// </summary>
        public void SwitchGuideLines()
        {
            this.GuideLinesVisibility = this.ShowGuideLines ? Visibility.Visible : Visibility.Collapsed;
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
                this.Shapes.Remove(shape);
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

            Scalar borderColor = new Scalar(this.BorderBrush.Color.B, this.BorderBrush.Color.G, this.BorderBrush.Color.R);
            using Mat image = this.BitmapSource.ToMat();
            foreach (ShapeL shapeL in this.ShapeLs)
            {
                if (shapeL is PointL pointL)
                {
                    int radius = (int)Math.Ceiling(PointVisual2D.DefaultThickness);
                    int thickness = (int)Math.Ceiling(PointVisual2D.DefaultStrokeThickness);
                    await Task.Run(() => image.Circle(pointL.X, pointL.Y, radius, borderColor, thickness));       //空心圆
                    await Task.Run(() => image.Circle(pointL.X, pointL.Y, radius - thickness, Scalar.Black, -1)); //实心圆
                }
                if (shapeL is LineL lineL)
                {
                    await Task.Run(() => image.Line(lineL.A.X, lineL.A.Y, lineL.B.X, lineL.B.Y, borderColor, this.BorderThickness));
                }
                if (shapeL is RectangleL rectangleL)
                {
                    Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                    await Task.Run(() => image.Rectangle(rect, borderColor, this.BorderThickness));
                }
                if (shapeL is CircleL circleL)
                {
                    await Task.Run(() => image.Circle(circleL.X, circleL.Y, circleL.Radius, borderColor, this.BorderThickness));
                }
                if (shapeL is EllipseL ellipseL)
                {
                    Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                    Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                    RotatedRect rect = new RotatedRect(center, size, 0);
                    await Task.Run(() => image.Ellipse(rect, borderColor, this.BorderThickness));
                }
                if (shapeL is PolygonL polygonL)
                {
                    Point[] contour = new Point[polygonL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL point = polygonL.Points.ElementAt(index);
                        contour[index] = new Point(point.X, point.Y);
                    }
                    await Task.Run(() => image.DrawContours(new[] { contour }, -1, borderColor, this.BorderThickness));
                }
                if (shapeL is PolylineL polylineL)
                {
                    Point[] contour = new Point[polylineL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL point = polylineL.Points.ElementAt(index);
                        contour[index] = new Point(point.X, point.Y);
                    }
                    await Task.Run(() => image.Polylines(new[] { contour }, false, borderColor, this.BorderThickness));
                }
            }
            this.BitmapSource = image.ToBitmapSource();

            this.Idle();

            await base.TryCloseAsync(true);
        }
        #endregion


        //Events

        #region 选中形状事件 —— void OnSelectShape()
        /// <summary>
        /// 选中形状事件
        /// </summary>
        public void OnSelectShape()
        {
            if (this.SelectedShapeL != null)
            {
                Shape shape = (Shape)this.SelectedShapeL.Tag;
                shape.Blink();
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(ShapeEventArgs eventArgs)
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        public void OnShapeMouseLeftDown(ShapeEventArgs eventArgs)
        {
            if (this.CanvasMode != CanvasMode.Draw)
            {
                this.SelectedShapeL = null;
                this.SelectedShapeL = eventArgs.ShapeL;
            }
        }
        #endregion

        #endregion
    }
}
