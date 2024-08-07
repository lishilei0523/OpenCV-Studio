using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 几何变换测试
    /// </summary>
    [TestClass]
    public class GeometryTests
    {
        #region # 测试绝对缩放 —— void TestResizeAbsolutely()
        /// <summary>
        /// 测试绝对缩放
        /// </summary>
        [TestMethod]
        public void TestResizeAbsolutely()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Chessboard.jpg");
            using Mat result = matrix.ResizeAbsolutely(400, 400);

            Cv2.ImShow("OpenCV绝对缩放-原图", matrix);
            Cv2.ImShow("OpenCV绝对缩放-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试相对缩放 —— void TestResizeRelatively()
        /// <summary>
        /// 测试相对缩放
        /// </summary>
        [TestMethod]
        public void TestResizeRelatively()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Chessboard.jpg");
            using Mat result = matrix.ResizeRelatively(0.4f);

            Cv2.ImShow("OpenCV相对缩放-原图", matrix);
            Cv2.ImShow("OpenCV相对缩放-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试自适应缩放 —— void TestResizeAdaptively()
        /// <summary>
        /// 测试自适应缩放
        /// </summary>
        [TestMethod]
        public void TestResizeAdaptively()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg");
            using Mat result = matrix.ResizeAdaptively(512);

            Cv2.ImShow("OpenCV自适应缩放-原图", matrix);
            Cv2.ImShow("OpenCV自适应缩放-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试仿射变换 —— void TestAffineTrans()
        /// <summary>
        /// 测试仿射变换
        /// </summary>
        [TestMethod]
        public void TestAffineTrans()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Chessboard.jpg");

            Point2f[] sourcePoints =
            {
                new Point2f(50, 50),
                new Point2f(200, 50),
                new Point2f(50, 200)
            };
            Point2f[] targetPoints =
            {
                new Point2f(100, 100),
                new Point2f(200, 50),
                new Point2f(100, 250)
            };
            using Mat result = matrix.AffineTrans(sourcePoints, targetPoints);

            Cv2.ImShow("OpenCV仿射变换-原图", matrix);
            Cv2.ImShow("OpenCV仿射变换-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试透视变换 —— void TestPerspectiveTrans()
        /// <summary>
        /// 测试透视变换
        /// </summary>
        [TestMethod]
        public void TestPerspectiveTrans()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Chessboard.jpg");

            Point2f[] sourcePoints =
            {
                new Point2f(56, 65),
                new Point2f(368, 52),
                new Point2f(28, 387),
                new Point2f(389, 390)
            };
            Point2f[] targetPoints =
            {
                new Point2f(100, 145),
                new Point2f(300, 100),
                new Point2f(80, 290),
                new Point2f(310, 300)
            };
            using Mat result = matrix.PerspectiveTrans(sourcePoints, targetPoints);

            Cv2.ImShow("OpenCV透视变换-原图", matrix);
            Cv2.ImShow("OpenCV透视变换-效果图", result);
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
    }
}
