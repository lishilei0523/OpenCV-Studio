using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Enums;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.IOC.Core.Mediators;
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
using Point = System.Windows.Point;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 透视变换视图模型
    /// </summary>
    public class PerspectiveViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public PerspectiveViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

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

        #region 参考操作模式 —— CanvasMode SourceCanvasMode
        /// <summary>
        /// 参考操作模式
        /// </summary>
        [DependencyProperty]
        public CanvasMode SourceCanvasMode { get; set; }
        #endregion

        #region 目标操作模式 —— CanvasMode TargetCanvasMode
        /// <summary>
        /// 目标操作模式
        /// </summary>
        [DependencyProperty]
        public CanvasMode TargetCanvasMode { get; set; }
        #endregion

        #region 参考选中拖拽 —— bool SourceDragChecked
        /// <summary>
        /// 参考选中拖拽
        /// </summary>
        [DependencyProperty]
        public bool SourceDragChecked { get; set; }
        #endregion

        #region 参考选中点 —— bool SourcePointChecked
        /// <summary>
        /// 参考选中点
        /// </summary>
        [DependencyProperty]
        public bool SourcePointChecked { get; set; }
        #endregion

        #region 目标选中拖拽 —— bool TargetDragChecked
        /// <summary>
        /// 目标选中拖拽
        /// </summary>
        [DependencyProperty]
        public bool TargetDragChecked { get; set; }
        #endregion

        #region 目标选中点 —— bool TargetPointChecked
        /// <summary>
        /// 目标选中点
        /// </summary>
        [DependencyProperty]
        public bool TargetPointChecked { get; set; }
        #endregion

        #region 透视变换矩阵 —— Mat PerspectiveMatrix
        /// <summary>
        /// 透视变换矩阵
        /// </summary>
        [DependencyProperty]
        public Mat PerspectiveMatrix { get; set; }
        #endregion

        #region 参考点集 —— ObservableCollection<PointVisual2D> SourcePoints
        /// <summary>
        /// 参考点集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<PointVisual2D> SourcePoints { get; set; }
        #endregion

        #region 参考点数据集 —— ObservableCollection<PointL> SourcePointLs
        /// <summary>
        /// 参考点数据集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<PointL> SourcePointLs { get; set; }
        #endregion

        #region 目标点集 —— ObservableCollection<PointVisual2D> TargetPoints
        /// <summary>
        /// 目标点集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<PointVisual2D> TargetPoints { get; set; }
        #endregion

        #region 目标点数据集 —— ObservableCollection<PointL> TargetPointLs
        /// <summary>
        /// 目标点数据集
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<PointL> TargetPointLs { get; set; }
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
            this.SourcePoints = new ObservableCollection<PointVisual2D>();
            this.SourcePointLs = new ObservableCollection<PointL>();
            this.TargetPoints = new ObservableCollection<PointVisual2D>();
            this.TargetPointLs = new ObservableCollection<PointL>();

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

        #region 透视变换 —— async void PerspectiveTransform()
        /// <summary>
        /// 透视变换
        /// </summary>
        public async void PerspectiveTransform()
        {
            #region # 验证

            if (!this.SourcePointLs.Any())
            {
                MessageBox.Show("参考点不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.TargetPointLs.Any())
            {
                MessageBox.Show("目标点不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SourcePointLs.Count != this.TargetPointLs.Count)
            {
                MessageBox.Show("参考点与目标点数量不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IEnumerable<Point2f> sourcePoints = this.SourcePointLs.Select(point => new Point2f(point.X, point.Y));
            IEnumerable<Point2f> targetPoints = this.TargetPointLs.Select(point => new Point2f(point.X, point.Y));
            this.PerspectiveMatrix = await Task.Run(() => Cv2.GetPerspectiveTransform(targetPoints, sourcePoints));

            using Mat targetImage = this.TargetImage.ToMat();
            using Mat resultImage = new Mat();
            await Task.Run(() => Cv2.WarpPerspective(targetImage, resultImage, this.PerspectiveMatrix, targetImage.Size()));

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(resultImage.ToBitmapSource());
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion


        //Events

        #region 参考拖拽点击事件 —— void OnSourceDragClick()
        /// <summary>
        /// 参考拖拽点击事件
        /// </summary>
        public void OnSourceDragClick()
        {
            if (this.SourceDragChecked)
            {
                this.SourceCanvasMode = CanvasMode.Drag;
                this.SourcePointChecked = false;
            }
        }
        #endregion

        #region 参考点点击事件 —— void OnSourcePointClick()
        /// <summary>
        /// 参考点点击事件
        /// </summary>
        public void OnSourcePointClick()
        {
            if (this.SourcePointChecked)
            {
                this.SourceCanvasMode = CanvasMode.Draw;
                this.SourceDragChecked = false;
            }
        }
        #endregion

        #region 目标拖拽点击事件 —— void OnTargetDragClick()
        /// <summary>
        /// 目标拖拽点击事件
        /// </summary>
        public void OnTargetDragClick()
        {
            if (this.TargetDragChecked)
            {
                this.TargetCanvasMode = CanvasMode.Drag;
                this.TargetPointChecked = false;
            }
        }
        #endregion

        #region 目标点点击事件 —— void OnTargetPointClick()
        /// <summary>
        /// 目标点点击事件
        /// </summary>
        public void OnTargetPointClick()
        {
            if (this.TargetPointChecked)
            {
                this.TargetCanvasMode = CanvasMode.Draw;
                this.TargetDragChecked = false;
            }
        }
        #endregion

        #region 参考拖拽元素事件 —— void OnSourceDragElement(CanvasEx canvas)
        /// <summary>
        /// 参考拖拽元素事件
        /// </summary>
        public void OnSourceDragElement(CanvasEx canvas)
        {
            if (this.SourceImage == null)
            {
                return;
            }

            double leftMargin = canvas.GetRectifiedLeft(canvas.SelectedVisual);
            double topMargin = canvas.GetRectifiedTop(canvas.SelectedVisual);

            if (canvas.SelectedVisual is PointVisual2D point)
            {
                this.RebuildSourcePoint(point, leftMargin, topMargin);
            }
        }
        #endregion

        #region 参考绘制开始事件 —— void OnSourceDraw(CanvasEx canvas)
        /// <summary>
        /// 参考绘制开始事件
        /// </summary>
        public void OnSourceDraw(CanvasEx canvas)
        {
            if (this.SourceImage == null)
            {
                return;
            }
            if (this.SourcePointChecked)
            {
                this.DrawSourcePoint(canvas);
            }
        }
        #endregion

        #region 目标拖拽元素事件 —— void OnTargetDragElement(CanvasEx canvas)
        /// <summary>
        /// 目标拖拽元素事件
        /// </summary>
        public void OnTargetDragElement(CanvasEx canvas)
        {
            if (this.TargetImage == null)
            {
                return;
            }

            double leftMargin = canvas.GetRectifiedLeft(canvas.SelectedVisual);
            double topMargin = canvas.GetRectifiedTop(canvas.SelectedVisual);
            if (canvas.SelectedVisual is PointVisual2D point)
            {
                this.RebuildTargetPoint(point, leftMargin, topMargin);
            }
        }
        #endregion

        #region 目标绘制开始事件 —— void OnTargetDraw(CanvasEx canvas)
        /// <summary>
        /// 目标绘制开始事件
        /// </summary>
        public void OnTargetDraw(CanvasEx canvas)
        {
            if (this.TargetImage == null)
            {
                return;
            }

            if (this.TargetPointChecked)
            {
                this.DrawTargetPoint(canvas);
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
                this.PerspectiveMatrix?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion


        //Private

        #region 参考重建点 —— void RebuildSourcePoint(PointVisual2D point, double leftMargin, double topMargin)
        /// <summary>
        /// 参考重建点
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildSourcePoint(PointVisual2D point, double leftMargin, double topMargin)
        {
            PointL pointL = (PointL)point.Tag;
            int index = this.SourcePointLs.IndexOf(pointL);
            if (index != -1)
            {
                this.SourcePointLs.Remove(pointL);

                int x = (int)Math.Ceiling(point.X + leftMargin);
                int y = (int)Math.Ceiling(point.Y + topMargin);
                PointL newPointL = new PointL(x, y);

                point.Tag = newPointL;
                this.SourcePointLs.Insert(index, newPointL);
            }
        }
        #endregion

        #region 参考绘制点 —— void DrawSourcePoint(CanvasEx canvas)
        /// <summary>
        /// 参考绘制点
        /// </summary>
        private void DrawSourcePoint(CanvasEx canvas)
        {
            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            int x = (int)Math.Ceiling(rectifiedVertex.X);
            int y = (int)Math.Ceiling(rectifiedVertex.Y);

            PointL pointL = new PointL(x, y);
            PointVisual2D point = new PointVisual2D
            {
                X = rectifiedVertex.X,
                Y = rectifiedVertex.Y,
                Fill = new SolidColorBrush(Colors.Black),
                Stroke = new SolidColorBrush(Colors.Red),
                RenderTransform = canvas.MatrixTransform,
                Tag = pointL
            };
            canvas.Children.Add(point);

            pointL.Tag = point;
            this.SourcePointLs.Add(pointL);
            this.SourcePoints.Add(point);
        }
        #endregion

        #region 目标重建点 —— void RebuildTargetPoint(PointVisual2D point, double leftMargin, double topMargin)
        /// <summary>
        /// 目标重建点
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="leftMargin">左边距</param>
        /// <param name="topMargin">上边距</param>
        private void RebuildTargetPoint(PointVisual2D point, double leftMargin, double topMargin)
        {
            PointL pointL = (PointL)point.Tag;
            int index = this.TargetPointLs.IndexOf(pointL);
            if (index != -1)
            {
                this.TargetPointLs.Remove(pointL);

                int x = (int)Math.Ceiling(point.X + leftMargin);
                int y = (int)Math.Ceiling(point.Y + topMargin);
                PointL newPointL = new PointL(x, y);

                point.Tag = newPointL;
                this.TargetPointLs.Insert(index, newPointL);
            }
        }
        #endregion

        #region 目标绘制点 —— void DrawTargetPoint(CanvasEx canvas)
        /// <summary>
        /// 目标绘制点
        /// </summary>
        private void DrawTargetPoint(CanvasEx canvas)
        {
            Point rectifiedVertex = canvas.RectifiedStartPosition!.Value;
            int x = (int)Math.Ceiling(rectifiedVertex.X);
            int y = (int)Math.Ceiling(rectifiedVertex.Y);

            PointL pointL = new PointL(x, y);
            PointVisual2D point = new PointVisual2D
            {
                X = rectifiedVertex.X,
                Y = rectifiedVertex.Y,
                Fill = new SolidColorBrush(Colors.Black),
                Stroke = new SolidColorBrush(Colors.Red),
                RenderTransform = canvas.MatrixTransform,
                Tag = pointL
            };
            canvas.Children.Add(point);

            pointL.Tag = point;
            this.TargetPointLs.Add(pointL);
            this.TargetPoints.Add(point);
        }
        #endregion

        #endregion
    }
}
