using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 频率域测试
    /// </summary>
    [TestClass]
    public class FrequencyTests
    {
        #region # 测试幅度谱图 —— void TestMagnitudeSpectrum()
        /// <summary>
        /// 测试幅度谱图
        /// </summary>
        [TestMethod]
        public void TestMagnitudeSpectrum()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Deer.jpg", ImreadModes.Grayscale);
            using Mat magnitudeMatrix = matrix.GenerateMagnitudeSpectrum();

            Cv2.ImShow("OpenCV傅里叶变换-原图", matrix);
            Cv2.ImShow("OpenCV傅里叶变换-幅度谱图", magnitudeMatrix);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试相位谱图 —— void TestPhaseSpectrum()
        /// <summary>
        /// 测试相位谱图
        /// </summary>
        [TestMethod]
        public void TestPhaseSpectrum()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Deer.jpg", ImreadModes.Grayscale);
            using Mat phaseMatrix = matrix.GeneratePhaseSpectrum();

            Cv2.ImShow("OpenCV傅里叶变换-原图", matrix);
            Cv2.ImShow("OpenCV傅里叶变换-相位谱图", phaseMatrix);
            Cv2.WaitKey();
        }
        #endregion
    }
}
