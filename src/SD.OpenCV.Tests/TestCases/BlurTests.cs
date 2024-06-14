using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 滤波测试
    /// </summary>
    [TestClass]
    public class BlurTests
    {
        #region # 测试单尺度Retinex增强 —— void TestSingleScaleRetinex()
        /// <summary>
        /// 测试单尺度Retinex增强
        /// </summary>
        [TestMethod]
        public void TestSingleScaleRetinex()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Cloud.jpg");
            using Mat result = matrix.SingleScaleRetinex(80);

            Cv2.ImShow("OpenCV SSR-原图", matrix);
            Cv2.ImShow("OpenCV SSR-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试锐化滤波 —— void TestSharpBlur()
        /// <summary>
        /// 测试锐化滤波
        /// </summary>
        [TestMethod]
        public void TestSharpBlur()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/Cat.jpg");
            using Mat result = matrix.SharpBlur();

            Cv2.ImShow("OpenCV锐化滤波-原图", matrix);
            Cv2.ImShow("OpenCV锐化滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试理想低通滤波 —— void TestIdealLPBlur()
        /// <summary>
        /// 测试理想低通滤波
        /// </summary>
        [TestMethod]
        public void TestIdealLPBlur()
        {
            float sigma = 50f;
            using Mat matrix = Cv2.ImRead("Content/Images/Avatar.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.IdealLPBlur(sigma);

            Cv2.ImShow("OpenCV理想低通滤波-原图", matrix);
            Cv2.ImShow("OpenCV理想低通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试理想高通滤波 —— void TestIdealHPBlur()
        /// <summary>
        /// 测试理想高通滤波
        /// </summary>
        [TestMethod]
        public void TestIdealHPBlur()
        {
            float sigma = 5f;
            using Mat matrix = Cv2.ImRead("Content/Images/Avatar.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.IdealHPBlur(sigma);

            Cv2.ImShow("OpenCV理想高通滤波-原图", matrix);
            Cv2.ImShow("OpenCV理想高通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试理想带通滤波 —— void TestIdealBPBlur()
        /// <summary>
        /// 测试理想带通滤波
        /// </summary>
        [TestMethod]
        public void TestIdealBPBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.IdealBPBlur(sigma, bandWidth);

            Cv2.ImShow("OpenCV理想带通滤波-原图", matrix);
            Cv2.ImShow("OpenCV理想带通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试理想带阻滤波 —— void TestIdealBRBlur()
        /// <summary>
        /// 测试理想带阻滤波
        /// </summary>
        [TestMethod]
        public void TestIdealBRBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.IdealBRBlur(sigma, bandWidth);

            Cv2.ImShow("OpenCV理想带阻滤波-原图", matrix);
            Cv2.ImShow("OpenCV理想带阻滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试高斯低通滤波 —— void TestGaussianLPBlur()
        /// <summary>
        /// 测试高斯低通滤波
        /// </summary>
        [TestMethod]
        public void TestGaussianLPBlur()
        {
            float sigma = 50f;
            using Mat matrix = Cv2.ImRead("Content/Images/Cat.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.GaussianLPBlur(sigma);

            Cv2.ImShow("OpenCV高斯低通滤波-原图", matrix);
            Cv2.ImShow("OpenCV高斯低通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试高斯高通滤波 —— void TestGaussianHPBlur()
        /// <summary>
        /// 测试高斯高通滤波
        /// </summary>
        [TestMethod]
        public void TestGaussianHPBlur()
        {
            float sigma = 5f;
            using Mat matrix = Cv2.ImRead("Content/Images/Cat.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.GaussianHPBlur(sigma);

            Cv2.ImShow("OpenCV高斯高通滤波-原图", matrix);
            Cv2.ImShow("OpenCV高斯高通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试高斯带通滤波 —— void TestGaussianBPBlur()
        /// <summary>
        /// 测试高斯带通滤波
        /// </summary>
        [TestMethod]
        public void TestGaussianBPBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.GaussianBPBlur(sigma, bandWidth);

            Cv2.ImShow("OpenCV高斯带通滤波-原图", matrix);
            Cv2.ImShow("OpenCV高斯带通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试高斯带阻滤波 —— void TestGaussianBRBlur()
        /// <summary>
        /// 测试高斯带阻滤波
        /// </summary>
        [TestMethod]
        public void TestGaussianBRBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.GaussianBRBlur(sigma, bandWidth);

            Cv2.ImShow("OpenCV高斯带阻滤波-原图", matrix);
            Cv2.ImShow("OpenCV高斯带阻滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试高斯同态滤波 —— void TestGaussianHomoBlur()
        /// <summary>
        /// 测试高斯同态滤波
        /// </summary>
        [TestMethod]
        public void TestGaussianHomoBlur()
        {
            float gammaH = 2.2f;
            float gammaL = 0.5f;
            float sigma = 0.01f;
            float slope = 1f;
            using Mat matrix = Cv2.ImRead("Content/Images/Book.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.GaussianHomoBlur(gammaH, gammaL, sigma, slope);

            Cv2.ImShow("OpenCV高斯同态滤波-原图", matrix);
            Cv2.ImShow("OpenCV高斯同态滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试巴特沃斯低通滤波 —— void TestButterworthLPBlur()
        /// <summary>
        /// 测试巴特沃斯低通滤波
        /// </summary>
        [TestMethod]
        public void TestButterworthLPBlur()
        {
            float sigma = 50f;
            using Mat matrix = Cv2.ImRead("Content/Images/Cat.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ButterworthLPBlur(sigma, 2);

            Cv2.ImShow("OpenCV巴特沃斯低通滤波-原图", matrix);
            Cv2.ImShow("OpenCV巴特沃斯低通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试巴特沃斯高通滤波 —— void TestButterworthHPBlur()
        /// <summary>
        /// 测试巴特沃斯高通滤波
        /// </summary>
        [TestMethod]
        public void TestButterworthHPBlur()
        {
            float sigma = 5f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ButterworthHPBlur(sigma, 2);

            Cv2.ImShow("OpenCV巴特沃斯高通滤波-原图", matrix);
            Cv2.ImShow("OpenCV巴特沃斯高通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试巴特沃斯带通滤波 —— void TestButterworthBPBlur()
        /// <summary>
        /// 测试巴特沃斯带通滤波
        /// </summary>
        [TestMethod]
        public void TestButterworthBPBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ButterworthBPBlur(sigma, bandWidth, 2);

            Cv2.ImShow("OpenCV巴特沃斯带通滤波-原图", matrix);
            Cv2.ImShow("OpenCV巴特沃斯带通滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试巴特沃斯带阻滤波 —— void TestButterworthBRBlur()
        /// <summary>
        /// 测试巴特沃斯带阻滤波
        /// </summary>
        [TestMethod]
        public void TestButterworthBRBlur()
        {
            float sigma = 18f;
            float bandWidth = 23f;
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ButterworthBRBlur(sigma, bandWidth, 2);

            Cv2.ImShow("OpenCV巴特沃斯带阻滤波-原图", matrix);
            Cv2.ImShow("OpenCV巴特沃斯带阻滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试巴特沃斯同态滤波 —— void TestButterworthHomoBlur()
        /// <summary>
        /// 测试巴特沃斯同态滤波
        /// </summary>
        [TestMethod]
        public void TestButterworthHomoBlur()
        {
            float gammaH = 2.0f;
            float gammaL = 0.2f;
            float sigma = 2f;
            float slope = 1f;
            using Mat matrix = Cv2.ImRead("Content/Images/Book.jpg", ImreadModes.Grayscale);
            using Mat result = matrix.ButterworthHomoBlur(gammaH, gammaL, sigma, slope);

            Cv2.ImShow("OpenCV巴特沃斯同态滤波-原图", matrix);
            Cv2.ImShow("OpenCV巴特沃斯同态滤波-效果图", result);
            Cv2.WaitKey();
        }
        #endregion
    }
}
