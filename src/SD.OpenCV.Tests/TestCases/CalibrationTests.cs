using MathNet.Numerics.LinearAlgebra;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Models;
using SD.Toolkits.Mathematics.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 标定测试
    /// </summary>
    [TestClass]
    public class CalibrationTests
    {
        #region # 测试标定相机 —— void TestCalibrateCamera()
        /// <summary>
        /// 测试标定相机
        /// </summary>
        [TestMethod]
        public void TestCalibrateCamera()
        {
            //读取图片
            string imageDir = "Content/Images/Standard";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //设置参数
            int patternSideSize = 5; //标定板方格边长
            Size patternSize = new Size(6, 9);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(640, 480);//图像尺寸

            //标定
            CameraIntrinsics cameraIntrinsics = Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out ICollection<string> failedImageKeys);

            Trace.WriteLine($"标定重投影误差: {cameraIntrinsics.CalibratedReprojectionError}");
            Trace.WriteLine($"重投影误差: {cameraIntrinsics.ReprojectionError}");
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("畸变向量:");
            Trace.WriteLine(cameraIntrinsics.DistortionVector);
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("内参矩阵:");
            Trace.WriteLine(cameraIntrinsics.IntrinsicMatrix);
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("外参矩阵:");
            foreach (KeyValuePair<string, Matrix<double>> kv in extrinsicMatrices)
            {
                Trace.WriteLine($"图像: {kv.Key}");
                Trace.WriteLine(kv.Value);
                Trace.WriteLine("---------------------");
            }
            foreach (string failedImageKey in failedImageKeys)
            {
                Trace.WriteLine($"图像: {failedImageKey}计算角点失败！");
            }

            //释放资源
            foreach (Mat image in images.Values)
            {
                image.Dispose();
            }
        }
        #endregion

        #region # 测试标定眼在手上 —— void TestCalibrateEyeInHand()
        /// <summary>
        /// 测试标定眼在手上
        /// </summary>
        [TestMethod]
        public void TestCalibrateEyeInHand()
        {
            //读取机器人位姿
            string filePath = "Content/RobotPoses/RobotPosts.csv";
            string[] lines = File.ReadAllLines(filePath);
            IDictionary<string, Pose> robotPoses = new Dictionary<string, Pose>();
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
                string id = (i + 1).ToString("D3");

                Pose robotPose = new Pose(id, x, y, z, rx, ry, rz);
                robotPoses.Add(robotPose.Id, robotPose);
            }

            //读取图片
            string imageDir = "Content/Images/Faw";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //设置参数
            int patternSideSize = 5; //标定板方格边长
            Size patternSize = new Size(11, 8);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(2048, 1536);//图像尺寸

            //标定相机
            Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out ICollection<string> failedImageKeys);

            //标定手眼
            Matrix<double> rtMatrix = Calibrator.CalibrateEyeInHand(HandEyeCalibrationMethod.TSAI, robotPoses, extrinsicMatrices);
            Trace.WriteLine(rtMatrix);

            //释放资源
            foreach (Mat image in images.Values)
            {
                image.Dispose();
            }
        }
        #endregion

        #region # 测试标定眼在手外 —— void TestCalibrateEyeToHand()
        /// <summary>
        /// 测试标定眼在手外
        /// </summary>
        [TestMethod]
        public void TestCalibrateEyeToHand()
        {
            //读取图片
            string imageDir = "Content/Images/Faw";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //读取机器人位姿
            string filePath = "Content/RobotPoses/RobotPosts.csv";
            string[] lines = File.ReadAllLines(filePath);
            IDictionary<string, Pose> robotPoses = new Dictionary<string, Pose>();
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
                string id = (i + 1).ToString("D3");

                Pose robotPose = new Pose(id, x, y, z, rx, ry, rz);
                robotPoses.Add(robotPose.Id, robotPose);
            }

            //设置参数
            int patternSideSize = 5; //标定板方格边长
            Size patternSize = new Size(11, 8);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(2048, 1536);//图像尺寸

            //标定相机
            Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out ICollection<string> failedImageKeys);

            //标定手眼
            Matrix<double> rtMatrix = Calibrator.CalibrateEyeToHand(HandEyeCalibrationMethod.TSAI, robotPoses, extrinsicMatrices);
            Trace.WriteLine(rtMatrix);

            //释放资源
            foreach (Mat image in images.Values)
            {
                image.Dispose();
            }
        }
        #endregion

        #region # 测试解析外参 —— void TestSolveExtrinsic()
        /// <summary>
        /// 测试解析外参
        /// </summary>
        [TestMethod]
        public void TestSolveExtrinsic()
        {
            //读取图片
            string imageDir = "Content/Images/Standard";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //设置参数
            int patternSideSize = 5; //标定板方格边长，单位mm
            Size patternSize = new Size(6, 9);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(640, 480);//图像尺寸

            //标定相机
            CameraIntrinsics cameraIntrinsics = Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out _);

            //解析外参
            string testImagePath = "Content/Images/Standard/left01.jpg";
            string testImageName = Path.GetFileNameWithoutExtension(testImagePath);
            using Mat testImage = Cv2.ImRead(testImagePath, ImreadModes.Grayscale);
            Matrix<double> extrinsicMatrix = Calibrator.SolveExtrinsicMatrix(patternSideSize, patternSize, patternType, 30, 0.01, testImage, cameraIntrinsics);

            Trace.WriteLine($"图像: \"{testImageName}\": ");
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("外参-内参携带: ");
            Trace.WriteLine(extrinsicMatrix);
            Trace.WriteLine("----------------------------------");
            Trace.WriteLine("外参-外参解析: ");
            Trace.WriteLine(extrinsicMatrices[testImageName]);

            //释放资源
            foreach (Mat image in images.Values)
            {
                image.Dispose();
            }
        }
        #endregion

        #region # 测试矫正畸变 —— void TestRectifyDistortions()
        /// <summary>
        /// 测试矫正畸变
        /// </summary>
        [TestMethod]
        public void TestRectifyDistortions()
        {
            //读取图片
            string imageDir = "Content/Images/Standard";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //设置参数
            int patternSideSize = 5; //标定板方格边长，单位mm
            Size patternSize = new Size(6, 9);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(640, 480);//图像尺寸

            //标定相机
            CameraIntrinsics cameraIntrinsics = Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out _);

            //矫正畸变
            string testImagePath = "Content/Images/Standard/left02.jpg";
            using Mat testImage = Cv2.ImRead(testImagePath);
            using Mat result = testImage.RectifyDistortions(cameraIntrinsics);

            Cv2.ImShow("OpenCV矫正畸变-原图", testImage);
            Cv2.ImShow("OpenCV矫正畸变-效果图", result);
            Cv2.WaitKey();

            //释放资源
            foreach (Mat image in images.Values)
            {
                image.Dispose();
            }
        }
        #endregion
    }
}
