﻿using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.OpenCV.Primitives.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 缩放视图模型
    /// </summary>
    public class ScaleViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ScaleViewModel()
        {
            //默认值
            this.SelectedScaleMode = ScaleMode.Absolute;
            this.Width = 1024;
            this.Height = 768;
            this.ScaleRatio = 0.5f;
            this.SideSize = 512;
            this.AbsoluteVisibility = Visibility.Visible;
            this.RelativeVisibility = Visibility.Collapsed;
            this.AdaptiveVisibility = Visibility.Collapsed;
        }

        #endregion

        #region # 属性

        #region 缩放模式 —— ScaleMode? SelectedScaleMode
        /// <summary>
        /// 缩放模式
        /// </summary>
        [DependencyProperty]
        public ScaleMode? SelectedScaleMode { get; set; }
        #endregion

        #region 目标宽度 —— int? Width
        /// <summary>
        /// 目标宽度
        /// </summary>
        [DependencyProperty]
        public int? Width { get; set; }
        #endregion

        #region 目标高度 —— int? Height
        /// <summary>
        /// 目标高度
        /// </summary>
        [DependencyProperty]
        public int? Height { get; set; }
        #endregion

        #region 缩放率 —— float? ScaleRatio
        /// <summary>
        /// 缩放率
        /// </summary>
        [DependencyProperty]
        public float? ScaleRatio { get; set; }
        #endregion

        #region 目标边长 —— int? SideSize
        /// <summary>
        /// 目标边长
        /// </summary>
        [DependencyProperty]
        public int? SideSize { get; set; }
        #endregion

        #region 绝对缩放可见性 —— Visibility AbsoluteVisibility
        /// <summary>
        /// 绝对缩放可见性
        /// </summary>
        [DependencyProperty]
        public Visibility AbsoluteVisibility { get; set; }
        #endregion

        #region 相对缩放可见性 —— Visibility RelativeVisibility
        /// <summary>
        /// 相对缩放可见性
        /// </summary>
        [DependencyProperty]
        public Visibility RelativeVisibility { get; set; }
        #endregion

        #region 适应缩放可见性 —— Visibility AdaptiveVisibility
        /// <summary>
        /// 适应缩放可见性
        /// </summary>
        [DependencyProperty]
        public Visibility AdaptiveVisibility { get; set; }
        #endregion

        #region 缩放模式字典 —— IDictionary<string, string> ScaleModes
        /// <summary>
        /// 缩放模式字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, string> ScaleModes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            this.ScaleModes = typeof(ScaleMode).GetEnumMembers();
            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 切换缩放模式 —— void SwitchScaleMode()
        /// <summary>
        /// 切换缩放模式
        /// </summary>
        public void SwitchScaleMode()
        {
            switch (this.SelectedScaleMode)
            {
                case ScaleMode.Absolute:
                    this.AbsoluteVisibility = Visibility.Visible;
                    this.RelativeVisibility = Visibility.Collapsed;
                    this.AdaptiveVisibility = Visibility.Collapsed;
                    break;
                case ScaleMode.Relative:
                    this.AbsoluteVisibility = Visibility.Collapsed;
                    this.RelativeVisibility = Visibility.Visible;
                    this.AdaptiveVisibility = Visibility.Collapsed;
                    break;
                case ScaleMode.Adaptive:
                    this.AbsoluteVisibility = Visibility.Collapsed;
                    this.RelativeVisibility = Visibility.Collapsed;
                    this.AdaptiveVisibility = Visibility.Visible;
                    break;
                case null:
                    this.AbsoluteVisibility = Visibility.Collapsed;
                    this.RelativeVisibility = Visibility.Collapsed;
                    this.AdaptiveVisibility = Visibility.Collapsed;
                    break;
                default:
                    throw new NotSupportedException();
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

            if (!this.SelectedScaleMode.HasValue)
            {
                MessageBox.Show("缩放模式不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SelectedScaleMode == ScaleMode.Absolute && !this.Width.HasValue)
            {
                MessageBox.Show("目标宽度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SelectedScaleMode == ScaleMode.Absolute && !this.Height.HasValue)
            {
                MessageBox.Show("目标高度不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SelectedScaleMode == ScaleMode.Relative && !this.ScaleRatio.HasValue)
            {
                MessageBox.Show("缩放率不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SelectedScaleMode == ScaleMode.Adaptive && !this.SideSize.HasValue)
            {
                MessageBox.Show("目标边长不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            await base.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
