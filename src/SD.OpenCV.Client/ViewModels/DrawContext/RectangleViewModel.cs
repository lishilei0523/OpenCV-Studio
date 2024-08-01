using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.DrawContext
{
    /// <summary>
    /// 绘制矩形视图模型
    /// </summary>
    public class RectangleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 顶点
        /// </summary>
        private Point? _vertex;

        /// <summary>
        /// 矩形
        /// </summary>
        private Rectangle _rectangle;

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public RectangleViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 颜色 —— Color? Color
        /// <summary>
        /// 颜色
        /// </summary>
        [DependencyProperty]
        public Color? Color { get; set; }
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

        #region 图像 —— Mat Image
        /// <summary>
        /// 图像
        /// </summary>
        public Mat Image { get; set; }
        #endregion

        #region 图像源 —— BitmapSource BitmapSource
        /// <summary>
        /// 图像源
        /// </summary>
        [DependencyProperty]
        public BitmapSource BitmapSource { get; set; }
        #endregion

        #region 已选矩形数据 —— RectangleL SelectedRectangleL
        /// <summary>
        /// 已选矩形数据
        /// </summary>
        [DependencyProperty]
        public RectangleL SelectedRectangleL { get; set; }
        #endregion

        #region 矩形集 —— ObservableCollection<Rectangle> Rectangles
        /// <summary>
        /// 矩形集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Rectangle> Rectangles { get; set; }
        #endregion

        #region 矩形数据集 —— ObservableCollection<RectangleL> RectangleLs
        /// <summary>
        /// 矩形数据集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<RectangleL> RectangleLs { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.Color = Colors.Red;
            this.Thickness = 2;
            this.ShowGridLines = true;
            this.GridLinesVisibility = Visibility.Visible;
            this.Rectangles = new ObservableCollection<Rectangle>();
            this.RectangleLs = new ObservableCollection<RectangleL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(BitmapSource bitmapSource)
        {
            this.Image = bitmapSource.ToMat();
            this.BitmapSource = bitmapSource;
        }
        #endregion

        #region 切换显示网格线 —— void SwitchGridLines()
        /// <summary>
        /// 切换显示网格线
        /// </summary>
        public void SwitchGridLines()
        {
            this.GridLinesVisibility = this.ShowGridLines ? Visibility.Visible : Visibility.Collapsed;
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

            foreach (Rectangle rectangle in this.Rectangles)
            {
                ScalableCanvas canvas = (ScalableCanvas)rectangle.Parent;
                int x = (int)Math.Ceiling(canvas.GetRectifiedLeft(rectangle));
                int y = (int)Math.Ceiling(canvas.GetRectifiedTop(rectangle));
                int width = (int)Math.Ceiling(rectangle.Width);
                int height = (int)Math.Ceiling(rectangle.Height);
                Rect rect = new Rect(x, y, width, height);
                int thickness = (int)Math.Ceiling(rectangle.StrokeThickness);
                SolidColorBrush brush = (SolidColorBrush)rectangle.Stroke;
                Scalar color = new Scalar(brush.Color.B, brush.Color.G, brush.Color.R);
                await Task.Run(() => this.Image.Rectangle(rect, color, thickness));
            }
            this.BitmapSource = this.Image.ToBitmapSource();

            this.Idle();

            await base.TryCloseAsync(true);
        }
        #endregion

        #region 鼠标左键按下事件 —— void OnMouseLeftDown(ScalableCanvas canvas...
        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        public void OnMouseLeftDown(ScalableCanvas canvas, MouseButtonEventArgs eventArgs)
        {
            //设置光标
            Mouse.OverrideCursor = Cursors.Cross;

            this._vertex = eventArgs.GetPosition(canvas);

            eventArgs.Handled = true;
        }
        #endregion

        #region 鼠标移动事件 —— void OnMouseMove(ScalableCanvas canvas...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public void OnMouseMove(ScalableCanvas canvas, MouseEventArgs eventArgs)
        {
            #region # 验证

            if (!this._vertex.HasValue)
            {
                return;
            }
            if (this.BitmapSource == null)
            {
                return;
            }

            #endregion

            if (eventArgs.LeftButton == MouseButtonState.Pressed)
            {
                //设置光标
                Mouse.OverrideCursor = Cursors.Cross;

                if (this._rectangle == null)
                {
                    this._rectangle = new Rectangle
                    {
                        Fill = Brushes.Transparent,
                        Stroke = new SolidColorBrush(this.Color!.Value),
                        StrokeThickness = this.Thickness!.Value
                    };
                    canvas.Children.Add(this._rectangle);
                }

                Point vertex = this._vertex.Value;
                Point position = eventArgs.GetPosition(canvas);
                Point rectifiedVertex = canvas.MatrixTransform.Inverse!.Transform(vertex);
                Point rectifiedPosition = canvas.MatrixTransform.Inverse!.Transform(position);

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

                eventArgs.Handled = true;
            }
        }
        #endregion

        #region 鼠标松开事件 —— void OnMouseUp()
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public void OnMouseUp()
        {
            //设置光标
            Mouse.OverrideCursor = Cursors.Arrow;

            if (this._rectangle != null)
            {
                this.Rectangles.Add(this._rectangle);

                ScalableCanvas canvas = (ScalableCanvas)this._rectangle.Parent;
                int x = (int)Math.Ceiling(canvas.GetRectifiedLeft(this._rectangle));
                int y = (int)Math.Ceiling(canvas.GetRectifiedTop(this._rectangle));
                int width = (int)Math.Ceiling(this._rectangle.Width);
                int height = (int)Math.Ceiling(this._rectangle.Height);
                this.RectangleLs.Add(new RectangleL(x, y, width, height));
            }

            this._vertex = null;
            this._rectangle = null;
        }
        #endregion

        #region 选中矩形事件 —— void OnSelectRectangle()
        /// <summary>
        /// 选中矩形事件
        /// </summary>
        public void OnSelectRectangle()
        {
            if (this.SelectedRectangleL != null)
            {
                int x = this.SelectedRectangleL.X;
                int y = this.SelectedRectangleL.Y;
                int width = this.SelectedRectangleL.Width;
                int height = this.SelectedRectangleL.Height;
                string rectangle = $"{{X:{x}, Y:{y}, Width:{width}, Height:{height}}}";
                Clipboard.SetText(rectangle);
                base.ToastSuccess("已复制剪贴板！");
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
                this.Image?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
