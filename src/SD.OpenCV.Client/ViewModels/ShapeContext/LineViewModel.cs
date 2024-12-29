using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Extensions;
using SD.Infrastructure.WPF.Models;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SD.OpenCV.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 霍夫线查找视图模型
    /// </summary>
    public class LineViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 线段颜色列表
        /// </summary>
        private static readonly Color[] _LineColors =
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
        public LineViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 距离精度 —— double Rho
        /// <summary>
        /// 距离精度
        /// </summary>
        [DependencyProperty]
        public double Rho { get; set; }
        #endregion

        #region 角度精度 —— double Theta
        /// <summary>
        /// 角度精度
        /// </summary>
        [DependencyProperty]
        public double Theta { get; set; }
        #endregion

        #region 阈值 —— int Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public int Threshold { get; set; }
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
            this.Rho = 1;
            this.Theta = Math.PI / 180;
            this.Threshold = 100;
            this.Shapes = new ObservableCollection<Shape>();
            this.ShapeLs = new ObservableCollection<ShapeL>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 查找线段 —— async void HoughFindLines()
        /// <summary>
        /// 查找线段
        /// </summary>
        public async void HoughFindLines()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //清空线段
            this.Shapes.Clear();
            this.ShapeLs.Clear();

            //查找线段
            using Mat image = this.BitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC1
                ? image
                : image.CvtColor(ColorConversionCodes.BGR2GRAY);
            LineSegmentPoint[] lineSegments = await Task.Run(() => Cv2.HoughLinesP(grayImage, this.Rho, this.Theta, this.Threshold));

            //绘制线段
            for (int index = 0; index < lineSegments.Length; index++)
            {
                LineSegmentPoint lineSegment = lineSegments[index];
                LineL lineL = new LineL(new PointL(lineSegment.P1.X, lineSegment.P1.Y), new PointL(lineSegment.P2.X, lineSegment.P2.Y));
                Line line = new Line
                {
                    X1 = lineSegment.P1.X,
                    Y1 = lineSegment.P1.Y,
                    X2 = lineSegment.P2.X,
                    Y2 = lineSegment.P2.Y,
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(_LineColors[index % 5]),
                    StrokeThickness = 2
                };

                line.Tag = lineL;
                lineL.Tag = line;
                this.Shapes.Add(line);
                this.ShapeLs.Add(lineL);
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
            foreach (Line line in this.Shapes.OfType<Line>())
            {
                LineL lineL = (LineL)line.Tag;
                int thickness = (int)Math.Ceiling(line.StrokeThickness);
                SolidColorBrush borderBrush = (SolidColorBrush)line.Stroke;
                Scalar borderColor = new Scalar(borderBrush.Color.B, borderBrush.Color.G, borderBrush.Color.R);
                await Task.Run(() => colorImage.Line(lineL.A.X, lineL.A.Y, lineL.B.X, lineL.B.Y, borderColor, thickness));
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
