using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Results;
using SD.OpenCV.Primitives.Models;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SD.OpenCV.Reconstructions
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
        /// SuperPoint提取器
        /// </summary>
        private static SuperPoint _SuperPoint;

        /// <summary>
        /// SuperPoint匹配器
        /// </summary>
        private static SuperLightGlue _SuperLightGlue;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static Reconstructor()
        {
            _Initialized = false;
            _SuperPoint = null;
            _SuperLightGlue = null;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 是否已初始化 —— static bool Initialized
        /// <summary>
        /// 只读属性 - 是否已初始化
        /// </summary>
        public static bool Initialized
        {
            get => _Initialized;
        }
        #endregion

        #region 只读属性 - SuperPoint提取器 —— static SuperPoint SuperPoint
        /// <summary>
        /// 只读属性 - SuperPoint提取器
        /// </summary>
        public static SuperPoint SuperPoint
        {
            get => _SuperPoint;
        }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— static void Initialize()
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

            const string superPointPath = "Content/Models/superpoint.onnx";
            const string superLightGluePath = "Content/Models/superpoint_lightglue.onnx";
            _SuperPoint = new SuperPoint(superPointPath);
            _SuperLightGlue = new SuperLightGlue(superLightGluePath);
            _SuperPoint.StartSession();
            _SuperLightGlue.StartSession();
            _Initialized = true;
        }
        #endregion

        #region 初始化 —— static void Initialize(SuperPoint superPoint, SuperLightGlue...
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="superPoint">SuperPoint提取器</param>
        /// <param name="superLightGlue">SuperPoint匹配器</param>
        public static void Initialize(SuperPoint superPoint, SuperLightGlue superLightGlue)
        {
            #region # 验证

            if (_Initialized)
            {
                throw new InvalidOperationException("重建器已初始化，不可重复初始化！");
            }

            #endregion

            _SuperPoint = superPoint;
            _SuperLightGlue = superLightGlue;
            _Initialized = true;
        }
        #endregion

        #region 匹配图像 —— static MatchResult Match(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 匹配图像
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="threshold">匹配阈值</param>
        /// <returns>匹配结果</returns>
        public static MatchResult Match(Mat sourceImage, Mat targetImage, float threshold = 0.9f)
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

            #endregion

            //推理匹配
            Feature[] sourceFeatures = _SuperPoint.Infer(sourceImage, 0);
            Feature[] targetFeatures = _SuperPoint.Infer(targetImage, 0);
            DMatch[] matches = _SuperLightGlue.Infer((sourceFeatures, targetFeatures), threshold);

            //解析匹配结果
            IList<KeyPoint> sourceKeyPoints = sourceFeatures.Select(x => new KeyPoint(x.ScaledKeyPoint, 2)).ToList();
            IList<KeyPoint> targetKeyPoints = targetFeatures.Select(x => new KeyPoint(x.ScaledKeyPoint, 2)).ToList();
            MatchResult matchResult = matches.ResolveMatchResult(sourceKeyPoints, targetKeyPoints);

            return matchResult;
        }
        #endregion

        #region 重建图像 —— static Mat RecoverImage(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 重建图像
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="threshold">匹配阈值</param>
        /// <returns>重建后源图像</returns>
        public static Mat RecoverImage(Mat sourceImage, Mat targetImage, float threshold = 0.9f)
        {
            #region # 验证

            if (!_Initialized)
            {
                throw new InvalidOperationException("重建器未初始化，请先初始化！");
            }

            #endregion

            //解析匹配结果
            MatchResult matchResult = Match(sourceImage, targetImage, threshold);
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            //计算单应矩阵
            using InputArray sourcePoints = InputArray.Create(matchedSourcePoints);
            using InputArray targetPoints = InputArray.Create(matchedTargetPoints);
            using Mat homoMatrix = Cv2.FindHomography(sourcePoints, targetPoints, HomographyMethods.Ransac);

            //透视变换源图像
            Mat result = new Mat();
            Cv2.WarpPerspective(sourceImage, result, homoMatrix, sourceImage.Size());

            return result;
        }
        #endregion

        #region 重建图像 —— static Mat RecoverImage(Mat sourceImage, MatchResult matchResult)
        /// <summary>
        /// 重建图像
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="matchResult">匹配结果</param>
        /// <returns>重建后源图像</returns>
        public static Mat RecoverImage(Mat sourceImage, MatchResult matchResult)
        {
            //解析匹配结果
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            //计算单应矩阵
            using InputArray sourcePoints = InputArray.Create(matchedSourcePoints);
            using InputArray targetPoints = InputArray.Create(matchedTargetPoints);
            using Mat homoMatrix = Cv2.FindHomography(sourcePoints, targetPoints, HomographyMethods.Ransac);

            //透视变换源图像
            Mat result = new Mat();
            Cv2.WarpPerspective(sourceImage, result, homoMatrix, sourceImage.Size());

            return result;
        }
        #endregion

        #region 重建位姿 —— static double[,] RecoverPose(Mat sourceImage, Mat targetImage...
        /// <summary>
        /// 重建位姿
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="targetImage">目标图像</param>
        /// <param name="cameraMatrix">相机内参矩阵</param>
        /// <param name="threshold">匹配阈值</param>
        /// <returns>旋转平移矩阵: 4x4二维数组</returns>
        public static double[,] RecoverPose(Mat sourceImage, Mat targetImage, double[,] cameraMatrix, float threshold = 0.9f)
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
            MatchResult matchResult = Match(sourceImage, targetImage, threshold);
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            using InputArray sourcePoints = InputArray.Create(matchedSourcePoints);
            using InputArray targetPoints = InputArray.Create(matchedTargetPoints);

            //计算本征矩阵
            using Mat cameraMat = Mat.FromArray(cameraMatrix);
            using Mat essentialMat = Cv2.FindEssentialMat(sourcePoints, targetPoints, cameraMat);

            //计算RT矩阵
            using Mat rMat = new Mat();
            using Mat tMat = new Mat();
            Cv2.RecoverPose(essentialMat, sourcePoints, targetPoints, cameraMat, rMat, tMat);
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

        #region 重建位姿 —— static double[,] RecoverPose(Mat sourceImage, double[,] cameraMatrix...
        /// <summary>
        /// 重建位姿
        /// </summary>
        /// <param name="sourceImage">源图像</param>
        /// <param name="cameraMatrix">相机内参矩阵</param>
        /// <param name="matchResult">匹配结果</param>
        /// <returns>旋转平移矩阵: 4x4二维数组</returns>
        public static double[,] RecoverPose(Mat sourceImage, double[,] cameraMatrix, MatchResult matchResult)
        {
            #region # 验证

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
            Point2f[] matchedSourcePoints = matchResult.GetMatchedSourcePoints();
            Point2f[] matchedTargetPoints = matchResult.GetMatchedTargetPoints();

            using InputArray sourcePoints = InputArray.Create(matchedSourcePoints);
            using InputArray targetPoints = InputArray.Create(matchedTargetPoints);

            //计算本征矩阵
            using Mat cameraMat = Mat.FromArray(cameraMatrix);
            using Mat essentialMat = Cv2.FindEssentialMat(sourcePoints, targetPoints, cameraMat);

            //计算RT矩阵
            using Mat rMat = new Mat();
            using Mat tMat = new Mat();
            Cv2.RecoverPose(essentialMat, sourcePoints, targetPoints, cameraMat, rMat, tMat);
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

        #region 解析匹配结果 —— static MatchResult ResolveMatchResult(this DMatch[] matches...
        /// <summary>
        /// 解析匹配结果
        /// </summary>
        /// <param name="matches">OpenCV匹配结果集</param>
        /// <param name="sourceKeyPoints">源关键点集</param>
        /// <param name="targetKeyPoints">目标关键点集</param>
        /// <returns>匹配结果</returns>
        public static MatchResult ResolveMatchResult(this DMatch[] matches, IList<KeyPoint> sourceKeyPoints, IList<KeyPoint> targetKeyPoints)
        {
            #region # 验证

            matches ??= Array.Empty<DMatch>();
            sourceKeyPoints ??= new List<KeyPoint>();
            targetKeyPoints ??= new List<KeyPoint>();
            if (!matches.Any() || !sourceKeyPoints.Any() || !targetKeyPoints.Any())
            {
                return new MatchResult(0, matches, sourceKeyPoints, targetKeyPoints, new Dictionary<int, KeyPoint>(), new Dictionary<int, KeyPoint>());
            }

            #endregion

            IDictionary<int, KeyPoint> matchedSourceKeyPoints = new Dictionary<int, KeyPoint>();
            IDictionary<int, KeyPoint> matchedTargetKeyPoints = new Dictionary<int, KeyPoint>();
            foreach (DMatch goodMatch in matches)
            {
                matchedSourceKeyPoints.Add(goodMatch.QueryIdx, sourceKeyPoints.ElementAt(goodMatch.QueryIdx));
                matchedTargetKeyPoints.Add(goodMatch.TrainIdx, targetKeyPoints.ElementAt(goodMatch.TrainIdx));
            }

            MatchResult matchResult = new MatchResult(matches.Length, matches, sourceKeyPoints, targetKeyPoints, matchedSourceKeyPoints, matchedTargetKeyPoints);

            return matchResult;
        }
        #endregion

        #endregion
    }
}
