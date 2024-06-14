using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 形态学测试
    /// </summary>
    [TestClass]
    public class MorphologyTests
    {
        #region # 测试腐蚀 —— void TestErode()
        /// <summary>
        /// 测试腐蚀
        /// </summary>
        [TestMethod]
        public void TestErode()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphErode();

            Cv2.ImShow("OpenCV腐蚀-原图", matrix);
            Cv2.ImShow("OpenCV腐蚀-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试膨胀 —— void TestDilate()
        /// <summary>
        /// 测试膨胀
        /// </summary>
        [TestMethod]
        public void TestDilate()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphDilate();

            Cv2.ImShow("OpenCV膨胀-原图", matrix);
            Cv2.ImShow("OpenCV膨胀-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试开运算 —— void TestOpen()
        /// <summary>
        /// 测试开运算
        /// </summary>
        [TestMethod]
        public void TestOpen()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphOpen();

            Cv2.ImShow("OpenCV开运算-原图", matrix);
            Cv2.ImShow("OpenCV开运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试闭运算 —— void TestClose()
        /// <summary>
        /// 测试闭运算
        /// </summary>
        [TestMethod]
        public void TestClose()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphClose();

            Cv2.ImShow("OpenCV闭运算-原图", matrix);
            Cv2.ImShow("OpenCV闭运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试梯度运算 —— void TestGradient()
        /// <summary>
        /// 测试梯度运算
        /// </summary>
        [TestMethod]
        public void TestGradient()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphGradient();

            Cv2.ImShow("OpenCV梯度运算-原图", matrix);
            Cv2.ImShow("OpenCV梯度运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试礼帽运算 —— void TestTopHat()
        /// <summary>
        /// 测试礼帽运算
        /// </summary>
        [TestMethod]
        public void TestTopHat()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphTopHat();

            Cv2.ImShow("OpenCV礼帽运算-原图", matrix);
            Cv2.ImShow("OpenCV礼帽运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试黑帽运算 —— void TestBlackHat()
        /// <summary>
        /// 测试黑帽运算
        /// </summary>
        [TestMethod]
        public void TestBlackHat()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            using Mat result = matrix.MorphBlackHat();

            Cv2.ImShow("OpenCV黑帽运算-原图", matrix);
            Cv2.ImShow("OpenCV黑帽运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试击中与否运算 —— void TestHitMiss()
        /// <summary>
        /// 测试击中与否运算
        /// </summary>
        [TestMethod]
        public void TestHitMiss()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.MorphHitMiss();

            Cv2.ImShow("OpenCV击中与否运算-原图", matrix);
            Cv2.ImShow("OpenCV击中与否运算-效果图", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
