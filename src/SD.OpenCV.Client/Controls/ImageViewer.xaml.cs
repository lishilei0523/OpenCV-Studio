﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SD.OpenCV.Client.Controls
{
    /// <summary>
    /// 图片查看器
    /// </summary>
    public partial class ImageViewer : UserControl
    {
        #region # 字段及构造器

        /// <summary>
        /// 缩放系数
        /// </summary>
        private const float ScaleFactor = 1.1F;

        /// <summary>
        /// 顶点
        /// </summary>
        private Point _vertex;

        /// <summary>
        /// 是否正在移动
        /// </summary>
        private bool _moving;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ImageViewer()
        {
            //注册依赖属性
            ImageViewer.ImageSourceProperty = DependencyProperty.Register(nameof(ImageViewer.ImageSource), typeof(ImageSource), typeof(ImageViewer), new PropertyMetadata(null));
        }

        /// <summary>
        /// 构造器
        /// </summary>
        public ImageViewer()
        {
            this.InitializeComponent();
        }

        #endregion

        #region # 依赖属性

        #region 图片源 —— ImageSource ImageSource

        /// <summary>
        /// 图片源依赖属性
        /// </summary>
        public static DependencyProperty ImageSourceProperty;

        /// <summary>
        /// 图片源
        /// </summary>
        public ImageSource ImageSource
        {
            get { return (ImageSource)base.GetValue(ImageViewer.ImageSourceProperty); }
            set { base.SetValue(ImageViewer.ImageSourceProperty, value); }
        }

        #endregion

        #endregion

        #region # 事件处理程序

        #region ViewBox鼠标滚轮滚动事件 —— void OnViewBoxMouseWheel(object sender...
        /// <summary>
        /// ViewBox鼠标滚轮滚动事件
        /// </summary>
        private void OnViewBoxMouseWheel(object sender, MouseWheelEventArgs eventArgs)
        {
            Point position = eventArgs.GetPosition(this.Viewbox);
            MatrixTransform matrixTransform = (MatrixTransform)this.Viewbox.RenderTransform;
            Matrix matrix = matrixTransform.Matrix;

            if (eventArgs.Delta > 0)
            {
                matrix.ScaleAtPrepend(ImageViewer.ScaleFactor, ImageViewer.ScaleFactor, position.X, position.Y);
            }
            else
            {
                matrix.ScaleAtPrepend(1 / ImageViewer.ScaleFactor, 1 / ImageViewer.ScaleFactor, position.X, position.Y);
            }

            this.Viewbox.RenderTransform = new MatrixTransform(matrix);
        }
        #endregion

        #region ViewBox鼠标左键点击事件 —— void OnViewBoxMouseLeftButtonDown(object sender...
        /// <summary>
        /// ViewBox鼠标左键点击事件
        /// </summary>
        private void OnViewBoxMouseLeftButtonDown(object sender, MouseButtonEventArgs eventArgs)
        {
            #region # 验证

            if (this.Viewbox.IsMouseCaptured)
            {
                return;
            }

            #endregion

            this.Cursor = Cursors.Hand;
            this.Viewbox.CaptureMouse();
            this._vertex = eventArgs.MouseDevice.GetPosition(this.Frame);
            this._moving = true;
            eventArgs.Handled = true;
        }
        #endregion

        #region ViewBox鼠标左键松开事件 —— void OnViewBoxMouseLeftButtonUp(object sender...
        /// <summary>
        /// ViewBox鼠标左键松开事件
        /// </summary>
        private void OnViewBoxMouseLeftButtonUp(object sender, MouseButtonEventArgs eventArgs)
        {
            this.Cursor = Cursors.Arrow;
            this.Viewbox.ReleaseMouseCapture();
            this._moving = false;
        }
        #endregion

        #region ViewBox鼠标移动事件 —— void OnViewBoxMouseMove(object sender...
        /// <summary>
        /// ViewBox鼠标移动事件
        /// </summary>
        private void OnViewBoxMouseMove(object sender, MouseEventArgs eventArgs)
        {
            #region # 验证

            if (!this.Viewbox.IsMouseCaptured || !this._moving)
            {
                return;
            }

            #endregion

            Point position = eventArgs.MouseDevice.GetPosition(this.Frame);
            Matrix matrix = this.Viewbox.RenderTransform.Value;

            //计算偏移量 
            matrix.OffsetX = matrix.OffsetX + position.X - this._vertex.X;
            matrix.OffsetY = matrix.OffsetY + position.Y - this._vertex.Y;

            //给鼠标起点和图片临时位置重新赋值
            this._vertex = position;
            this.Viewbox.RenderTransform = new MatrixTransform(matrix);
        }
        #endregion 

        #endregion
    }
}