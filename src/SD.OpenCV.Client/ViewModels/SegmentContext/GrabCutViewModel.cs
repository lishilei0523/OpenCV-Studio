using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// GrabCut分割视图模型
    /// </summary>
    public class GrabCutViewModel : PreviewViewModel
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
            this.RectangleChecked = true;
            this.OnRectangleClick();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 分割图像 —— async void SegmentImage()
        /// <summary>
        /// 分割图像
        /// </summary>
        public async void SegmentImage()
        {
            #region # 验证

            if (this.RectangleL == null)
            {
                MessageBox.Show("矩形区域不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
        }
        #endregion

        #region 制作掩膜 —— async void MakeMask()
        /// <summary>
        /// 制作掩膜
        /// </summary>
        public async void MakeMask()
        {
            #region # 验证

            if (this.RectangleL == null)
            {
                MessageBox.Show("矩形区域不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
            this.BitmapSource = mask.ToBitmapSource();
            mask.Dispose();

            this.Idle();
        }
        #endregion

        #region 重置 —— override void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            if (this.Rectangle != null)
            {
                CanvasEx canvasEx = this.Rectangle.Parent as CanvasEx;
                canvasEx?.Children.Remove(this.Rectangle);
            }
            this.Rectangle = null;
            this.RectangleL = null;

            base.Reset();
        }
        #endregion


        //Events

        #region 拖拽点击事件 —— void OnDragClick()
        /// <summary>
        /// 拖拽点击事件
        /// </summary>
        public void OnDragClick()
        {
            if (this.DragChecked)
            {
                this.CanvasMode = CanvasMode.Drag;

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
            this.Rectangle.Tag = rectangle;
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
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2
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

            //重建矩形
            double leftMargin = canvas.GetRectifiedLeft(this.Rectangle);
            double topMargin = canvas.GetRectifiedTop(this.Rectangle);
            this.RebuildRectangle(this.Rectangle, leftMargin, topMargin);
        }
        #endregion

        #endregion
    }
}
