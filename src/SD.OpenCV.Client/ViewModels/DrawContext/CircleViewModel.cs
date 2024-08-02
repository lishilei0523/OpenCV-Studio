using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Visual2Ds;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.DrawContext
{
    /// <summary>
    /// 绘制圆形视图模型
    /// </summary>
    public class CircleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 圆心
        /// </summary>
        private Point? _center;

        /// <summary>
        /// 圆形
        /// </summary>
        private CircleVisual2D _circle;

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

        #region 已选圆形 —— CircleVisual2D SelectedCircle
        /// <summary>
        /// 已选圆形
        /// </summary>
        [DependencyProperty]
        public CircleVisual2D SelectedCircle { get; set; }
        #endregion

        #region 圆形集 —— ObservableCollection<CircleVisual2D> Circles
        /// <summary>
        /// 圆形集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<CircleVisual2D> Circles { get; set; }
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
            this.Circles = new ObservableCollection<CircleVisual2D>();

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

            foreach (CircleVisual2D circle in this.Circles)
            {
                int x = (int)Math.Ceiling(circle.Center.X);
                int y = (int)Math.Ceiling(circle.Center.Y);
                int radius = (int)Math.Ceiling(circle.Radius);
                int thickness = (int)Math.Ceiling(circle.StrokeThickness);
                SolidColorBrush brush = (SolidColorBrush)circle.Stroke;
                Scalar color = new Scalar(brush.Color.B, brush.Color.G, brush.Color.R);
                await Task.Run(() => this.Image.Circle(x, y, radius, color, thickness));
            }
            this.BitmapSource = this.Image.ToBitmapSource();

            this.Idle();

            await base.TryCloseAsync(true);
        }
        #endregion

        #region 鼠标左键按下事件 —— void OnMouseLeftDown(CanvasEx canvas...
        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        public void OnMouseLeftDown(CanvasEx canvas, MouseButtonEventArgs eventArgs)
        {
            //设置光标
            Mouse.OverrideCursor = Cursors.Cross;

            this._center = eventArgs.GetPosition(canvas);

            eventArgs.Handled = true;
        }
        #endregion

        #region 鼠标移动事件 —— void OnMouseMove(CanvasEx canvas...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public void OnMouseMove(CanvasEx canvas, MouseEventArgs eventArgs)
        {
            #region # 验证

            if (!this._center.HasValue)
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

                if (this._circle == null)
                {
                    this._circle = new CircleVisual2D()
                    {
                        Fill = Brushes.Transparent,
                        Stroke = new SolidColorBrush(this.Color!.Value),
                        StrokeThickness = this.Thickness!.Value
                    };
                    canvas.Children.Add(this._circle);
                }

                Point center = this._center.Value;
                Point position = eventArgs.GetPosition(canvas);
                Point rectifiedCenter = canvas.MatrixTransform.Inverse!.Transform(center);
                Point rectifiedPosition = canvas.MatrixTransform.Inverse!.Transform(position);
                Vector vector = Point.Subtract(rectifiedPosition, rectifiedCenter);
                this._circle.Center = rectifiedCenter;
                this._circle.Radius = vector.Length;
                Trace.WriteLine($"圆心: {rectifiedCenter}, 半径: {vector.Length}");
                this._circle.RenderTransform = canvas.MatrixTransform;

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

            if (this._circle != null)
            {
                this.Circles.Add(this._circle);
            }

            this._center = null;
            this._circle = null;
        }
        #endregion

        #region 选中圆形事件 —— void OnSelectCircle()
        /// <summary>
        /// 选中圆形事件
        /// </summary>
        public void OnSelectCircle()
        {
            if (this.SelectedCircle != null)
            {
                int x = (int)Math.Ceiling(this.SelectedCircle.Center.X);
                int y = (int)Math.Ceiling(this.SelectedCircle.Center.Y);
                int radius = (int)Math.Ceiling(this.SelectedCircle.Radius);
                string circle = $"{{X:{x}, Y:{y}, Radius:{radius}}}";
                Clipboard.SetText(circle);
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
