using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.Primitives.Extensions;
using SD.OpenCV.Reconstructions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// 特征测试
    /// </summary>
    [TestClass]
    public class FeatureTests
    {
        #region # 测试计算特征 —— void TestComputeFeatures()
        /// <summary>
        /// 测试计算特征
        /// </summary>
        [TestMethod]
        public void TestComputeFeatures()
        {
            //读取图片
            string imagePath = "Content/Images/Faw/001.bmp";
            using Mat image = Cv2.ImRead(imagePath);

            //计算关键点与描述子
            using SuperFeature feature = new SuperFeature();
            using Mat descriptors = new Mat();
            feature.DetectAndCompute(image, null, out KeyPoint[] keyPoints, descriptors);

            Trace.WriteLine($"图像: {Path.GetFileNameWithoutExtension(imagePath)}");
            Trace.WriteLine($"关键点数量: {keyPoints.Length}");
            Trace.WriteLine($"描述子行数: {descriptors.Rows}");
            Trace.WriteLine($"描述子列数: {descriptors.Cols}");
        }
        #endregion

        #region # 测试匹配特征 —— void TestMatchFeatures()
        /// <summary>
        /// 测试匹配特征
        /// </summary>
        [TestMethod]
        public void TestMatchFeatures()
        {
            //读取图像
            using Mat image1 = Cv2.ImRead("Content/Images/Steel-1.jpg");
            using Mat image2 = Cv2.ImRead("Content/Images/Steel-2.jpg");

            //缩放图像
            int scaledSize = 512;
            using Mat scaledImage1 = image1.ResizeAdaptively(scaledSize);
            using Mat scaledImage2 = image2.ResizeAdaptively(scaledSize);

            //定义特征提取器与匹配器
            using SuperFeature feature = new SuperFeature();
            using SuperMatcher matcher = new SuperMatcher();

            //推理匹配
            float threshold = 0.9f;
            feature.ComputeAll(scaledImage1, null, out long[] sourceKptsArray, out int[] sourceKptsDims, out float[] sourceDescArray, out int[] sourceDescDims, out KeyPoint[] srcKeyPoints, out Mat srcDecriptors);
            feature.ComputeAll(scaledImage2, null, out long[] targetKptsArray, out int[] targetKptsDims, out float[] targetDescArray, out int[] targetDescDims, out KeyPoint[] tgtKeyPoints, out Mat tgtDecriptors);
            DMatch[] matches = matcher.Match(threshold, sourceKptsArray, sourceKptsDims, sourceDescArray, sourceDescDims, targetKptsArray, targetKptsDims, targetDescArray, targetDescDims);

            Trace.WriteLine($"阈值: {threshold}");
            Trace.WriteLine($"匹配关键点数量: {matches.Length}");

            //缩放关键点
            ICollection<KeyPoint> scaledSrcKpts = srcKeyPoints.ScaleKeyPoints(image1.Width, image1.Height, scaledSize);
            ICollection<KeyPoint> scaledTgtKpts = tgtKeyPoints.ScaleKeyPoints(image1.Width, image1.Height, scaledSize);

            //绘制匹配结果、保存图片
            using Mat outImage = new Mat();
            Cv2.DrawMatches(image1, scaledSrcKpts, image2, scaledTgtKpts, matches, outImage);
            Cv2.ImWrite("Content/MatchResult.jpg", outImage);
            Trace.WriteLine("图像已保存");

            //释放资源
            srcDecriptors.Dispose();
            tgtDecriptors.Dispose();
        }
        #endregion
    }
}
