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
using SD.Infrastructure.WPF.Visual2Ds;
using SourceChord.FluentWPF.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 霍夫圆查找视图模型
    /// </summary>
    public class CircleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 圆颜色列表
        /// </summary>
        private static readonly Color[] _CircleColors =
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
        public CircleViewModel(IWindowManager windowManager)
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

        #region 查找模式 —— HoughModes HoughMode
        /// <summary>
        /// 查找模式
        /// </summary>
        [DependencyProperty]
        public HoughModes HoughMode { get; set; }
        #endregion

        #region 分辨率 —— double? Dp
        /// <summary>
        /// 分辨率
        /// </summary>
        [DependencyProperty]
        public double? Dp { get; set; }
        #endregion

        #region 圆心最小距离 —— double? MinDistance
        /// <summary>
        /// 圆心最小距离
        /// </summary>
        [DependencyProperty]
        public double? MinDistance { get; set; }
        #endregion

        #region 最小半径 —— int? MinRadius
        /// <summary>
        /// 最小半径
        /// </summary>
        [DependencyProperty]
        public int? MinRadius { get; set; }
        #endregion

        #region 最大半径 —— int? MaxRadius
        /// <summary>
        /// 最大半径
        /// </summary>
        [DependencyProperty]
        public int? MaxRadius { get; set; }
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

        #region 查找模式字典 —— IDictionary<string, string> HoughModes
        /// <summary>
        /// 查找模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> HoughModes { get; set; }
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
            this.HoughMode = OpenCvSharp.HoughModes.Gradient;
            this.Dp = 1;
            this.MinDistance = 200;
            this.MinRadius = 0;
            this.MaxRadius = 500;
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();
            this.HoughModes = typeof(HoughModes).GetEnumMembers();

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

        #region 查找圆形 —— async void HoughFindCircles()
        /// <summary>
        /// 查找圆形
        /// </summary>
        public async void HoughFindCircles()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Dp.HasValue)
            {
                MessageBox.Show("分辨率不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinDistance.HasValue)
            {
                MessageBox.Show("圆心最小距离不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MinRadius.HasValue)
            {
                MessageBox.Show("最小半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.MaxRadius.HasValue)
            {
                MessageBox.Show("最大半径不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //清空圆形
            this.ShapeLs.Clear();
            this.Shapes.Clear();

            //查找圆形
            using Mat image = this.BitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC1 ? image : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            CircleSegment[] circles = await Task.Run(() => Cv2.HoughCircles(grayImage, this.HoughMode, this.Dp!.Value, this.MinDistance!.Value, 100D, 100D, this.MinRadius!.Value, this.MaxRadius!.Value));

            //绘制圆形
            for (int index = 0; index < circles.Length; index++)
            {
                CircleSegment circleSegment = circles[index];
                OpenCvSharp.Point center = circleSegment.Center.ToPoint();
                int radius = (int)Math.Ceiling(circleSegment.Radius);

                CircleL circleL = new CircleL(center.X, center.Y, radius);
                CircleVisual2D circle = new CircleVisual2D
                {
                    Center = new Point(circleSegment.Center.X, circleSegment.Center.Y),
                    Radius = circleSegment.Radius,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(_CircleColors[index % 5]),
                    StrokeThickness = 2,
                    Tag = circleL
                };

                circleL.Tag = circle;
                this.ShapeLs.Add(circleL);
                this.Shapes.Add(circle);
                circle.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
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
                this.ToastSuccess("已复制剪贴板！");
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
                if (shape is CircleVisual2D circle)
                {
                    CircleL circleL = (CircleL)circle.Tag;
                    await Task.Run(() => image.Circle(circleL.X, circleL.Y, circleL.Radius, borderColor, thickness));
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
            if (canvas.SelectedVisual is CircleVisual2D circle)
            {
                this.RebuildCircle(circle, leftMargin, topMargin);
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

            if (canvas.SelectedVisual is CircleVisual2D circle)
            {
                Point retifiedCenter = new Point(circle.Center.X + leftMargin, circle.Center.Y + topMargin);
                Vector vector = retifiedCenter - canvas.RectifiedMousePosition!.Value;
                circle.Radius = Math.Abs(vector.Length);

                this.RebuildCircle(circle, leftMargin, topMargin);
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
                newCircleL.Tag = circle;
                this.ShapeLs.Insert(index, newCircleL);
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
