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
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Models;
using SD.Toolkits.Mathematics.Extensions;
using SD.Toolkits.Mathematics.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// 手眼标定视图模型
    /// </summary>
    public class HandEyeViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public HandEyeViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        #region 已选位姿 —— Pose? SelectedPose
        /// <summary>
        /// 已选位姿
        /// </summary>
        [DependencyProperty]
        public Pose? SelectedPose { get; set; }
        #endregion

        #region 已选位姿矩阵 —— string SelectedPoseMatrix
        /// <summary>
        /// 已选位姿矩阵
        /// </summary>
        [DependencyProperty]
        public string SelectedPoseMatrix { get; set; }
        #endregion

        #region 已选图像源 —— BitmapSource SelectedBitmapSource
        /// <summary>
        /// 已选图像源
        /// </summary>
        [DependencyProperty]
        public BitmapSource SelectedBitmapSource { get; set; }
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

        #region 手眼矩阵 —— Matrix<double> HandEyeMatrix
        /// <summary>
        /// 手眼矩阵
        /// </summary>
        public Matrix<double> HandEyeMatrix { get; set; }
        #endregion

        #region 手眼矩阵文本 —— string HandEyeMatrixText
        /// <summary>
        /// 手眼矩阵文本
        /// </summary>
        [DependencyProperty]
        public string HandEyeMatrixText { get; set; }
        #endregion

        #region 位姿列表 —— ObservableCollection<Pose> Poses
        /// <summary>
        /// 位姿列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<Pose> Poses { get; set; }
        #endregion

        #region 图像源列表 —— ObservableCollection<BitmapSource> BitmapSources
        /// <summary>
        /// 图像源列表
        /// </summary>
        [DependencyProperty]
        public ObservableCollection<BitmapSource> BitmapSources { get; set; }
        #endregion

        #region 标定参数视图模型 —— HandEyeParamViewModel ParamViewModel
        /// <summary>
        /// 标定参数视图模型
        /// </summary>
        [DependencyProperty]
        public HandEyeParamViewModel ParamViewModel { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnInitializeAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            this.BitmapSources = new ObservableCollection<BitmapSource>();

            return base.OnInitializeAsync(cancellationToken);
        }
        #endregion

        #region 打开图像 —— async void OpenImages()
        /// <summary>
        /// 打开图像
        /// </summary>
        public async void OpenImages()
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

                foreach (string fileName in openFileDialog.FileNames)
                {
                    using Mat image = await Task.Run(() => Cv2.ImRead(fileName));
                    BitmapSource bitmapSource = image.ToBitmapSource();
                    this.BitmapSources.Add(bitmapSource);
                }

                //默认预览第一张
                this.SelectedBitmapSource = this.BitmapSources.First();

                this.Idle();
            }
        }
        #endregion

        #region 删除图像 —— void RemoveImage()
        /// <summary>
        /// 删除图像
        /// </summary>
        public void RemoveImage()
        {
            if (this.SelectedBitmapSource != null)
            {
                this.BitmapSources.Remove(this.SelectedBitmapSource);
            }
        }
        #endregion

        #region 打开位姿 —— async void OpenPoses()
        /// <summary>
        /// 打开位姿
        /// </summary>
        public async void OpenPoses()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.csv)|*.csv",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                #region # 验证

                if (string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    MessageBox.Show("未选择位姿！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.Busy();

                this.Poses = new ObservableCollection<Pose>();
                string[] lines = await Task.Run(() => File.ReadAllLines(openFileDialog.FileName));
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] locations = line.Split(',');
                    double x = double.Parse(locations[0].Trim());
                    double y = double.Parse(locations[1].Trim());
                    double z = double.Parse(locations[2].Trim());
                    double rx = double.Parse(locations[3].Trim());
                    double ry = double.Parse(locations[4].Trim());
                    double rz = double.Parse(locations[5].Trim());
                    string id = $"Robot_Pose_{(i + 1):D3}";

                    Pose pose = new Pose(id, x, y, z, rx, ry, rz);
                    this.Poses.Add(pose);
                }

                //默认预览第一个位姿
                Pose first = this.Poses.First();
                this.SelectedPose = first;

                this.Idle();
            }
        }
        #endregion

        #region 选择位姿 —— void SelectPose()
        /// <summary>
        /// 选择位姿
        /// </summary>
        public void SelectPose()
        {
            this.Busy();

            if (this.SelectedPose != null)
            {
                this.SelectedPoseMatrix = this.SelectedPose.Value.ToRotationTranslationMatrix().ToMatrixString("F4");
            }

            this.Idle();
        }
        #endregion

        #region 删除位姿 —— void RemovePose()
        /// <summary>
        /// 删除位姿
        /// </summary>
        public void RemovePose()
        {
            if (this.SelectedPose.HasValue)
            {
                this.Poses.Remove(this.SelectedPose.Value);
            }
        }
        #endregion

        #region 打开内参 —— async void OpenCameraIntrinsics()
        /// <summary>
        /// 打开内参
        /// </summary>
        public async void OpenCameraIntrinsics()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "(*.cins)|*.cins",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (openFileDialog.ShowDialog() == true)
            {
                #region # 验证

                if (string.IsNullOrWhiteSpace(openFileDialog.FileName))
                {
                    MessageBox.Show("未选择相机内参！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                #endregion

                this.Busy();

                string binaryText = await Task.Run(() => File.ReadAllText(openFileDialog.FileName));
                this.CameraIntrinsics = binaryText.AsBinaryTo<CameraIntrinsics>();
                this.CalibratedReprojectionError = this.CameraIntrinsics.CalibratedReprojectionError.ToString("F9");
                this.ReprojectionError = this.CameraIntrinsics.ReprojectionError.ToString("F9");
                this.DistortionVector = DenseVector.OfArray(this.CameraIntrinsics.DistortionVector).ToVectorString("F10");
                this.IntrinsicMatrix = DenseMatrix.OfArray(this.CameraIntrinsics.IntrinsicMatrix).ToMatrixString("F4");

                this.Idle();
            }
        }
        #endregion

        #region 标定参数 —— async void SetParameters()
        /// <summary>
        /// 标定参数
        /// </summary>
        public async void SetParameters()
        {
            HandEyeParamViewModel viewModel = this.ParamViewModel ?? ResolveMediator.Resolve<HandEyeParamViewModel>();
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

            if (this.SelectedBitmapSource == null)
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

            using Mat image = this.SelectedBitmapSource.ToMat();
            using Mat grayImage = image.Type() == MatType.CV_8UC3
                ? image.CvtColor(ColorConversionCodes.BGR2GRAY)
                : image.Clone();
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
                this.SelectedBitmapSource = image.ToBitmapSource();
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

            if (this.BitmapSources == null || !this.BitmapSources.Any())
            {
                MessageBox.Show("图像未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.Poses == null || !this.Poses.Any())
            {
                MessageBox.Show("位姿未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.BitmapSources.Count != this.Poses.Count)
            {
                MessageBox.Show("图像与位姿数量不一致！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.ParamViewModel == null)
            {
                MessageBox.Show("标定参数未设置！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (this.CameraIntrinsics == null)
            {
                MessageBox.Show("相机内参未加载！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBoxResult confirmed = MessageBox.Show("确定要执行标定？", "警告", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirmed != MessageBoxResult.Yes)
            {
                return;
            }

            #endregion

            this.Busy();

            PatternType patternType = this.ParamViewModel.SelectedPatternType!.Value;
            int patternSideSize = this.ParamViewModel.PatternSideSize!.Value;
            Size patternSize = new Size(this.ParamViewModel.RowPointsCount!.Value, this.ParamViewModel.ColumnPointsCount!.Value);
            int maxCount = this.ParamViewModel.MaxCount!.Value;
            double epsilon = this.ParamViewModel.Epsilon!.Value;
            IDictionary<string, Pose> robotPoses = new Dictionary<string, Pose>();
            for (int i = 0; i < this.BitmapSources.Count; i++)
            {
                Pose pose = this.Poses.ElementAt(i);
                robotPoses.Add(i.ToString(), pose);
            }

            //转灰度图
            IDictionary<string, Mat> grayImages = new Dictionary<string, Mat>();
            for (int i = 0; i < this.BitmapSources.Count; i++)
            {
                BitmapSource bitmapSource = this.BitmapSources[i];
                using Mat image = bitmapSource.ToMat();
                Mat grayImage;
                if (image.Type() == MatType.CV_8UC3)
                {
                    grayImage = image.CvtColor(ColorConversionCodes.BGR2GRAY);
                }
                else if (image.Type() == MatType.CV_8UC1)
                {
                    grayImage = image.Clone();
                }
                else
                {
                    MessageBox.Show($"不支持的图像格式：{image.Type()}！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                grayImages.Add(i.ToString(), grayImage);
            }

            IDictionary<string, Matrix<double>> extrinsicMatrices = await Task.Run(() => Calibrator.SolveExtrinsicMatrices(patternSideSize, patternSize, patternType, maxCount, epsilon, this.CameraIntrinsics, grayImages));
            if (this.ParamViewModel.SelectedHandEyeMode == HandEyeMode.EyeInHand)
            {
                this.HandEyeMatrix = await Task.Run(() => Calibrator.CalibrateEyeInHand(HandEyeCalibrationMethod.TSAI, robotPoses, extrinsicMatrices));
            }
            else if (this.ParamViewModel.SelectedHandEyeMode == HandEyeMode.EyeToHand)
            {
                this.HandEyeMatrix = await Task.Run(() => Calibrator.CalibrateEyeToHand(HandEyeCalibrationMethod.TSAI, robotPoses, extrinsicMatrices));
            }
            else
            {
                throw new NotSupportedException();
            }
            this.HandEyeMatrixText = this.HandEyeMatrix.ToMatrixString("F4");

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

            if (this.HandEyeMatrix == null)
            {
                MessageBox.Show("没有标定结果可导出！", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            #endregion

            this.Busy();

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "(*.chem)|*.chem",
                FileName = $"{this.ParamViewModel.CameraId}_HandEye_{DateTime.Now:yyyyMMddHHmmss}",
                AddExtension = true,
                RestoreDirectory = true
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                string binaryText = this.HandEyeMatrix.ToBinaryString();
                await Task.Run(() => File.WriteAllText(saveFileDialog.FileName, binaryText));
            }

            this.Idle();
        }
        #endregion

        #endregion
    }
}
