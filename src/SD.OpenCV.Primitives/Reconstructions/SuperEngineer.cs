using OpenCvSharp;
using SD.Toolkits.OpenCV.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.Toolkits.OpenCV.Reconstructions
{
    /// <summary>
    /// Super特征工程
    /// </summary>
    public static class SuperEngineer
    {
        #region # 解析匹配结果 —— static MatchResult ResolveMatchResult(this IEnumerable<DMatch> matches...
        /// <summary>
        /// 解析匹配结果
        /// </summary>
        /// <param name="matches">OpenCV匹配结果集</param>
        /// <param name="sourceKeyPoints">源关键点集</param>
        /// <param name="targetKeyPoints">目标关键点集</param>
        /// <returns>匹配结果</returns>
        public static MatchResult ResolveMatchResult(this IEnumerable<DMatch> matches, IEnumerable<KeyPoint> sourceKeyPoints, IEnumerable<KeyPoint> targetKeyPoints)
        {
            #region # 验证

            matches = matches?.ToArray() ?? Array.Empty<DMatch>();
            sourceKeyPoints = sourceKeyPoints?.ToArray() ?? Array.Empty<KeyPoint>();
            targetKeyPoints = targetKeyPoints?.ToArray() ?? Array.Empty<KeyPoint>();
            if (!matches.Any() || !sourceKeyPoints.Any() || !targetKeyPoints.Any())
            {
                return new MatchResult(0, new Dictionary<int, KeyPoint>(), new Dictionary<int, KeyPoint>());
            }

            #endregion

            IDictionary<int, KeyPoint> matchedSourceKeyPoints = new Dictionary<int, KeyPoint>();
            IDictionary<int, KeyPoint> matchedTargetKeyPoints = new Dictionary<int, KeyPoint>();
            foreach (DMatch goodMatch in matches)
            {
                matchedSourceKeyPoints.Add(goodMatch.QueryIdx, sourceKeyPoints.ElementAt(goodMatch.QueryIdx));
                matchedTargetKeyPoints.Add(goodMatch.TrainIdx, targetKeyPoints.ElementAt(goodMatch.TrainIdx));
            }

            MatchResult matchResult = new MatchResult(matches.Count(), matchedSourceKeyPoints, matchedTargetKeyPoints);

            return matchResult;
        }
        #endregion

        #region # 匹配结果转DMatch列表 —— static ICollection<DMatch> ToDMatches(this MatchResult matchResult)
        /// <summary>
        /// 匹配结果转DMatch列表
        /// </summary>
        /// <param name="matchResult">匹配结果</param>
        /// <returns>DMatch列表</returns>
        public static ICollection<DMatch> ToDMatches(this MatchResult matchResult)
        {
            IList<DMatch> dMatches = new List<DMatch>();
            for (int index = 0; index < matchResult.MatchedCount; index++)
            {
                KeyValuePair<int, KeyPoint> srcKv = matchResult.MatchedSourceKeyPoints.ElementAt(index);
                KeyValuePair<int, KeyPoint> tgtKv = matchResult.MatchedTargetKeyPoints.ElementAt(index);
                DMatch dMatch = new DMatch(srcKv.Key, tgtKv.Key, 0);
                dMatches.Add(dMatch);
            }

            return dMatches;
        }
        #endregion

        #region # 获取图像特征 —— static float[] GetImageFeatures(this Mat image)
        /// <summary>
        /// 获取图像特征
        /// </summary>
        /// <param name="image">图像</param>
        /// <returns>图像特征数组</returns>
        public static float[] GetImageFeatures(this Mat image)
        {
            Mat image32FC1;
            if (image.Type() != MatType.CV_32FC1)
            {
                image32FC1 = new Mat();
                image.ConvertTo(image32FC1, MatType.CV_32FC1);
            }
            else
            {
                image32FC1 = image.Clone();
            }

            IList<float> imageFeatures = new List<float>();
            for (int rowIndex = 0; rowIndex < image.Rows; rowIndex++)
            {
                for (int colIndex = 0; colIndex < image.Cols; colIndex++)
                {
                    imageFeatures.Add(image32FC1.At<float>(rowIndex, colIndex) / 255.0f);
                }
            }

            //释放资源
            image32FC1.Dispose();

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
