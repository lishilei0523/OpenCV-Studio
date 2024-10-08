﻿using OpenCvSharp;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Models
{
    /// <summary>
    /// 匹配结果
    /// </summary>
    public class MatchResult
    {
        #region # 构造器

        #region 00.无参构造器
        /// <summary>
        /// 无参构造器
        /// </summary>
        private MatchResult() { }
        #endregion

        #region 01.创建匹配结果构造器
        /// <summary>
        /// 创建匹配结果构造器
        /// </summary>
        /// <param name="matchedCount">匹配数量</param>
        /// <param name="dMatches">匹配索引列表</param>
        /// <param name="sourceKeyPoints">源关键点列表</param>
        /// <param name="targetKeyPoints">目标关键点列表</param>
        /// <param name="matchedSourceKeyPoints">匹配的源关键点字典</param>
        /// <param name="matchedTargetKeyPoints">匹配的目标关键点字典</param>
        public MatchResult(int matchedCount, DMatch[] dMatches, IList<KeyPoint> sourceKeyPoints, IList<KeyPoint> targetKeyPoints, IDictionary<int, KeyPoint> matchedSourceKeyPoints, IDictionary<int, KeyPoint> matchedTargetKeyPoints)
            : this()
        {
            this.MatchedCount = matchedCount;
            this.DMatches = dMatches;
            this.SourceKeyPoints = sourceKeyPoints;
            this.TargetKeyPoints = targetKeyPoints;
            this.MatchedSourceKeyPoints = matchedSourceKeyPoints;
            this.MatchedTargetKeyPoints = matchedTargetKeyPoints;
        }
        #endregion

        #endregion

        #region # 属性

        #region 匹配数量 —— int MatchedCount
        /// <summary>
        /// 匹配数量
        /// </summary>
        public int MatchedCount { get; private set; }
        #endregion

        #region 匹配索引列表 —— DMatch[] DMatches
        /// <summary>
        /// 匹配索引列表
        /// </summary>
        public DMatch[] DMatches { get; private set; }
        #endregion

        #region 源关键点列表 —— IList<KeyPoint> SourceKeyPoints
        /// <summary>
        /// 源关键点列表
        /// </summary>
        public IList<KeyPoint> SourceKeyPoints { get; private set; }
        #endregion

        #region 目标关键点列表 —— IList<KeyPoint> TargetKeyPoints
        /// <summary>
        /// 目标关键点列表
        /// </summary>
        public IList<KeyPoint> TargetKeyPoints { get; private set; }
        #endregion

        #region 匹配的源关键点字典 —— IDictionary<int, KeyPoint> MatchedSourceKeyPoints
        /// <summary>
        /// 匹配的源关键点字典
        /// </summary>
        /// <remarks>键：索引，值：关键点</remarks>
        public IDictionary<int, KeyPoint> MatchedSourceKeyPoints { get; private set; }
        #endregion

        #region 匹配的目标关键点字典 —— IDictionary<int, KeyPoint> MatchedTargetKeyPoints
        /// <summary>
        /// 匹配的目标关键点字典
        /// </summary>
        /// <remarks>键：索引，值：关键点</remarks>
        public IDictionary<int, KeyPoint> MatchedTargetKeyPoints { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 获取匹配的源坐标点列表 —— Point2f[] GetMatchedSourcePoints()
        /// <summary>
        /// 获取匹配的源坐标点列表
        /// </summary>
        /// <returns>坐标点列表</returns>
        public Point2f[] GetMatchedSourcePoints()
        {
            Point2f[] points = this.MatchedSourceKeyPoints.Values.Select(x => x.Pt).ToArray();

            return points;
        }
        #endregion

        #region 获取匹配的目标坐标点列表 —— Point2f[] GetMatchedTargetPoints()
        /// <summary>
        /// 获取匹配的目标坐标点列表
        /// </summary>
        /// <returns>坐标点列表</returns>
        public Point2f[] GetMatchedTargetPoints()
        {
            Point2f[] points = this.MatchedTargetKeyPoints.Values.Select(x => x.Pt).ToArray();

            return points;
        }
        #endregion

        #endregion
    }
}
