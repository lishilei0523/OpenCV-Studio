using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SourceChord.FluentWPF.Animations;
using System;
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
    /// 霍夫线查找视图模型
    /// </summary>
    public class LineViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 线段颜色列表
        /// </summary>
        private static readonly Color[] _LineColors =
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
        public LineViewModel(IWindowManager windowManager)
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

        #region 距离精度 —— double? Rho
        /// <summary>
        /// 距离精度
        /// </summary>
        [DependencyProperty]
        public double? Rho { get; set; }
        #endregion

        #region 角度精度 —— double? Theta
        /// <summary>
        /// 角度精度
        /// </summary>
        [DependencyProperty]
        public double? Theta { get; set; }
        #endregion

        #region 阈值 —— int? Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public int? Threshold { get; set; }
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
            this.Rho = 1;
            this.Theta = Math.PI / 180;
            this.Threshold = 100;
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

        #region 查找线段 —— async void HoughFindLines()
        /// <summary>
        /// 查找线段
        /// </summary>
        public async void HoughFindLines()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Rho.HasValue)
            {
                MessageBox.Show("距离精度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Theta.HasValue)
            {
                MessageBox.Show("角度精度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Threshold.HasValue)
            {
                MessageBox.Show("阈值不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //清空线段
            this.ShapeLs.Clear();
            this.Shapes.Clear();

            //查找线段
            using Mat image = this.BitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC1 ? image : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            LineSegmentPoint[] lines = await Task.Run(() => Cv2.HoughLinesP(grayImage, this.Rho!.Value, this.Theta!.Value, this.Threshold!.Value));

            //绘制线段
            for (int index = 0; index < lines.Length; index++)
            {
                LineSegmentPoint lineSegmentPoint = lines[index];
                LineL lineL = new LineL(new PointL(lineSegmentPoint.P1.X, lineSegmentPoint.P1.Y), new PointL(lineSegmentPoint.P2.X, lineSegmentPoint.P2.Y));
                Line line = new Line
                {
                    X1 = lineSegmentPoint.P1.X,
                    Y1 = lineSegmentPoint.P1.Y,
                    X2 = lineSegmentPoint.P2.X,
                    Y2 = lineSegmentPoint.P2.Y,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(_LineColors[index % 5]),
                    StrokeThickness = 2,
                    Tag = lineL
                };
                lineL.Tag = line;
                this.ShapeLs.Add(lineL);
                this.Shapes.Add(line);
                line.MouseLeftButtonDown += this.OnShapeMouseLeftDown;
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
                if (shape is Line line)
                {
                    LineL lineL = (LineL)line.Tag;
                    await Task.Run(() => colorImage.Line(lineL.A.X, lineL.A.Y, lineL.B.X, lineL.B.Y, borderColor, thickness));
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
            if (canvas.SelectedVisual is Line line)
            {
                this.RebuildLine(line, leftMargin, topMargin);
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

            if (canvas.SelectedVisual is Line line)
            {
                Vector vectorA = canvas.RectifiedMousePosition!.Value - new Point(line.X1 + leftMargin, line.Y1 + topMargin);
                Vector vectorB = canvas.RectifiedMousePosition!.Value - new Point(line.X2 + leftMargin, line.Y2 + topMargin);
                if (vectorA.Length < vectorB.Length)
                {
                    line.X1 = canvas.RectifiedMousePosition!.Value.X - leftMargin;
                    line.Y1 = canvas.RectifiedMousePosition!.Value.Y - topMargin;
                }
                else
                {
                    line.X2 = canvas.RectifiedMousePosition!.Value.X - leftMargin;
                    line.Y2 = canvas.RectifiedMousePosition!.Value.Y - topMargin;
                }

                this.RebuildLine(line, leftMargin, topMargin);
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

        #region 重建线段 —— void RebuildLine(Line line, double leftMargin, double topMargin)
        /// <summary>
        /// 重建线段
        /// </summary>
        /// <param name="line">线段</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildLine(Line line, double leftMargin, double topMargin)
        {
            LineL lineL = (LineL)line.Tag;
            int index = this.ShapeLs.IndexOf(lineL);
            if (index != -1)
            {
                this.ShapeLs.Remove(lineL);

                int x1 = (int)Math.Ceiling(line.X1 + leftMargin);
                int y1 = (int)Math.Ceiling(line.Y1 + topMargin);
                int x2 = (int)Math.Ceiling(line.X2 + leftMargin);
                int y2 = (int)Math.Ceiling(line.Y2 + topMargin);
                LineL newLineL = new LineL(new PointL(x1, y1), new PointL(x2, y2));

                line.Tag = newLineL;
                newLineL.Tag = line;
                this.ShapeLs.Insert(index, newLineL);
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
