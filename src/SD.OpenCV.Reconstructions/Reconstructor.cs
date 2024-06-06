using OpenCvSharp;
using SD.Toolkits.OpenCV.Extensions;
using SD.Toolkits.OpenCV.Models;
using System;
using System.Collections.Generic;

namespace SD.Toolkits.OpenCV.Reconstructions
{
    /// <summary>
    /// 重建器
    /// </summary>
    public static class Reconstructor
    {
        #region # 字段及构造器

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _Initialized;

        /// <summary>
        /// 特征提取器
        /// </summary>
        private static SuperFeature _Feature;

        /// <summary>
        /// 特征匹配器
        /// </summary>
        private static SuperMatcher _Matcher;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static Reconstructor()
        {
            _Initialized = false;
            _Feature = null;
            _Matcher = null;
        }

        #endregion

        #region # 初始化 —— static void Initialize()
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            #region # 验证

            if (_Initialized)
            {
                throw new InvalidOperationException("重建器已初始化，不可重复初始化！");
            }

            #endregion

            _Feature = new SuperFeature();
            _Matcher = new SuperMatcher();
            _Initialized = true;
        }
        #endregion

        #region # 初始化 —— static void Initialize(SuperFeature feature, SuperMatcher matcher)
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="feature">特征提取器</param>
        /// <param name="matcher">特征匹配器</param>
        public static void Initialize(SuperFeature feature, SuperMatcher matcher)
        {
            #region # 验证

            if (_Initialized)
            {
                throw new InvalidOperationException("重建器已初始化，不可重复初始化！");
            }

            #endregion

            _Feature = feature;
            _Matcher = matcher;
            _Initialized = true;
        }
        #endregion

        #region # 匹配图像 —— static MatchResult Match(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 匹配图像
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="threshold">匹配阈值</param>
        /// <param name="scaledSize">图像缩放尺寸</param>
        /// <returns>旋转平移矩阵: 4x4二维数组</returns>
        public static MatchResult Match(Mat sourceImage, Mat targetImage, float threshold = 0.9f, int scaledSize = 512)
        {
            #region # 验证

            if (!_Initialized)
            {
                throw new InvalidOperationException("重建器未初始化，请先初始化！");
            }
            if (sourceImage == null)
            {
                throw new ArgumentNullException(nameof(sourceImage), "源图像不可为空！");
            }
            if (targetImage == null)
            {
                throw new ArgumentNullException(nameof(targetImage), "目标图像不可为空！");
            }
            if (!(sourceImage.Rows == targetImage.Rows && sourceImage.Cols == targetImage.Cols))
            {
                throw new InvalidOperationException("源图像与目标图像尺寸不一致！");
            }

            #endregion

            //缩放图像
            using Mat scaledSourceImage = sourceImage.ResizeAdaptively(scaledSize);
            using Mat scaledReferenceImage = targetImage.ResizeAdaptively(scaledSize);

            //推理匹配
            _Feature.ComputeAll(scaledSourceImage, null, out long[] sourceKptsArray, out int[] sourceKptsDims, out float[] sourceDescArray, out int[] sourceDescDims, out KeyPoint[] srcKeyPoints, out Mat srcDecriptors);
            _Feature.ComputeAll(scaledReferenceImage, null, out long[] targetKptsArray, out int[] targetKptsDims, out float[] targetDescArray, out int[] targetDescDims, out KeyPoint[] tgtKeyPoints, out Mat tgtDecriptors);
            DMatch[] matches = _Matcher.Match(threshold, sourceKptsArray, sourceKptsDims, sourceDescArray, sourceDescDims, targetKptsArray, targetKptsDims, targetDescArray, targetDescDims);

            //关键点缩放
            IList<KeyPoint> scaledSrcKpts = srcKeyPoints.ScaleKeyPoints(sourceImage.Width, sourceImage.Height, scaledSize);
            IList<KeyPoint> scaledTgtKpts = tgtKeyPoints.ScaleKeyPoints(targetImage.Width, targetImage.Height, scaledSize);

            //解析匹配结果
            MatchResult matchResult = matches.ResolveMatchResult(scaledSrcKpts, scaledTgtKpts);

            //释放资源
            srcDecriptors.Dispose();
            tgtDecriptors.Dispose();

            return matchResult;
        }
        #endregion

        #region # 重建图像 —— static Mat RecoverImage(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 重建图像
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="threshold">匹配阈值</param>
        /// <param name="scaledSize">图像缩放尺寸</param>
        /// <returns>重建后源图像</returns>
        public static Mat RecoverImage(Mat sourceImage, Mat targetImage, float threshold = 0.9f, int scaledSize = 512)
        {
            #region # 验证

            if (!_Initialized)
            {
                throw new InvalidOperationException("重建器未初始化，请先初始化！");
            }

            #endregion

            //解析匹配结果
            MatchResult matchResult = Match(sourceImage, targetImage, threshold, scaledSize);
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            //计算单应矩阵
            using Mat homoMat = Cv2.FindHomography(InputArray.Create(matchedSourcePoints), InputArray.Create(matchedTargetPoints), HomographyMethods.Ransac);

            //透射变换源图像
            Mat result = new Mat();
            Cv2.WarpPerspective(sourceImage, result, homoMat, sourceImage.Size());

            return result;
        }
        #endregion

        #region # 重建位姿 —— static double[,] RecoverPose(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 重建位姿
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="cameraMatrix">相机内参矩阵</param>
        /// <param name="threshold">匹配阈值</param>
        /// <param name="scaledSize">图像缩放尺寸</param>
        /// <returns>旋转平移矩阵: 4x4二维数组</returns>
        public static double[,] RecoverPose(Mat sourceImage, Mat targetImage, double[,] cameraMatrix, float threshold = 0.9f, int scaledSize = 512)
        {
            #region # 验证

            if (!_Initialized)
            {
                throw new InvalidOperationException("重建器未初始化，请先初始化！");
            }
            if (cameraMatrix == null)
            {
                throw new ArgumentNullException(nameof(cameraMatrix), "相机内参矩阵不可为空！");
            }
            if (!(cameraMatrix.Rank == 2 && cameraMatrix.GetLength(0) == 3 && cameraMatrix.GetLength(1) == 3))
            {
                throw new InvalidOperationException("相机内参矩阵必须为3x3矩阵！");
            }

            #endregion

            //解析匹配结果
            MatchResult matchResult = Match(sourceImage, targetImage, threshold, scaledSize);
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            //计算本征矩阵
            using Mat cameraMat = Mat.FromArray(cameraMatrix);
            using Mat essentialMat = Cv2.FindEssentialMat(InputArray.Create(matchedSourcePoints), InputArray.Create(matchedTargetPoints), cameraMat);

            //计算RT矩阵
            using Mat rMat = new Mat();
            using Mat tMat = new Mat();
            Cv2.RecoverPose(essentialMat, InputArray.Create(matchedSourcePoints), InputArray.Create(matchedTargetPoints), cameraMat, rMat, tMat);
            double[,] rtArray4x4 =
            {
                {rMat.At<double>(0, 0), rMat.At<double>(0, 1), rMat.At<double>(0, 2), tMat.At<double>(0, 0)},
                {rMat.At<double>(1, 0), rMat.At<double>(1, 1), rMat.At<double>(1, 2), tMat.At<double>(1, 0)},
                {rMat.At<double>(2, 0), rMat.At<double>(2, 1), rMat.At<double>(2, 2), tMat.At<double>(2, 0)},
                {0, 0, 0, 1}
            };

            return rtArray4x4;
        }
        #endregion
    }
}
