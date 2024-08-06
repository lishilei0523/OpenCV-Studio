using Caliburn.Micro;
using OpenCvSharp;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 透视绘制视图模型
    /// </summary>
    public class PerspectiveDrawViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public PerspectiveDrawViewModel(IWindowManager windowManager)
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

        #region 参考图像 —— BitmapSource SourceImage
        /// <summary>
        /// 参考图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource SourceImage { get; set; }
        #endregion

        #region 目标图像 —— BitmapSource TargetImage
        /// <summary>
        /// 目标图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource TargetImage { get; set; }
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

        #region 透视变换矩阵 —— Mat PerspectiveMatrix
        /// <summary>
        /// 透视变换矩阵
        /// </summary>
        public Mat PerspectiveMatrix { get; set; }
        #endregion

        #region 矩形 —— Rectangle Rectangle
        /// <summary>
        /// 矩形
        /// </summary>
        [DependencyProperty]
        public Rectangle Rectangle { get; set; }
        #endregion

        #region 多边形 —— Polygon Polygon
        /// <summary>
        /// 多边形
        /// </summary>
        [DependencyProperty]
        public Polygon Polygon { get; set; }
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
            this.Polygon = new Polygon();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(BitmapSource sourceImage...
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sourceImage">参考图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="perspectiveMatrix">透视矩阵</param>
        public void Load(BitmapSource sourceImage, BitmapSource targetImage, Mat perspectiveMatrix)
        {
            this.SourceImage = sourceImage;
            this.TargetImage = targetImage;
            this.PerspectiveMatrix = perspectiveMatrix;
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

            if (this.SourceImage == null)
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
            RectangleL rectangleL = new RectangleL(x, y, width, height);

            Point2f pointA = new Point2f(rectangleL.TopLeft.X, rectangleL.TopLeft.Y);
            Point2f pointB = new Point2f(rectangleL.BottomLeft.X, rectangleL.BottomLeft.Y);
            Point2f pointC = new Point2f(rectangleL.BottomRight.X, rectangleL.BottomRight.Y);
            Point2f pointD = new Point2f(rectangleL.TopRight.X, rectangleL.TopRight.Y);
            IList<Point2f> sourceCoutour = new[] { pointA, pointB, pointC, pointD };
            IList<Point2f> targetCoutour = Cv2.PerspectiveTransform(sourceCoutour, this.PerspectiveMatrix.Inv());
            PointCollection polyPoints = new PointCollection();
            foreach (Point2f point in targetCoutour)
            {
                Point polyPoint = new Point(point.X, point.Y);
                polyPoints.Add(polyPoint);
            }
            this.Polygon.Points = polyPoints.Sequentialize();
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

            //绘制目标
            double leftMargin = canvas.GetRectifiedLeft(this.Rectangle);
            double topMargin = canvas.GetRectifiedTop(this.Rectangle);
            this.RebuildRectangle(this.Rectangle, leftMargin, topMargin);
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
                this.PerspectiveMatrix?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
