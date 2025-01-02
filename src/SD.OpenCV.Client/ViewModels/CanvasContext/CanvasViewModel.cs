using Caliburn.Micro;
using Microsoft.WindowsAPICodePack.Dialogs;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace SD.OpenCV.Client.ViewModels.CanvasContext
{
    /// <summary>
    /// Canvas视图模型
    /// </summary>
    public class CanvasViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public CanvasViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 图像路径 —— string ImagePath
        /// <summary>
        /// 图像路径
        /// </summary>
        public string ImagePath { get; set; }
        #endregion

        #region 边框颜色 —— Color BorderColor
        /// <summary>
        /// 边框颜色
        /// </summary>
        private Color _borderColor;

        /// <summary>
        /// 边框颜色
        /// </summary>
        public Color BorderColor
        {
            get => this._borderColor;
            set
            {
                this.Set(ref this._borderColor, value);
                this.BorderBrush = new SolidColorBrush(value);
            }
        }
        #endregion

        #region 边框画刷 —— SolidColorBrush BorderBrush
        /// <summary>
        /// 边框画刷
        /// </summary>
        [DependencyProperty]
        public SolidColorBrush BorderBrush { get; set; }
        #endregion

        #region 边框粗细 —— int BorderThickness
        /// <summary>
        /// 边框粗细
        /// </summary>
        [DependencyProperty]
        public int BorderThickness { get; set; }
        #endregion

        #region 显示参考线 —— bool ShowGuideLines
        /// <summary>
        /// 显示参考线
        /// </summary>
        private bool _showGuideLines;

        /// <summary>
        /// 显示参考线
        /// </summary>
        public bool ShowGuideLines
        {
            get => this._showGuideLines;
            set
            {
                this.Set(ref this._showGuideLines, value);
                this.GuideLinesVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region 参考线可见性 —— Visibility GuideLinesVisibility
        /// <summary>
        /// 参考线可见性
        /// </summary>
        [DependencyProperty]
        public Visibility GuideLinesVisibility { get; set; }
        #endregion

        #region 鼠标X坐标 —— int? MousePositionX
        /// <summary>
        /// 鼠标X坐标
        /// </summary>
        [DependencyProperty]
        public int? MousePositionX { get; set; }
        #endregion

        #region 鼠标Y坐标 —— int? MousePositionY
        /// <summary>
        /// 鼠标Y坐标
        /// </summary>
        [DependencyProperty]
        public int? MousePositionY { get; set; }
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
            this.BorderColor = Colors.Red;
            this.BorderThickness = 2;
            this.ShowGuideLines = true;
            this.GuideLinesVisibility = Visibility.Visible;
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— void Load(string imagePath, BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public void Load(string imagePath, BitmapSource bitmapSource)
        {
            this.ImagePath = imagePath;
            this.BitmapSource = bitmapSource;
            this.Image = bitmapSource.ToMat();
        }
        #endregion


        //Actions

        #region 复制形状 —— void CopyShape()
        /// <summary>
        /// 复制形状
        /// </summary>
        public void CopyShape()
        {
            if (this.SelectedShapeL != null)
            {
                Clipboard.SetText(this.SelectedShapeL.Text);
                base.ToastSuccess("已复制剪贴板！");
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
                this.Shapes.Remove(shape);
            }
        }
        #endregion

        #region 绘制形状 —— async void DrawShapes()
        /// <summary>
        /// 绘制形状
        /// </summary>
        public async void DrawShapes()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            Scalar borderColor = new Scalar(this.BorderBrush.Color.B, this.BorderBrush.Color.G, this.BorderBrush.Color.R);
            using Mat image = this.BitmapSource.ToMat();
            foreach (ShapeL shapeL in this.ShapeLs)
            {
                if (shapeL is PointL pointL)
                {
                    int radius = (int)Math.Ceiling(PointVisual2D.DefaultThickness);
                    int thickness = (int)Math.Ceiling(PointVisual2D.DefaultStrokeThickness);
                    await Task.Run(() => image.Circle(pointL.X, pointL.Y, radius, borderColor, thickness));       //空心圆
                    await Task.Run(() => image.Circle(pointL.X, pointL.Y, radius - thickness, Scalar.Black, -1)); //实心圆
                }
                if (shapeL is LineL lineL)
                {
                    await Task.Run(() => image.Line(lineL.A.X, lineL.A.Y, lineL.B.X, lineL.B.Y, borderColor, this.BorderThickness));
                }
                if (shapeL is RectangleL rectangleL)
                {
                    Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                    await Task.Run(() => image.Rectangle(rect, borderColor, this.BorderThickness));
                }
                if (shapeL is RotatedRectangleL rotatedRectangleL)
                {
                    Point[] contour = new Point[4];
                    contour[0] = new Point(rotatedRectangleL.TopLeft.X, rotatedRectangleL.TopLeft.Y);
                    contour[1] = new Point(rotatedRectangleL.TopRight.X, rotatedRectangleL.TopRight.Y);
                    contour[2] = new Point(rotatedRectangleL.BottomRight.X, rotatedRectangleL.BottomRight.Y);
                    contour[3] = new Point(rotatedRectangleL.BottomLeft.X, rotatedRectangleL.BottomLeft.Y);
                    await Task.Run(() => image.DrawContours(new[] { contour }, -1, borderColor, this.BorderThickness));
                }
                if (shapeL is CircleL circleL)
                {
                    await Task.Run(() => image.Circle(circleL.X, circleL.Y, circleL.Radius, borderColor, this.BorderThickness));
                }
                if (shapeL is EllipseL ellipseL)
                {
                    Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                    Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                    RotatedRect rect = new RotatedRect(center, size, 0);
                    await Task.Run(() => image.Ellipse(rect, borderColor, this.BorderThickness));
                }
                if (shapeL is PolygonL polygonL)
                {
                    Point[] contour = new Point[polygonL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL point = polygonL.Points.ElementAt(index);
                        contour[index] = new Point(point.X, point.Y);
                    }
                    await Task.Run(() => image.DrawContours(new[] { contour }, -1, borderColor, this.BorderThickness));
                }
                if (shapeL is PolylineL polylineL)
                {
                    Point[] contour = new Point[polylineL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL point = polylineL.Points.ElementAt(index);
                        contour[index] = new Point(point.X, point.Y);
                    }
                    await Task.Run(() => image.Polylines(new[] { contour }, false, borderColor, this.BorderThickness));
                }
            }
            this.BitmapSource = image.ToBitmapSource();

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

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Shapes.Any())
            {
                MessageBox.Show("形状不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
            await this.DrawMask(mask, this.ShapeLs);

            this.BitmapSource = mask.ToBitmapSource();

            this.Idle();
        }
        #endregion

        #region 应用掩膜 —— async void ApplyMask()
        /// <summary>
        /// 应用掩膜
        /// </summary>
        public async void ApplyMask()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.Shapes.Any())
            {
                MessageBox.Show("形状不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
            await this.DrawMask(mask, this.ShapeLs);

            //提取有效区域
            using Mat result = new Mat();
            this.Image.CopyTo(result, mask);
            this.BitmapSource = result.ToBitmapSource();
        }
        #endregion

        #region 分割图像 —— async void SegmentImage()
        /// <summary>
        /// 分割图像
        /// </summary>
        public async void SegmentImage()
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog
            {
                Title = "请选择目标文件夹",
                IsFolderPicker = true
            };
            if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.Busy();

                const int thickness = -1;
                IList<Mat> results = new List<Mat>();
                foreach (ShapeL shapeL in this.ShapeLs)
                {
                    if (shapeL is RectangleL rectangleL)
                    {
                        //生成掩膜
                        using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
                        Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                        await Task.Run(() => mask.Rectangle(rect, Scalar.White, thickness));

                        //适用掩膜
                        using Mat canvas = new Mat();
                        this.Image.CopyTo(canvas, mask);

                        //提取有效区域
                        Mat result = canvas[rect];
                        results.Add(result);
                    }
                    if (shapeL is RotatedRectangleL rotatedRectangleL)
                    {
                        //生成掩膜
                        using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
                        Point[] contour = new Point[4];
                        contour[0] = new Point(rotatedRectangleL.TopLeft.X, rotatedRectangleL.TopLeft.Y);
                        contour[1] = new Point(rotatedRectangleL.TopRight.X, rotatedRectangleL.TopRight.Y);
                        contour[2] = new Point(rotatedRectangleL.BottomRight.X, rotatedRectangleL.BottomRight.Y);
                        contour[3] = new Point(rotatedRectangleL.BottomLeft.X, rotatedRectangleL.BottomLeft.Y);
                        await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));

                        //适用掩膜
                        using Mat canvas = new Mat();
                        this.Image.CopyTo(canvas, mask);

                        //提取有效区域
                        Rect boundingRect = Cv2.BoundingRect(contour);
                        Mat result = canvas[boundingRect];
                        results.Add(result);
                    }
                    if (shapeL is CircleL circleL)
                    {
                        //生成掩膜
                        using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
                        await Task.Run(() => mask.Circle(circleL.X, circleL.Y, circleL.Radius, Scalar.White, thickness));

                        //适用掩膜
                        using Mat canvas = new Mat();
                        this.Image.CopyTo(canvas, mask);

                        //提取有效区域
                        int x = circleL.X - circleL.Radius;
                        int y = circleL.Y - circleL.Radius;
                        int sideSize = circleL.Radius * 2;
                        Rect boundingRect = new Rect(x, y, sideSize, sideSize);
                        Mat result = canvas[boundingRect];
                        results.Add(result);
                    }
                    if (shapeL is EllipseL ellipseL)
                    {
                        //生成掩膜
                        using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
                        Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                        Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                        RotatedRect rect = new RotatedRect(center, size, 0);
                        await Task.Run(() => mask.Ellipse(rect, Scalar.White, thickness));

                        //适用掩膜
                        using Mat canvas = new Mat();
                        this.Image.CopyTo(canvas, mask);

                        //提取有效区域
                        int x = ellipseL.X - ellipseL.RadiusX;
                        int y = ellipseL.Y - ellipseL.RadiusY;
                        int width = ellipseL.RadiusX * 2;
                        int height = ellipseL.RadiusY * 2;
                        Rect boundingRect = new Rect(x, y, width, height);
                        Mat result = canvas[boundingRect];
                        results.Add(result);
                    }
                    if (shapeL is PolygonL polygonL)
                    {
                        //生成掩膜
                        using Mat mask = Mat.Zeros(this.Image.Size(), MatType.CV_8UC1);
                        Point[] contour = new Point[polygonL.Points.Count];
                        for (int index = 0; index < polygonL.Points.Count; index++)
                        {
                            PointL pointL = polygonL.Points.ElementAt(index);
                            contour[index] = new Point(pointL.X, pointL.Y);
                        }
                        await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));

                        //适用掩膜
                        using Mat canvas = new Mat();
                        this.Image.CopyTo(canvas, mask);

                        //提取有效区域
                        Rect boundingRect = Cv2.BoundingRect(contour);
                        Mat result = canvas[boundingRect];
                        results.Add(result);
                    }
                }

                string imageName = Path.GetFileNameWithoutExtension(this.ImagePath);
                string imageExtension = Path.GetExtension(this.ImagePath);
                for (int index = 0; index < results.Count; index++)
                {
                    string imagePartPath = $@"{folderDialog.FileName}\{imageName}-{index + 1}{imageExtension}";

                    using Mat result = results[index];
                    await Task.Run(() => result.SaveImage(imagePartPath));
                }

                this.Idle();
                this.ToastSuccess("已保存！");
            }
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

        #region 画布鼠标移动事件 —— void OnCanvasMouseMove(CanvasEx canvas)
        /// <summary>
        /// 画布鼠标移动事件
        /// </summary>
        public void OnCanvasMouseMove(CanvasEx canvas)
        {
            System.Windows.Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
            this.MousePositionX = (int)Math.Ceiling(rectifiedPosition.X);
            this.MousePositionY = (int)Math.Ceiling(rectifiedPosition.Y);
        }
        #endregion


        //Private

        #region 绘制掩膜 —— async Task DrawMask(Mat mask, IList<ShapeL> shapeLs)
        /// <summary>
        /// 绘制掩膜
        /// </summary>
        /// <param name="mask">掩膜</param>
        /// <param name="shapeLs">形状列表</param>
        private async Task DrawMask(Mat mask, IList<ShapeL> shapeLs)
        {
            const int thickness = -1;
            foreach (ShapeL shapeL in shapeLs)
            {
                if (shapeL is RectangleL rectangleL)
                {
                    Rect rect = new Rect(rectangleL.X, rectangleL.Y, rectangleL.Width, rectangleL.Height);
                    await Task.Run(() => mask.Rectangle(rect, Scalar.White, thickness));
                }
                if (shapeL is RotatedRectangleL rotatedRectangleL)
                {
                    Point[] contour = new Point[4];
                    contour[0] = new Point(rotatedRectangleL.TopLeft.X, rotatedRectangleL.TopLeft.Y);
                    contour[1] = new Point(rotatedRectangleL.TopRight.X, rotatedRectangleL.TopRight.Y);
                    contour[2] = new Point(rotatedRectangleL.BottomRight.X, rotatedRectangleL.BottomRight.Y);
                    contour[3] = new Point(rotatedRectangleL.BottomLeft.X, rotatedRectangleL.BottomLeft.Y);
                    await Task.Run(() => mask.DrawContours(new[] { contour }, -1, Scalar.White, thickness));
                }
                if (shapeL is CircleL circleL)
                {
                    await Task.Run(() => mask.Circle(circleL.X, circleL.Y, circleL.Radius, Scalar.White, thickness));
                }
                if (shapeL is EllipseL ellipseL)
                {
                    Point2f center = new Point2f(ellipseL.X, ellipseL.Y);
                    Size2f size = new Size2f(ellipseL.RadiusX * 2, ellipseL.RadiusY * 2);
                    RotatedRect rotatedRect = new RotatedRect(center, size, 0);
                    await Task.Run(() => mask.Ellipse(rotatedRect, Scalar.White, thickness));
                }
                if (shapeL is PolygonL polygonL)
                {
                    Point[] contour = new Point[polygonL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL pointL = polygonL.Points.ElementAt(index);
                        contour[index] = new Point(pointL.X, pointL.Y);
                    }
                    await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));
                }
                if (shapeL is PolylineL polylineL)
                {
                    Point[] contour = new Point[polylineL.Points.Count];
                    for (int index = 0; index < contour.Length; index++)
                    {
                        PointL pointL = polylineL.Points.ElementAt(index);
                        contour[index] = new Point(pointL.X, pointL.Y);
                    }
                    await Task.Run(() => mask.DrawContours(new[] { contour }, 0, Scalar.White, thickness));
                }
            }
        }
        #endregion

        #endregion
    }
}
