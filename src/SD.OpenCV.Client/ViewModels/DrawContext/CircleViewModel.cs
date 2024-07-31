﻿using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.Infrastructure.WPF.CustomControls;
using SD.Infrastructure.WPF.Visual2Ds;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System;
using System.Collections.Generic;
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

        #region 圆形集 —— IList<CircleVisual2D> Circles
        /// <summary>
        /// 圆形集
        /// </summary>
        public IList<CircleVisual2D> Circles { get; set; }
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
            this.Circles = new List<CircleVisual2D>();

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

        #region 预览图像 —— async void PreviewImage()
        /// <summary>
        /// 预览图像
        /// </summary>
        public async void PreviewImage()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(this.BitmapSource);
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
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

        #region 鼠标左键按下事件 —— void OnMouseLeftDown(ScalableCanvas canvas...
        /// <summary>
        /// 鼠标左键按下事件
        /// </summary>
        public void OnMouseLeftDown(ScalableCanvas canvas, MouseButtonEventArgs eventArgs)
        {
            //设置光标
            Mouse.OverrideCursor = Cursors.Cross;

            this._center = eventArgs.GetPosition(canvas);

            eventArgs.Handled = true;
        }
        #endregion

        #region 鼠标移动事件 —— void OnMouseMove(ScalableCanvas canvas...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public void OnMouseMove(ScalableCanvas canvas, MouseEventArgs eventArgs)
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