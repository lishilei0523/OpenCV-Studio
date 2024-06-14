#if NET462_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// WPF测试
    /// </summary>
    [TestClass]
    public class WPFTests
    {
        #region # 测试映射BitmapSource —— void TestToBitmapSource()
        /// <summary>
        /// 测试映射BitmapSource
        /// </summary>
        [TestMethod]
        public void TestToBitmapSource()
        {
            using Mat matrix = Cv2.ImRead("Content/Images/China.jpg");
            BitmapSource bitmapSource = matrix.ToBitmapSource();

            BitmapEncoder bitmapEncoder = new JpegBitmapEncoder();
            BitmapFrame bitmapFrame = BitmapFrame.Create(bitmapSource);
            bitmapEncoder.Frames.Add(bitmapFrame);
            using FileStream outputStream = File.OpenWrite("Content/Images/China.BS.jpg");
            bitmapEncoder.Save(outputStream);
        }
        #endregion

        #region # 测试映射OpenCV矩阵 —— void TestToMat()
        /// <summary>
        /// 测试映射OpenCV矩阵
        /// </summary>
        [TestMethod]
        public void TestToMat()
        {
            Uri imageUri = new Uri("Content/Images/China.jpg", UriKind.Relative);
            BitmapSource bitmapSource = new BitmapImage(imageUri);

            using Mat matrix = bitmapSource.ToMat();
            matrix.SaveImage("Content/Images/China.BS-MT.jpg");
        }
        #endregion
    }
}
#endif
