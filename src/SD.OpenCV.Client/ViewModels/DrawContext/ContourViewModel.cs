using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Visual2Ds;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.DrawContext
{
    /// <summary>
    /// 绘制轮廓视图模型
    /// </summary>
    public class ContourViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 点集
        /// </summary>
        private IList<Point> _points;

        /// <summary>
        /// 遮罩集
        /// </summary>
        private IList<CircleVisual2D> _shades;

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

        #region 已选多边形 —— Polygon SelectedPolygon
        /// <summary>
        /// 已选多边形
        /// </summary>
        [DependencyProperty]
        public Polygon SelectedPolygon { get; set; }
        #endregion

        #region 多边形集 —— ObservableCollection<Polygon> Polygons
        /// <summary>
        /// 多边形集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Polygon> Polygons { get; set; }
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
            this._points = new List<Point>();
            this._shades = new List<CircleVisual2D>();
            this.Polygons = new ObservableCollection<Polygon>();

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
            this.BitmapSource = this.Image.ToBitmapSource();
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

            foreach (Polygon polygon in this.Polygons)
            {
                OpenCvSharp.Point[] contour = polygon.Points.Select(point => new OpenCvSharp.Point(point.X, point.Y)).ToArray();
                int thickness = (int)Math.Ceiling(polygon.StrokeThickness);
                SolidColorBrush brush = (SolidColorBrush)polygon.Stroke;
                Scalar color = new Scalar(brush.Color.B, brush.Color.G, brush.Color.R);
                await Task.Run(() => this.Image.DrawContours(new[] { contour }, -1, color, thickness));
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

            Point point = eventArgs.GetPosition(canvas);
            Point rectifiedPoint = canvas.MatrixTransform.Inverse!.Transform(point);

            Brush colorBrush = new SolidColorBrush(this.Color!.Value);
            if (!this._points.Any())
            {
                colorBrush = new SolidColorBrush(Colors.Yellow);
            }

            CircleVisual2D shade = new CircleVisual2D
            {
                Fill = Brushes.Transparent,
                Stroke = colorBrush,
                StrokeThickness = this.Thickness!.Value,
                Center = rectifiedPoint,
                Radius = 3,
                RenderTransform = canvas.MatrixTransform
            };
            canvas.Children.Add(shade);

            this._points.Add(rectifiedPoint);
            this._shades.Add(shade);

            eventArgs.Handled = true;
        }
        #endregion

        #region 鼠标右键按下事件 —— void OnMouseRightDown(ScalableCanvas canvas...
        /// <summary>
        /// 鼠标右键按下事件
        /// </summary>
        public void OnMouseRightDown(ScalableCanvas canvas, MouseButtonEventArgs eventArgs)
        {
            //设置光标
            Mouse.OverrideCursor = Cursors.Arrow;

            if (this._points.Count > 1)
            {
                PointCollection points = new PointCollection(this._points);
                points = points.Sequentialize();
                Polygon polygon = new Polygon
                {
                    Fill = Brushes.Transparent,
                    Stroke = new SolidColorBrush(this.Color!.Value),
                    StrokeThickness = this.Thickness!.Value,
                    Points = points,
                    RenderTransform = canvas.MatrixTransform
                };
                canvas.Children.Add(polygon);
                this.Polygons.Add(polygon);
            }

            //清空遮罩
            foreach (CircleVisual2D shade in this._shades)
            {
                canvas.Children.Remove(shade);
            }

            this._points.Clear();
            this._shades.Clear();

            eventArgs.Handled = true;
        }
        #endregion

        #region 选中多边形事件 —— void OnSelectPolygon()
        /// <summary>
        /// 选中多边形事件
        /// </summary>
        public void OnSelectPolygon()
        {
            if (this.SelectedPolygon != null)
            {
                string polygon = this.SelectedPolygon.Points.ToString();
                Clipboard.SetText(polygon);
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
