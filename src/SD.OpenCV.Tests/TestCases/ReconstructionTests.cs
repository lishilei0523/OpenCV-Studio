using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Calibrations;
using SD.OpenCV.Primitives.Models;
using SD.OpenCV.Reconstructions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 重建测试
    /// </summary>
    [TestClass]
    public class ReconstructionTests
    {
        #region # 测试匹配图像 —— void TestMatchImage()
        /// <summary>
        /// 测试匹配图像
        /// </summary>
        [TestMethod]
        public void TestMatchImage()
        {
            //初始化模型
            Reconstructor.Initialize();

            //读取图像
            using Mat image1 = Cv2.ImRead("Content/Images/Faw/001.bmp");
            using Mat image2 = Cv2.ImRead("Content/Images/Faw/006.bmp");

            //匹配图像
            MatchResult matchResult = Reconstructor.Match(image1, image2, 0.8f);

            Trace.WriteLine($"匹配关键点数量: {matchResult.MatchedCount}");
        }
        #endregion

        #region # 测试重建图像 —— void TestRecoverImage()
        /// <summary>
        /// 测试重建图像
        /// </summary>
        [TestMethod]
        public void TestRecoverImage()
        {
            //初始化模型
            Reconstructor.Initialize();

            //读取图像
            using Mat image1 = Cv2.ImRead("Content/Images/Faw/001.bmp");
            using Mat image2 = Cv2.ImRead("Content/Images/Faw/006.bmp");

            //重建图像
            using Mat result = Reconstructor.RecoverImage(image1, image2, 0.8f);

            Cv2.ImShow("OpenCV重建图像-原图1", image1);
            Cv2.ImShow("OpenCV重建图像-原图2", image2);
            Cv2.ImShow("OpenCV重建图像-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试重建位姿 —— void TestRecoverPose()
        /// <summary>
        /// 测试重建位姿
        /// </summary>
        [TestMethod]
        public void TestRecoverPose()
        {
            //初始化模型
            Reconstructor.Initialize();

            //标定相机
            CameraIntrinsics cameraIntrinsics = CalibrateCamera();

            //读取图像
            using Mat image1 = Cv2.ImRead("Content/Images/Faw/001.bmp");
            using Mat image2 = Cv2.ImRead("Content/Images/Faw/002.bmp");

            //重建位姿
            double[,] rtArray4x4 = Reconstructor.RecoverPose(image1, image2, cameraIntrinsics.IntrinsicMatrix, 0.98f);
            Matrix<double> rtMatrix = DenseMatrix.OfArray(rtArray4x4);

            Trace.WriteLine("RT矩阵: ");
            Trace.WriteLine(rtMatrix);
        }
        #endregion


        //Private

        #region # 标定相机 —— static CameraIntrinsics CalibrateCamera()
        /// <summary>
        /// 标定相机
        /// </summary>
        private static CameraIntrinsics CalibrateCamera()
        {
            //读取图像
            string imageDir = "Content/Images/Faw";
            string[] imagePaths = Directory.GetFiles(imageDir);
            IDictionary<string, Mat> images = new Dictionary<string, Mat>();
            foreach (string imagePath in imagePaths)
            {
                Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
                images.Add(Path.GetFileNameWithoutExtension(imagePath), image);
            }

            //设置参数
            float patternSideSize = 0.05f; //标定板方格边长
            Size patternSize = new Size(11, 8);//标定板尺寸，行列内角点个数
            PatternType patternType = PatternType.Chessboard;//标定板类型
            Size imageSize = new Size(2048, 1536);//图像尺寸

            //标定相机
            CameraIntrinsics cameraIntrinsics = Calibrator.MonoCalibrate(Guid.NewGuid().ToString(), patternSideSize, patternSize, patternType, 30, 0.01, imageSize, images, out _, out _);

            return cameraIntrinsics;
        }
        #endregion
    }
}
