using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.RectifyContext
{
    /// <summary>
    /// 矫正污点视图模型
    /// </summary>
    public class InpaintViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public InpaintViewModel(IWindowManager windowManager)
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

        #region 矩形 —— RectangleVisual2D Rectangle
        /// <summary>
        /// 矩形
        /// </summary>
        [DependencyProperty]
        public RectangleVisual2D Rectangle { get; set; }
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

        #region 应用 —— async void Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public async void Apply()
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
            using Mat image = this.BitmapSource.ToMat();
            using Mat result = await Task.Run(() => image.Inpaint(rect));
            this.BitmapSource = result.ToBitmapSource();

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
            if (canvas.SelectedVisual is RectangleVisual2D rectangle)
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
            if (canvas.SelectedVisual is RectangleVisual2D rectangle)
            {
                double leftMargin = canvas.GetRectifiedLeft(canvas.SelectedVisual);
                double topMargin = canvas.GetRectifiedTop(canvas.SelectedVisual);
                Point retifiedVertex = new Point(rectangle.Location.X + leftMargin, rectangle.Location.Y + topMargin);

                double width = canvas.RectifiedMousePosition!.Value.X - retifiedVertex.X;
                double height = canvas.RectifiedMousePosition!.Value.Y - retifiedVertex.Y;
                if (width > 0 && height > 0)
                {
                    rectangle.Size = new System.Windows.Size(width, height);
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

        #region 重建矩形 —— void RebuildRectangle(RectangleVisual2D rectangle...
        /// <summary>
        /// 重建矩形
        /// </summary>
        /// <param name="rectangle">矩形</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildRectangle(RectangleVisual2D rectangle, double leftMargin, double topMargin)
        {
            int x = (int)Math.Ceiling(rectangle.Location.X + leftMargin);
            int y = (int)Math.Ceiling(rectangle.Location.Y + topMargin);
            int width = (int)Math.Ceiling(rectangle.Size.Width);
            int height = (int)Math.Ceiling(rectangle.Size.Height);
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
                this.Rectangle = new RectangleVisual2D
                {
                    RenderTransform = canvas.MatrixTransform
                };
                canvas.Children.Add(this.Rectangle);
            }

            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;

            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                this.Rectangle.Location = rectifiedVertex;
            }
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                this.Rectangle.Location = new Point(rectifiedVertex.X, rectifiedPosition.Y);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                this.Rectangle.Location = new Point(rectifiedPosition.X, rectifiedVertex.Y);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                this.Rectangle.Location = rectifiedPosition;
            }

            double width = Math.Abs(rectifiedPosition.X - rectifiedVertex.X);
            double height = Math.Abs(rectifiedPosition.Y - rectifiedVertex.Y);
            this.Rectangle.Size = new System.Windows.Size(width, height);

            //重建矩形
            double leftMargin = canvas.GetRectifiedLeft(this.Rectangle);
            double topMargin = canvas.GetRectifiedTop(this.Rectangle);
            this.RebuildRectangle(this.Rectangle, leftMargin, topMargin);
        }
        #endregion

        #endregion
    }
}
