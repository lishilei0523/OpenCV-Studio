using Caliburn.Micro;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Infrastructure.Shapes;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CommonContext;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Size = OpenCvSharp.Size;

namespace SD.OpenCV.Client.ViewModels.GeometryContext
{
    /// <summary>
    /// 仿射变换视图模型
    /// </summary>
    public class AffineViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public AffineViewModel(IWindowManager windowManager)
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

        #region 仿射变换矩阵 —— Mat AffineMatrix
        /// <summary>
        /// 仿射变换矩阵
        /// </summary>
        public Mat AffineMatrix { get; set; }
        #endregion

        #region 参考形状列表 —— ObservableCollection<Shape> SourceShapes
        /// <summary>
        /// 参考形状列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> SourceShapes { get; set; }
        #endregion

        #region 参考形状数据列表 —— ObservableCollection<ShapeL> SourceShapeLs
        /// <summary>
        /// 参考形状数据列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ShapeL> SourceShapeLs { get; set; }
        #endregion

        #region 目标形状列表 —— ObservableCollection<Shape> TargetShapes
        /// <summary>
        /// 目标形状列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Shape> TargetShapes { get; set; }
        #endregion

        #region 目标形状数据列表 —— ObservableCollection<ShapeL> TargetShapeLs
        /// <summary>
        /// 目标形状数据列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<ShapeL> TargetShapeLs { get; set; }
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
            this.SourceShapes = new ObservableCollection<Shape>();
            this.SourceShapeLs = new ObservableCollection<ShapeL>();
            this.TargetShapes = new ObservableCollection<Shape>();
            this.TargetShapeLs = new ObservableCollection<ShapeL>();

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
                Filter = "图片文件(*.jpg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
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
                Filter = "图片文件(*.jpg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
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

        #region 仿射变换 —— async void AffineTransform()
        /// <summary>
        /// 仿射变换
        /// </summary>
        public async void AffineTransform()
        {
            #region # 验证

            if (!this.SourceShapeLs.Any())
            {
                MessageBox.Show("参考点不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SourceShapeLs.Count < 3)
            {
                MessageBox.Show("参考点数量不可小于3！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!this.TargetShapeLs.Any())
            {
                MessageBox.Show("目标点不可为空！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.TargetShapeLs.Count < 3)
            {
                MessageBox.Show("目标点数量不可小于3！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.SourceShapeLs.Count != this.TargetShapeLs.Count)
            {
                MessageBox.Show("参考点与目标点数量不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            IEnumerable<Point2f> sourcePoints = this.SourceShapeLs.OfType<PointL>().Select(point => new Point2f(point.X, point.Y));
            IEnumerable<Point2f> targetPoints = this.TargetShapeLs.OfType<PointL>().Select(point => new Point2f(point.X, point.Y));
            this.AffineMatrix = await Task.Run(() => Cv2.GetAffineTransform(targetPoints, sourcePoints));

            using Mat targetImage = this.TargetImage.ToMat();
            using Mat resultImage = new Mat();
            Size size = new Size(this.SourceImage.Width, this.SourceImage.Height);
            await Task.Run(() => Cv2.WarpAffine(targetImage, resultImage, this.AffineMatrix, size));

            ImageCompareViewModel viewModel = ResolveMediator.Resolve<ImageCompareViewModel>();
            viewModel.Load(this.SourceImage, resultImage.ToBitmapSource(), "目标图像变换效果图");
            await this._windowManager.ShowWindowAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 重置 —— void Reset()
        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            this.AffineMatrix?.Dispose();
            this.AffineMatrix = null;
            this.SourceShapes.Clear();
            this.SourceShapeLs.Clear();
            this.TargetShapes.Clear();
            this.TargetShapeLs.Clear();
        }
        #endregion


        //Private

        #region 页面失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 页面失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                this.AffineMatrix?.Dispose();
            }
            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
