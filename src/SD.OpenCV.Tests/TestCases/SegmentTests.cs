using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 图像分割测试
    /// </summary>
    [TestClass]
    public class SegmentTests
    {
        #region # 测试生成矩形掩膜 —— void TestGenerateRectangleMask()
        /// <summary>
        /// 测试生成矩形掩膜
        /// </summary>
        [TestMethod]
        public void TestGenerateRectangleMask()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Ballon.jpg");

            Rect rectangle = new Rect(140, 45, 330, 335);
            using Mat mask = matrix.GenerateMask(rectangle);

            Cv2.ImShow("OpenCV生成掩膜-原图", matrix);
            Cv2.ImShow("OpenCV生成掩膜-掩膜", mask);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试生成多边形掩膜 —— void TestGeneratePolygonMask()
        /// <summary>
        /// 测试生成多边形掩膜
        /// </summary>
        [TestMethod]
        public void TestGeneratePolygonMask()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg");

            Point[] contourPoints =
            {
                new Point(100, 100),
                new Point(1000, 150),
                new Point(1200, 600),
                new Point(400, 700)
            };
            using Mat mask = matrix.GenerateMask(contourPoints);

            Cv2.ImShow("OpenCV生成掩膜-原图", matrix);
            Cv2.ImShow("OpenCV生成掩膜-掩膜", mask);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试适用矩形掩膜 —— void TestApplyRectangleMask()
        /// <summary>
        /// 测试适用矩形掩膜
        /// </summary>
        [TestMethod]
        public void TestApplyRectangleMask()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Ballon.jpg");

            Rect rectangle = new Rect(140, 45, 330, 335);
            using Mat result = matrix.ApplyMask(rectangle);

            Cv2.ImShow("OpenCV适用掩膜-原图", matrix);
            Cv2.ImShow("OpenCV适用掩膜-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试适用多边形掩膜 —— void TestApplyPolygonMask()
        /// <summary>
        /// 测试适用多边形掩膜
        /// </summary>
        [TestMethod]
        public void TestApplyPolygonMask()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg");

            Point[] contourPoints =
            {
                new Point(100, 100),
                new Point(1000, 150),
                new Point(1200, 600),
                new Point(400, 700)
            };
            using Mat result = matrix.ApplyMask(contourPoints);

            Cv2.ImShow("OpenCV适用掩膜-原图", matrix);
            Cv2.ImShow("OpenCV适用掩膜-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试提取矩形内图像 —— void TestExtractMatrixInRectangle()
        /// <summary>
        /// 测试提取矩形内图像
        /// </summary>
        [TestMethod]
        public void TestExtractMatrixInRectangle()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);

            Rect rectangle = new Rect(100, 100, 640, 480);
            using Mat contourMatrix = matrix.ExtractMatrixInRectangle(rectangle);

            Cv2.ImShow("OpenCV提取矩形内图像-原图", matrix);
            Cv2.ImShow("OpenCV提取矩形内图像-效果图", contourMatrix);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试提取矩形内像素 —— void TestExtractMatrixInRectangle()
        /// <summary>
        /// 测试提取矩形内像素
        /// </summary>
        [TestMethod]
        public void TestExtractPixelsInRectangle()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);

            Rect rectangle = new Rect(100, 100, 640, 480);
            IEnumerable<byte> pixels = matrix.ExtractPixelsInRectangle(rectangle);
            Assert.IsTrue(pixels.Any());
        }
        #endregion

        #region # 测试提取轮廓内图像 —— void TestExtractMatrixInContour()
        /// <summary>
        /// 测试提取轮廓内图像
        /// </summary>
        [TestMethod]
        public void TestExtractMatrixInContour()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);

            Point[] contourPoints =
            {
                new Point(100, 100),
                new Point(1000, 150),
                new Point(1200, 600),
                new Point(400, 700)
            };
            using Mat contourMatrix = matrix.ExtractMatrixInContour(contourPoints);

            Cv2.ImShow("OpenCV提取轮廓内图像-原图", matrix);
            Cv2.ImShow("OpenCV提取轮廓内图像-效果图", contourMatrix);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试提取轮廓内像素 —— void TestExtractMatrixInContour()
        /// <summary>
        /// 测试提取轮廓内像素
        /// </summary>
        [TestMethod]
        public void TestExtractPixelsInContour()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);

            Point[] contourPoints =
            {
                new Point(100, 100),
                new Point(1000, 150),
                new Point(1200, 600),
                new Point(400, 700)
            };
            IEnumerable<byte> pixels = matrix.ExtractPixelsInContour(contourPoints);
            Assert.IsTrue(pixels.Any());
        }
        #endregion

        #region # 测试颜色分割 —— void TestColorSegment()
        /// <summary>
        /// 测试颜色分割
        /// </summary>
        [TestMethod]
        public void TestColorSegment()
        {
            Scalar lowerYellow = new Scalar(20, 43, 46);
            Scalar upperYellow = new Scalar(34, 255, 255);

            using Mat matrix = Cv2.ImRead("Content/Images/Flag.jpg");
            using Mat hsvMatrix = matrix.CvtColor(ColorConversionCodes.BGR2HSV);
            Mat result = hsvMatrix.ColorSegment(lowerYellow, upperYellow);

            Cv2.ImShow("OpenCV颜色分割-原图", matrix);
            Cv2.ImShow("OpenCV颜色分割-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试K-Means聚类分割 —— void TestKMeansSegment()
        /// <summary>
        /// 测试K-Means聚类分割
        /// </summary>
        [TestMethod]
        public void TestKMeansSegment()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Ballon.jpg");
            using Mat result = matrix.KMeansSegment(3);

            Cv2.ImShow("OpenCV K-Means聚类分割-原图", matrix);
            Cv2.ImShow("OpenCV K-Means聚类分割-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试滑动窗口 —— void TestSlideWindow()
        /// <summary>
        /// 测试线性变换
        /// </summary>
        [TestMethod]
        public void TestSlideWindow()
        {
            Action<Rect, Mat> action = (rect, roi) =>
            {
                roi.SaveImage($"Content/Images/{rect.X}-{rect.Y}.jpg");
            };

            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            for (int i = 0; i < 10; i++)
            {
                matrix.SlideWindow(200, 200, 50, action);
            }

            Cv2.WaitKey();
        }
        #endregion
    }
}
