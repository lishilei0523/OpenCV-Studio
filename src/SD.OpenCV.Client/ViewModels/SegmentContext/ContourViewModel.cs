﻿using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 轮廓分割视图模型
    /// </summary>
    public class ContourViewModel : PreviewViewModel
    {
        #region # 字段及构造器

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

        #region 选中多边形 —— bool PolygonChecked
        /// <summary>
        /// 选中多边形
        /// </summary>
        [DependencyProperty]
        public bool PolygonChecked { get; set; }
        #endregion

        #region 多边形 —— Polygon Polygon
        /// <summary>
        /// 多边形
        /// </summary>
        [DependencyProperty]
        public Polygon Polygon { get; set; }
        #endregion

        #region 多边形数据 —— PolygonL PolygonL
        /// <summary>
        /// 多边形数据
        /// </summary>
        [DependencyProperty]
        public PolygonL PolygonL { get; set; }
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
            this.PolygonChecked = true;
            this.OnPolygonClick();
            this._polyAnchors = new List<PointVisual2D>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 应用 —— void Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public void Apply()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.Polygon == null || (this.Polygon != null && !this.Polygon.Points.Any()))
            {
                MessageBox.Show("多边形不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            //生成掩膜
            OpenCvSharp.Point[] contour = this.PolygonL.Points.Select(pointL => new OpenCvSharp.Point(pointL.X, pointL.Y)).ToArray();
            using Mat mask = this.Image.GenerateMask(contour);

            //适用掩膜
            using Mat canvas = new Mat();
            this.Image.CopyTo(canvas, mask);

            //提取有效区域
            Rect boundingRect = Cv2.BoundingRect(contour);
            using Mat result = canvas[boundingRect];
            this.BitmapSource = result.ToBitmapSource();

            //重置多边形
            this.Polygon.Points.Clear();
        }
        #endregion

        #region 重置 —— override void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            foreach (PointVisual2D anchor in this._polyAnchors)
            {
                CanvasEx canvasEx = anchor.Parent as CanvasEx;
                canvasEx?.Children.Remove(anchor);
            }
            if (this.Polygon != null)
            {
                CanvasEx canvasEx = this.Polygon.Parent as CanvasEx;
                canvasEx?.Children.Remove(this.Polygon);
                this.Polygon.Points.Clear();
            }
            this.Polygon = null;
            this.PolygonL = null;
            this._polyAnchors.Clear();

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

                this.DragChecked = false;
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
                    if (this.Polygon != null && this.Polygon.Points.Any())
                    {
                        this.Polygon.Points.Clear();
                    }
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
            this.PolygonL = newPolygonL;
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
                ? new SolidColorBrush(Colors.Red)
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
            IEnumerable<Point> point2ds =
                from anchor in this._polyAnchors
                let leftMargin = canvas.GetRectifiedLeft(anchor)
                let topMargin = canvas.GetRectifiedTop(anchor)
                select new Point(anchor.X + leftMargin, anchor.Y + topMargin);
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
                Stroke = new SolidColorBrush(Colors.Red),
                StrokeThickness = 2,
                Points = points,
                RenderTransform = canvas.MatrixTransform,
                Tag = polygonL
            };

            polygonL.Tag = polygon;

            canvas.Children.Remove(this.Polygon);
            this.PolygonL = polygonL;
            this.Polygon = polygon;
            canvas.Children.Add(this.Polygon);

            //清空锚点
            foreach (PointVisual2D anchor in this._polyAnchors)
            {
                canvas.Children.Remove(anchor);
            }
            this._polyAnchors.Clear();
        }
        #endregion

        #endregion
    }
}
