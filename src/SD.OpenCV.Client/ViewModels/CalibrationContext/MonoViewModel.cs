using Caliburn.Micro;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using SD.Common;
using SD.Infrastructure.WPF.Caliburn.Aspects;
using SD.Infrastructure.WPF.Caliburn.Base;
using SD.IOC.Core.Mediators;
using SD.OpenCV.Client.ViewModels.CommonContext;
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Size = OpenCvSharp.Size;

namespace SD.OpenCV.Client.ViewModels.CalibrationContext
{
    /// <summary>
    /// 单目标定视图模型
    /// </summary>
    public class MonoViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MonoViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 已选图像 —— BitmapSource SelectedBitmap
        /// <summary>
        /// 已选图像
        /// </summary>
        [DependencyProperty]
        public BitmapSource SelectedBitmap { get; set; }
        #endregion

        #region 已选图像 —— KeyValuePair<string, Mat>? SelectedImage
        /// <summary>
        /// 已选图像
        /// </summary>
        [DependencyProperty]
        public KeyValuePair<string, Mat>? SelectedImage { get; set; }
        #endregion

        #region 标定重投影误差 —— string CalibratedReprojectionError
        /// <summary>
        /// 标定重投影误差
        /// </summary>
        [DependencyProperty]
        public string CalibratedReprojectionError { get; set; }
        #endregion

        #region 重投影误差 —— string ReprojectionError
        /// <summary>
        /// 重投影误差
        /// </summary>
        [DependencyProperty]
        public string ReprojectionError { get; set; }
        #endregion

        #region 畸变向量 —— string DistortionVector
        /// <summary>
        /// 畸变向量
        /// </summary>
        [DependencyProperty]
        public string DistortionVector { get; set; }
        #endregion

        #region 内参矩阵 —— string IntrinsicMatrix
        /// <summary>
        /// 内参矩阵
        /// </summary>
        [DependencyProperty]
        public string IntrinsicMatrix { get; set; }
        #endregion

        #region 相机内参 —— CameraIntrinsics CameraIntrinsics
        /// <summary>
        /// 相机内参
        /// </summary>
        public CameraIntrinsics CameraIntrinsics { get; set; }
        #endregion

        #region 图像字典 —— IDictionary<string, Mat> Images
        /// <summary>
        /// 图像字典
        /// </summary>
        [DependencyProperty]
        public IDictionary<string, Mat> Images { get; set; }
        #endregion

