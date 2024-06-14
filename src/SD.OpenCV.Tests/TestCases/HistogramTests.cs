using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 直方图测试
    /// </summary>
    [TestClass]
    public class HistogramTests
    {
        #region # 测试直方图规定化 —— void TestSpecifyHist()
        /// <summary>
        /// 测试直方图规定化
        /// </summary>
        [TestMethod]
        public void TestSpecifyHist()
        {
            using Mat sourceMatrix = Cv2.ImRead("Content/Images/Horse.jpg");
            using Mat referenceMatrix = Cv2.ImRead("Content/Images/Cat.jpg");

            Cv2.Split(sourceMatrix, out Mat[] sourceChannels);
            Cv2.Split(referenceMatrix, out Mat[] referenceChannels);

            using Mat bChannel = sourceChannels[0].SpecifyHist(referenceChannels[0]);
            using Mat gChannel = sourceChannels[1].SpecifyHist(referenceChannels[1]);
            using Mat rChannel = sourceChannels[2].SpecifyHist(referenceChannels[2]);

            using Mat result = new Mat();
            Mat[] channels = { bChannel, gChannel, rChannel };
            Cv2.Merge(channels, result);

            Cv2.ImShow("OpenCV直方图规定化-原图1", sourceMatrix);
            Cv2.ImShow("OpenCV直方图规定化-原图2", referenceMatrix);
            Cv2.ImShow("OpenCV直方图规定化-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试自适应直方图均衡化 —— void TestAdaptiveEqualizeHist()
        /// <summary>
        /// 测试自适应直方图均衡化
        /// </summary>
        [TestMethod]
        public void TestAdaptiveEqualizeHist()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.AdaptiveEqualizeHist();

            Cv2.ImShow("OpenCV自适应直方图均衡化-原图", matrix);
            Cv2.ImShow("OpenCV自适应直方图均衡化-效果图", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
