using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.Constants;
using SD.Infrastructure.WPF.CustomControls;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// GrabCut分割视图模型
    /// </summary>
    public class GrabCutViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public GrabCutViewModel(IWindowManager windowManager)
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

        #region 矩形 —— Rectangle Rectangle
        /// <summary>
        /// 矩形
        /// </summary>
        [DependencyProperty]
        public Rectangle Rectangle { get; set; }
        #endregion

        #region 矩形数据 —— RectangleL RectangleL
        /// <summary>
        /// 矩形数据
        /// </summary>
        [DependencyProperty]
        public RectangleL RectangleL { get; set; }
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
            this.Thickness = 2;
            this.ShowGridLines = true;
            this.GridLinesVisibility = Visibility.Visible;
            this.ScaleChecked = true;

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

            Rect rect = new Rect(this.RectangleL.X, this.RectangleL.Y, this.RectangleL.Width, this.RectangleL.Height);
            Mat mask = null;
            using Mat result = await Task.Run(() => this.Image.GrabCutSegment(rect, out mask));
            this.BitmapSource = result.ToBitmapSource();
            mask?.Dispose();

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
        }
        #endregion

        #region 绘制完成事件 —— void OnDrawn(CanvasEx canvas)
        /// <summary>
        /// 绘制完成事件
        /// </summary>
        public void OnDrawn(CanvasEx canvas)
        {
            if (this.Rectangle != null)
            {
                int x = (int)Math.Ceiling(canvas.GetRectifiedLeft(this.Rectangle));
                int y = (int)Math.Ceiling(canvas.GetRectifiedTop(this.Rectangle));
                int width = (int)Math.Ceiling(this.Rectangle.Width);
                int height = (int)Math.Ceiling(this.Rectangle.Height);
                this.RectangleL = new RectangleL(x, y, width, height);

                this.Rectangle.Tag = this.RectangleL;
                this.RectangleL.Tag = this.Rectangle;
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
            int x = (int)Math.Ceiling(leftMargin);
            int y = (int)Math.Ceiling(topMargin);
            int width = (int)Math.Ceiling(rectangle.Width);
            int height = (int)Math.Ceiling(rectangle.Height);
            this.RectangleL = new RectangleL(x, y, width, height);
            rectangle.Tag = this.RectangleL;
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle(CanvasEx canvas)
        /// <summary>
        /// 绘制矩形
        /// </summary>
        private void DrawRectangle(CanvasEx canvas)
        {
            if (this.Rectangle == null)
            {
                this.Rectangle = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(this.BorderColor!.Value),
                    StrokeThickness = this.Thickness!.Value
                };
                canvas.Children.Add(this.Rectangle);
            }

            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
            int width = (int)Math.Round(Math.Abs(rectifiedPosition.X - rectifiedVertex.X));
            int height = (int)Math.Round(Math.Abs(rectifiedPosition.Y - rectifiedVertex.Y));
            this.Rectangle.Width = width;
            this.Rectangle.Height = height;
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.Rectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this.Rectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.Rectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this.Rectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.Rectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this.Rectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.Rectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this.Rectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            this.Rectangle.RenderTransform = canvas.MatrixTransform;
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