        #region 标定参数视图模型 —— MonoParamViewModel ParamViewModel
        /// <summary>
        /// 标定参数视图模型
        /// </summary>
        [DependencyProperty]
        public MonoParamViewModel ParamViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 加载图像 —— async void LoadImages()
        /// <summary>
        /// 加载图像
        /// </summary>
        public async void LoadImages()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.jpg)|*.jpg|(*.png)|*.png|(*.bmp)|*.bmp",
                AddExtension = true,
                RestoreDirectory = true,
                Multiselect = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                #region # 验证

                if (!openFileDialog.FileNames.Any())
                {
                    MessageBox.Show("未选择图像！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.Busy();

                IDictionary<string, Mat> images = new Dictionary<string, Mat>();
                foreach (string filePath in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(filePath);
                    Mat image = await Task.Run(() => Cv2.ImRead(filePath));
                    images.Add(fileName, image);
                }
                this.Images = images;

                //默认预览第一张
                KeyValuePair<string, Mat> first = this.Images.First();
                this.SelectedImage = first;
                this.SelectedBitmap = first.Value.ToBitmapSource();

                this.Idle();
            }
        }
        #endregion

        #region 选择图像 —— async void SelectImage()
        /// <summary>
        /// 选择图像
        /// </summary>
        public async void SelectImage()
        {
            this.Busy();

            if (this.SelectedImage != null)
            {
                await this.OnUIThreadAsync(() =>
                {
                    BitmapSource bitmapSource = this.SelectedImage.Value.Value.ToBitmapSource();
                    this.SelectedBitmap = bitmapSource;

                    return Task.CompletedTask;
                });
            }

            this.Idle();
        }
        #endregion

        #region 预览图像 —— async void PreviewImage()
        /// <summary>
        /// 预览图像
        /// </summary>
        public async void PreviewImage()
        {
            #region # 验证

            if (this.SelectedBitmap == null)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            ImageViewModel viewModel = ResolveMediator.Resolve<ImageViewModel>();
            viewModel.Load(this.SelectedBitmap);
            await this._windowManager.ShowDialogAsync(viewModel);

            this.Idle();
        }
        #endregion

        #region 标定参数 —— async void SetParameters()
        /// <summary>
        /// 标定参数
        /// </summary>
        public async void SetParameters()
        {
            MonoParamViewModel viewModel = this.ParamViewModel ?? ResolveMediator.Resolve<MonoParamViewModel>();
            bool? result = await this._windowManager.ShowDialogAsync(viewModel);
            if (result == true)
            {
                this.ParamViewModel = viewModel;
            }
        }
        #endregion

        #region 绘制角点 —— async void DrawKeyPoints()
        /// <summary>
        /// 绘制角点
        /// </summary>
        public async void DrawKeyPoints()
        {
            #region # 验证

            if (!this.SelectedImage.HasValue)
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.ParamViewModel == null)
            {
                MessageBox.Show("标定参数未设置！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            using Mat image = this.SelectedImage.Value.Value.Clone();
            using Mat grayImage = new Mat();
            Cv2.CvtColor(image, grayImage, ColorConversionCodes.BGR2GRAY);
            PatternType patternType = this.ParamViewModel.SelectedPatternType!.Value;
            Size patternSize = new Size(this.ParamViewModel.RowPointsCount!.Value, this.ParamViewModel.ColumnPointsCount!.Value);
            int maxCount = this.ParamViewModel.MaxCount!.Value;
            double epsilon = this.ParamViewModel.Epsilon!.Value;

            bool success;
            ICollection<Point2f> cornerPoints = new List<Point2f>();
            if (patternType == PatternType.Chessboard)
            {
                success = await Task.Run(() => grayImage.GetOptimizedChessboardCorners(patternSize, maxCount, epsilon, out cornerPoints));
            }
            else if (patternType == PatternType.CirclesGrid)
            {
                success = await Task.Run(() => grayImage.GetOptimizedCirclesGridCorners(patternSize, maxCount, epsilon, out cornerPoints));
            }
            else
            {
                throw new NotSupportedException();
            }

            if (success)
            {
                Cv2.DrawChessboardCorners(image, patternSize, cornerPoints, true);
                this.SelectedBitmap = image.ToBitmapSource();
            }
            else
            {
                MessageBox.Show("未找到角点！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.Idle();
        }
        #endregion

        #region 执行标定 —— async void ExecuteCalibration()
        /// <summary>
        /// 执行标定
        /// </summary>
        public async void ExecuteCalibration()
        {
            #region # 验证

            if (this.Images == null || !this.Images.Any())
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.Images.Count < 6)
            {
                MessageBox.Show("图像数量不可小于6！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.ParamViewModel == null)
            {
                MessageBox.Show("标定参数未设置！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirmed = MessageBox.Show("确定要执行标定？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmed != MessageBoxResult.Yes)
            {
                return;
            }

            #endregion

            this.Busy();

            string cameraId = this.ParamViewModel.CameraId;
            PatternType patternType = this.ParamViewModel.SelectedPatternType!.Value;
            int patternSideSize = this.ParamViewModel.PatternSideSize!.Value;
            Size patternSize = new Size(this.ParamViewModel.RowPointsCount!.Value, this.ParamViewModel.ColumnPointsCount!.Value);
            Size imageSize = new Size(this.ParamViewModel.ImageWidth!.Value, this.ParamViewModel.ImageHeight!.Value);
            int maxCount = this.ParamViewModel.MaxCount!.Value;
            double epsilon = this.ParamViewModel.Epsilon!.Value;

            //转灰度图
            IDictionary<string, Mat> grayImages = new Dictionary<string, Mat>();
            foreach (KeyValuePair<string, Mat> kv in this.Images)
            {
                Mat grayImage = new Mat();
                Cv2.CvtColor(kv.Value, grayImage, ColorConversionCodes.BGR2GRAY);
                grayImages.Add(kv.Key, grayImage);
            }

            //标定内参
            this.CameraIntrinsics = await Task.Run(() => Calibrator.MonoCalibrate(cameraId, patternSideSize, patternSize, patternType, maxCount, epsilon, imageSize, grayImages, out IDictionary<string, Matrix<double>> extrinsicMatrices, out ICollection<string> failedImageKeys));
            this.CalibratedReprojectionError = this.CameraIntrinsics.CalibratedReprojectionError.ToString("F9");
            this.ReprojectionError = this.CameraIntrinsics.ReprojectionError.ToString("F9");
            this.DistortionVector = DenseVector.OfArray(this.CameraIntrinsics.DistortionVector).ToVectorString("F10");
            this.IntrinsicMatrix = DenseMatrix.OfArray(this.CameraIntrinsics.IntrinsicMatrix).ToMatrixString("F4");

            //释放资源
            foreach (Mat grayImage in grayImages.Values)
            {
                grayImage.Dispose();
            }

            this.Idle();
        }
        #endregion

        #region 导出结果 —— async void ExportResult()
        /// <summary>
        /// 导出结果
        /// </summary>
        public async void ExportResult()
        {
            #region # 验证

            if (this.CameraIntrinsics == null)
            {
                MessageBox.Show("没有标定结果可导出！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.cins)|*.cins",
                FileName = $"{this.ParamViewModel.CameraId}_Intrinsics_{DateTime.Now:yyyyMMddHHmmss}",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string binaryText = this.CameraIntrinsics.ToBinaryString();
                await Task.Run(() => File.WriteAllText(saveFileDialog.FileName, binaryText));
            }

            this.Idle();
        }
        #endregion

        #region 页面失活事件 —— override Task OnDeactivateAsync(bool close...
        /// <summary>
        /// 页面失活事件
        /// </summary>
        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close && this.Images != null)
            {
                foreach (Mat image in this.Images.Values)
                {
                    image.Dispose();
                }
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }
        #endregion

        #endregion
    }
}
