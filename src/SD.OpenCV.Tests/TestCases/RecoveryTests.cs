using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;
using System.Collections.Generic;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 图像恢复测试
    /// </summary>
    [TestClass]
    public class RecoveryTests
    {
        #region # 测试拼接图像 —— void TestStitch()
        /// <summary>
        /// 测试拼接图像
        /// </summary>
        [TestMethod]
        public void TestStitch()
        {
            IList<Mat> matrices = new List<Mat>
            {
                Cv2.ImRead("Content/Images/Earth-001.jpg"),
                Cv2.ImRead("Content/Images/Earth-002.jpg")
            };
            using Mat result = matrices.Stitch();

            Cv2.ImShow("OpenCV拼接图像", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试修复图像 —— void TestInpainte()
        /// <summary>
        /// 测试修复图像
        /// </summary>
        [TestMethod]
        public void TestInpainte()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Lena.jpg");
            Rect rectangle = new Rect(0, 0, 120, 160);
            using Mat result = matrix.Inpainte(rectangle);

            Cv2.ImShow("OpenCV修复图像-原图", matrix);
            Cv2.ImShow("OpenCV修复图像-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试曝光融合 —— void TestExposureFusion()
        /// <summary>
        /// 测试曝光融合
        /// </summary>
        [TestMethod]
        public void TestExposureFusion()
        {
            IList<Mat> matrices = new List<Mat>
            {
                Cv2.ImRead("Content/Images/EF-001.png"),
                Cv2.ImRead("Content/Images/EF-002.png"),
                Cv2.ImRead("Content/Images/EF-003.png"),
                Cv2.ImRead("Content/Images/EF-004.png")
            };
            using Mat result = matrices.ExposureFusion();
            result.SaveImage("EF.png");

            Cv2.ImShow("OpenCV曝光融合", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
