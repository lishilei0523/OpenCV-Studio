using OpenCvSharp;
using SD.OpenCV.Primitives.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Reconstructions
{
    /// <summary>
    /// Super特征工程
    /// </summary>
    public static class SuperEngineer
    {
        #region # 解析匹配结果 —— static MatchResult ResolveMatchResult(this DMatch[] matches...
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

            MatchResult matchResult = new MatchResult(matches.Count(), matches, sourceKeyPoints, targetKeyPoints, matchedSourceKeyPoints, matchedTargetKeyPoints);

            return matchResult;
        }
        #endregion

        #region # 获取图像特征 —— static float[] GetImageFeatures(this Mat image)
        /// <summary>
        /// 获取图像特征
        /// </summary>
        /// <param name="image">图像</param>
        /// <returns>图像特征数组</returns>
        public static unsafe float[] GetImageFeatures(this Mat image)
        {
            Mat image32F;
            if (image.Type() != MatType.CV_32FC1)
            {
                image32F = new Mat();
                image.ConvertTo(image32F, MatType.CV_32FC1);
            }
            else
            {
                image32F = image.Clone();
            }

            ConcurrentBag<float> imageFeatures = new ConcurrentBag<float>();
            image32F.ForEachAsFloat((valuePtr, _) =>
            {
                float value = *valuePtr;
                float normalValue = value / 255.0f;
                imageFeatures.Add(normalValue);
            });

            //释放资源
            image32F.Dispose();

            return imageFeatures.ToArray();
        }
        #endregion

        #region # 获取关键点特征 —— static float[] GetKeyPointsFeatures(IEnumerable<long> keyPoints)
        /// <summary>
        /// 获取关键点特征
        /// </summary>
        /// <param name="keyPoints">关键点列表</param>
        /// <returns>关键点特征数组</returns>
        public static float[] GetKeyPointsFeatures(IEnumerable<long> keyPoints)
        {
            #region # 验证

            keyPoints = keyPoints?.ToArray() ?? Array.Empty<long>();
            if (!keyPoints.Any())
            {
                return Array.Empty<float>();
            }

            #endregion

            float[] keyPointsFeatures = new float[keyPoints.Count()];
            for (int index = 0; index < keyPoints.Count(); index++)
            {
                keyPointsFeatures[index] = (keyPoints.ElementAt(index) - 256) / 256.0f;
            }

            return keyPointsFeatures;
        }
        #endregion
    }
}
