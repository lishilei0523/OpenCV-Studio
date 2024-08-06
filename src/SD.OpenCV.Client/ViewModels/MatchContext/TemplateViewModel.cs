using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Visual2Ds;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rect = OpenCvSharp.Rect;
using Size = System.Windows.Size;

namespace SD.OpenCV.Client.ViewModels.MatchContext
{
    /// <summary>
    /// 模板匹配视图模型
    /// </summary>
    public class TemplateViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public TemplateViewModel(IWindowManager windowManager)
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

        #region 参考矩形 —— Rectangle SourceRectangle
        /// <summary>
        /// 参考矩形
        /// </summary>
        [DependencyProperty]
        public Rectangle SourceRectangle { get; set; }
        #endregion

        #region 参考矩形数据 —— RectangleL SourceRectangleL
        /// <summary>
        /// 参考矩形数据
        /// </summary>
        [DependencyProperty]
        public RectangleL SourceRectangleL { get; set; }
        #endregion

        #region 目标矩形 —— RectangleVisual2D TargetRectangle
        /// <summary>
        /// 目标矩形
        /// </summary>
        [DependencyProperty]
        public RectangleVisual2D TargetRectangle { get; set; }
        #endregion

        #region 最小分值 —— double? MinScoreValue
        /// <summary>
        /// 最小分值
        /// </summary>
        [DependencyProperty]
        public double? MinScoreValue { get; set; }
        #endregion

        #region 最大分值 —— double? MaxScoreValue
        /// <summary>
        /// 最大分值
        /// </summary>
        [DependencyProperty]
        public double? MaxScoreValue { get; set; }
        #endregion

        #region 匹配模式 —— TemplateMatchModes MatchMode
        /// <summary>
        /// 匹配模式
        /// </summary>
        [DependencyProperty]
        public TemplateMatchModes MatchMode { get; set; }
        #endregion

        #region 匹配模式字典 —— IDictionary<string, string> MatchModes
        /// <summary>
        /// 匹配模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> MatchModes { get; set; }
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
            this.TargetRectangle = new RectangleVisual2D
            {
                Location = new Point(0, 0),
                Size = new Size(0, 0),
                Visibility = Visibility.Collapsed
            };
            this.MatchMode = TemplateMatchModes.CCorrNormed;
            this.MatchModes = typeof(TemplateMatchModes).GetEnumMembers();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 打开参考图像 —— async void OpenSourceImage()
        /// <summary>
        /// 打开参考图像
        /// </summary>
        public async void OpenSourceImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                this.SourceImage = image.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 打开目标图像 —— async void OpenTargetImage()
        /// <summary>
        /// 打开目标图像
        /// </summary>
        public async void OpenTargetImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                this.Busy();

                using Mat image = await Task.Run(() => Cv2.ImRead(openFileDialog.FileName));
                this.TargetImage = image.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 执行匹配 —— async void ExecuteMatch()
        /// <summary>
        /// 执行匹配
        /// </summary>
        public async void ExecuteMatch()
        {
            #region # 验证

            if (this.SourceImage == null)
            {
                MessageBox.Show("参考图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.TargetImage == null)
            {
                MessageBox.Show("目标图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SourceRectangleL == null)
            {
                MessageBox.Show("模板未绘制！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            //截取模板
            using Mat sourceImage = this.SourceImage.ToMat();
            using Mat targetImage = this.TargetImage.ToMat();
            Rect rect = new Rect(this.SourceRectangleL.X, this.SourceRectangleL.Y, this.SourceRectangleL.Width, this.SourceRectangleL.Height);
            using Mat template = sourceImage[rect];

            //执行匹配
            using Mat result = new Mat();
            double minValue = 0;
            double maxValue = 0;
            OpenCvSharp.Point minLocation = new OpenCvSharp.Point();
            OpenCvSharp.Point maxLocation = new OpenCvSharp.Point();
            await Task.Run(() => Cv2.MatchTemplate(targetImage, template, result, this.MatchMode));
            await Task.Run(() => Cv2.MinMaxLoc(result, out minValue, out maxValue, out minLocation, out maxLocation));

            //执行结果
            this.MinScoreValue = minValue;
            this.MaxScoreValue = maxValue;
            this.TargetRectangle.Visibility = Visibility.Visible;
            this.TargetRectangle.Location = new Point(maxLocation.X, maxLocation.Y);
            this.TargetRectangle.Size = new Size(this.SourceRectangleL.Width, this.SourceRectangleL.Height);

            this.Idle();
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
            this.SourceRectangleL = new RectangleL(x, y, width, height);
        }
        #endregion

        #region 绘制矩形 —— void DrawRectangle(CanvasEx canvas)
        /// <summary>
        /// 绘制矩形
        /// </summary>
        private void DrawRectangle(CanvasEx canvas)
        {
            if (this.SourceRectangle == null)
            {
                this.SourceRectangle = new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Transparent),
                    Stroke = new SolidColorBrush(Colors.Red),
                    StrokeThickness = 2
                };
                canvas.Children.Add(this.SourceRectangle);
            }

            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            Point rectifiedPosition = canvas.RectifiedMousePosition!.Value;
            int width = (int)Math.Round(Math.Abs(rectifiedPosition.X - rectifiedVertex.X));
            int height = (int)Math.Round(Math.Abs(rectifiedPosition.Y - rectifiedVertex.Y));
            this.SourceRectangle.Width = width;
            this.SourceRectangle.Height = height;
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.SourceRectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this.SourceRectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X > rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.SourceRectangle, rectifiedVertex.X * canvas.ScaledRatio);
                Canvas.SetTop(this.SourceRectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y > rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.SourceRectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this.SourceRectangle, rectifiedVertex.Y * canvas.ScaledRatio);
            }
            if (rectifiedPosition.X < rectifiedVertex.X && rectifiedPosition.Y < rectifiedVertex.Y)
            {
                Canvas.SetLeft(this.SourceRectangle, rectifiedPosition.X * canvas.ScaledRatio);
                Canvas.SetTop(this.SourceRectangle, rectifiedPosition.Y * canvas.ScaledRatio);
            }
            this.SourceRectangle.RenderTransform = canvas.MatrixTransform;

            //重建矩形
            double leftMargin = canvas.GetRectifiedLeft(this.SourceRectangle);
            double topMargin = canvas.GetRectifiedTop(this.SourceRectangle);
            this.RebuildRectangle(this.SourceRectangle, leftMargin, topMargin);
        }
        #endregion

        #endregion
    }
}
