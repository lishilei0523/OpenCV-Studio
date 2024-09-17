using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 灰度变换测试
    /// </summary>
    [TestClass]
    public class GrayscaleTests
    {
        #region # 测试线性变换 —— void TestLinearTransform()
        /// <summary>
        /// 测试线性变换
        /// </summary>
        [TestMethod]
        public void TestLinearTransform()
        {
            float alpha = 1f;   //对比度
            float beta = 30f;   //亮度
            using Mat matrix = Cv2.ImRead("Content/Images/Deer.jpg");
            using Mat result = matrix.LinearTransform(alpha, beta);

            Cv2.ImShow("OpenCV线性变换-原图", matrix);
            Cv2.ImShow("OpenCV线性变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试伽马变换 —— void TestGammaTransform()
        /// <summary>
        /// 测试伽马变换
        /// </summary>
        [TestMethod]
        public void TestGammaTransform()
        {
            float gamma = 1 / 2.0f;
            using Mat matrix = Cv2.ImRead("Content/Images/Deer.jpg");
            using Mat result = matrix.GammaTransform(gamma);

            Cv2.ImShow("OpenCV伽马变换-原图", matrix);
            Cv2.ImShow("OpenCV伽马变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试对数变换 —— void TestLogarithmicTransform()
        /// <summary>
        /// 测试对数变换
        /// </summary>
        [TestMethod]
        public void TestLogarithmicTransform()
        {
            float gamma = 1 / 1.2f;
            using Mat matrix = Cv2.ImRead("Content/Images/Deer.jpg");
            using Mat result = matrix.LogarithmicTransform(gamma);

            Cv2.ImShow("OpenCV对数变换-原图", matrix);
            Cv2.ImShow("OpenCV对数变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试距离变换 —— void TestDistanceTrans()
        /// <summary>
        /// 测试距离变换
        /// </summary>
        [TestMethod]
        public void TestDistanceTrans()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Chessboard.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.DistanceTrans();

            Cv2.ImShow("OpenCV距离变换-原图", matrix);
            Cv2.ImShow("OpenCV距离变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试阴影变换 —— void TestShadingTransform()
        /// <summary>
        /// 测试阴影变换
        /// </summary>
        [TestMethod]
        public void TestShadingTransform()
        {
            byte kernelSizeX = 5;
            byte kernelSizeY = 5;
            byte gain = 60;
            byte norse = 0;
            byte offset = 140;

            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.ShadingTransform(new Size(kernelSizeX, kernelSizeY), gain, norse, offset);

            Cv2.ImShow("OpenCV阴影变换-原图", matrix);
            Cv2.ImShow("OpenCV阴影变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
