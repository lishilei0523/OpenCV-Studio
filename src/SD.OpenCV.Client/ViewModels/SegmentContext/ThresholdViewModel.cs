﻿using Caliburn.Micro;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Client.ViewModels.SegmentContext
{
    /// <summary>
    /// 阈值分割视图模型
    /// </summary>
    public class ThresholdViewModel : PreviewViewModel
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ThresholdViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 阈值 —— double Threshold
        /// <summary>
        /// 阈值
        /// </summary>
        [DependencyProperty]
        public double Threshold { get; set; }
        #endregion

        #region 最大值 —— double MaxValue
        /// <summary>
        /// 最大值
        /// </summary>
        [DependencyProperty]
        public double MaxValue { get; set; }
        #endregion

        #region 阈值分割类型 —— ThresholdTypes ThresholdType
        /// <summary>
        /// 阈值分割类型
        /// </summary>
        [DependencyProperty]
        public ThresholdTypes ThresholdType { get; set; }
        #endregion

        #region 阈值分割类型字典 —— IDictionary<string, string> ThresholdTypes
        /// <summary>
        /// 阈值分割类型字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ThresholdTypes { get; set; }
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
            this.Threshold = 127;
            this.MaxValue = 255;
            this.ThresholdType = OpenCvSharp.ThresholdTypes.Binary;
            this.ThresholdTypes = typeof(ThresholdTypes).GetEnumMembers().Take(5).ToDictionary(x => x.Key, x => x.Value);

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 加载 —— override void Load(BitmapSource bitmapSource)
        /// <summary>
        /// 加载
        /// </summary>
        public override void Load(BitmapSource bitmapSource)
        {
            Mat image = bitmapSource.ToMat();
            if (image.Type() == MatType.CV_8UC3)
            {
                this.Image = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                image.Dispose();
            }
            else
            {
                this.Image = image;
            }
            this.BitmapSource = bitmapSource;
        }
        #endregion

        #region 滑动阈值 —— async void SlideThreshold()
        /// <summary>
        /// 滑动阈值
        /// </summary>
        public async void SlideThreshold()
        {
            #region # 验证

            if (this.BitmapSource == null)
            {
                MessageBox.Show("图像源不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            using Mat result = new Mat();
            await Task.Run(() => Cv2.Threshold(this.Image, result, this.Threshold, this.MaxValue, this.ThresholdType));
            this.BitmapSource = result.ToBitmapSource();
        }
        #endregion

        #endregion
    }
}
