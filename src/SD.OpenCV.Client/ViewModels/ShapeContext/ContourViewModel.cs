using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 轮廓查找视图模型
    /// </summary>
    public class ContourViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 多边形颜色列表
        /// </summary>
        private static readonly Color[] _PolyColors =
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
        public ContourViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 最小周长 —— double MinLength
        /// <summary>
        /// 最小周长
        /// </summary>
        [DependencyProperty]
        public double MinLength { get; set; }
        #endregion

        #region 最大周长 —— double MaxLength
        /// <summary>
        /// 最大周长
        /// </summary>
        [DependencyProperty]
        public double MaxLength { get; set; }
        #endregion

        #region 检测模式 —— RetrievalModes RetrievalMode
        /// <summary>
        /// 检测模式
        /// </summary>
        [DependencyProperty]
        public RetrievalModes RetrievalMode { get; set; }
        #endregion

        #region 检测模式字典 —— IDictionary<string, string> RetrievalModes
        /// <summary>
        /// 检测模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> RetrievalModes { get; set; }
        #endregion

        #region 近似模式 —— ContourApproximationModes ApproxMode
        /// <summary>
        /// 近似模式
        /// </summary>
        [DependencyProperty]
        public ContourApproximationModes ApproxMode { get; set; }
        #endregion

        #region 近似模式字典 —— IDictionary<string, string> ApproxModes
        /// <summary>
        /// 近似模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ApproxModes { get; set; }
        #endregion

        #region 已选形状数据 —— ShapeL SelectedShapeL
        /// <summary>
        /// 已选形状数据
        /// </summary>
        [DependencyProperty]
        public ShapeL SelectedShapeL { get; set; }
        #endregion

        #region 形状列表 —— ObservableCollection<Shape> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> Shapes { get; set; }
        #endregion

        #region 形状数据列表 —— ObservableCollection<ShapeL> ShapeLs
        /// <summary>
        /// 形状数据列表
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
            this.MinLength = 500;
            this.MaxLength = 20000;
            this.RetrievalMode = OpenCvSharp.RetrievalModes.List;
            this.ApproxMode = ContourApproximationModes.ApproxNone;
            this.RetrievalModes = typeof(RetrievalModes).GetEnumMembers();
            this.ApproxModes = typeof(ContourApproximationModes).GetEnumMembers();
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 查找轮廓 —— async void FindContours()
        /// <summary>
        /// 查找轮廓
        /// </summary>
        public async void FindContours()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //清空轮廓
            this.Shapes.Clear();
            this.ShapeLs.Clear();

            //查找轮廓
            using Mat image = this.BitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC1
                ? image
                : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Point[][] contours = { };
            await Task.Run(() => Cv2.FindContours(grayImage, out contours, out _, this.RetrievalMode, this.ApproxMode));

            //绘制轮廓
            IEnumerable<OpenCvSharp.Point[]> reservedContours =
                from contour in contours
                let length = Cv2.ArcLength(contour, true)
                where length >= this.MinLength && length <= this.MaxLength
                orderby length descending
                select contour;
            int index = 0;
            foreach (OpenCvSharp.Point[] contour in reservedContours)
            {
                PointCollection points = new PointCollection();
                IList<PointL> pointLs = new List<PointL>();
                foreach (OpenCvSharp.Point point in contour)
                {
                    Point point2D = new Point(point.X, point.Y);
                    PointL pointL = new PointL(point.X, point.Y);
                    points.Add(point2D);
                    pointLs.Add(pointL);
                }
                PolygonL polygonL = new PolygonL(pointLs);
                Polygon polygon = new Polygon
                {
                    Points = points,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(_PolyColors[index % 5]),
                    StrokeThickness = 2
                };

                polygon.Tag = polygonL;
                polygonL.Tag = polygon;
                this.Shapes.Add(polygon);
                this.ShapeLs.Add(polygonL);
                index++;
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
            using Mat colorImage = image.Type() == MatType.CV_8UC1
                ? image.CvtColor(ColorConversionCodes.GRAY2BGR)
                : image;
            foreach (Polygon polygon in this.Shapes.OfType<Polygon>())
            {
                PolygonL polygonL = (PolygonL)polygon.Tag;
                int thickness = (int)Math.Ceiling(polygon.StrokeThickness);
                SolidColorBrush borderBrush = (SolidColorBrush)polygon.Stroke;
                Scalar borderColor = new Scalar(borderBrush.Color.B, borderBrush.Color.G, borderBrush.Color.R);

                OpenCvSharp.Point[] contour = new OpenCvSharp.Point[polygonL.Points.Count];
                for (int index = 0; index < contour.Length; index++)
                {
                    PointL pointL = polygonL.Points.ElementAt(index);
                    contour[index] = new OpenCvSharp.Point(pointL.X, pointL.Y);
                }
                await Task.Run(() => colorImage.DrawContours(new[] { contour }, -1, borderColor, thickness));
            }

            this.BitmapSource = colorImage.ToBitmapSource();

            this.Idle();

            await base.TryCloseAsync(true);
        }
        #endregion

        #region 重置 —— override void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public override void Reset()
        {
            this.Shapes.Clear();
            this.ShapeLs.Clear();

            base.Reset();
        }
        #endregion


        //Events

        #region 选中形状事件 —— void OnSelectShape()
        /// <summary>
        /// 选中形状事件
        /// </summary>
        public void OnSelectShape()
        {
            if (this.SelectedShapeL != null)
            {
                Shape shape = (Shape)this.SelectedShapeL.Tag;
                shape.BlinkStroke();
            }
        }
        #endregion

        #region 形状鼠标左击事件 —— void OnShapeMouseLeftDown(CanvasEx canvas...
        /// <summary>
        /// 形状鼠标左击事件
        /// </summary>
        public void OnShapeMouseLeftDown(CanvasEx canvas, ShapeEventArgs eventArgs)
        {
            if (canvas.Mode != CanvasMode.Draw)
            {
                ShapeL shapeL = (ShapeL)eventArgs.Shape.Tag;
                this.SelectedShapeL = null;
                this.SelectedShapeL = shapeL;
            }
        }
        #endregion

        #endregion
    }
}
