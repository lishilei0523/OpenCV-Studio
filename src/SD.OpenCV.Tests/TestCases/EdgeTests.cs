using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 边缘检测测试
    /// </summary>
    [TestClass]
    public class EdgeTests
    {
        #region # 测试Robert边缘检测 —— void TestApplyRobert()
        /// <summary>
        /// 测试Robert边缘检测
        /// </summary>
        [TestMethod]
        public void TestApplyRobert()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ApplyRobert();

            Cv2.ImShow("OpenCV Robert边缘检测-原图", matrix);
            Cv2.ImShow("OpenCV Robert边缘检测-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Sobel边缘检测 —— void TestApplySobel()
        /// <summary>
        /// 测试Sobel边缘检测
        /// </summary>
        [TestMethod]
        public void TestApplySobel()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Horse.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ApplySobel();

            Cv2.ImShow("OpenCV Sobel边缘检测-原图", matrix);
            Cv2.ImShow("OpenCV Sobel边缘检测-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Scharr边缘检测 —— void TestApplyScharr()
        /// <summary>
        /// 测试Scharr边缘检测
        /// </summary>
        [TestMethod]
        public void TestApplyScharr()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Horse.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ApplyScharr();

            Cv2.ImShow("OpenCV Scharr边缘检测-原图", matrix);
            Cv2.ImShow("OpenCV Scharr边缘检测-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Laplacian边缘检测 —— void TestApplyLaplacian()
        /// <summary>
        /// 测试Laplacian边缘检测
        /// </summary>
        [TestMethod]
        public void TestApplyLaplacian()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Horse.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ApplyLaplacian();

            Cv2.ImShow("OpenCV Laplacian边缘检测-原图", matrix);
            Cv2.ImShow("OpenCV Laplacian边缘检测-效果图", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
