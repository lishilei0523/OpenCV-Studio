using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Extensions
{
    /// <summary>
    /// 关键点扩展
    /// </summary>
    public static class KeyPointExtension
    {
        #region # 检测Harris关键点 —— static Point[] DetectHarris(this Mat matrix, int blockSize...
        /// <summary>
        /// 检测Harris关键点
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="blockSize">块尺寸</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="k">自由参数</param>
        /// <returns>关键点列表</returns>
        public static unsafe Point[] DetectHarris(this Mat matrix, int blockSize, int kernelSize, double k)
        {
            //计算角点矩阵
            using Mat corneredResult = new Mat();
            Cv2.CornerHarris(matrix, corneredResult, blockSize, kernelSize, k);

            using Mat normalizedResult = new Mat();
            Cv2.Normalize(corneredResult, normalizedResult, 0, 255, NormTypes.MinMax);

            using Mat scaledResult = new Mat();
            Cv2.ConvertScaleAbs(normalizedResult, scaledResult);

            //整理角点
            ConcurrentBag<Point> points = new ConcurrentBag<Point>();
            scaledResult.ForEachAsByte((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                byte value = *valuePtr;
                if (value > 125)
                {
                    Point center = new Point(colIndex, rowIndex);
                    points.Add(center);
                }
            });

            return points.ToArray();
        }
        #endregion

        #region # 缩放关键点列表 —— static IList<KeyPoint> ScaleKeyPoints(this IEnumerable<KeyPoint>...
        /// <summary>
        /// 缩放关键点列表
        /// </summary>
        /// <param name="keyPoints">关键点列表</param>
        /// <param name="sourceWidth">原图像宽度</param>
        /// <param name="sourceHeight">原图像高度</param>
        /// <param name="scaledWidth">缩放图像宽度</param>
        /// <param name="scaledHeight">缩放图像高度</param>
        /// <param name="paddingX">X轴内边距</param>
        /// <param name="paddingY">Y轴内边距</param>
        /// <returns>缩放关键点列表</returns>
        /// <remarks>缩放为正方形缩放，缩放尺寸为目标边长</remarks>
        public static IList<KeyPoint> ScaleKeyPoints(this IEnumerable<KeyPoint> keyPoints, int sourceWidth,
            int sourceHeight, int scaledWidth, int scaledHeight, int paddingX, int paddingY)
        {
            #region # 验证

            keyPoints = keyPoints?.ToArray() ?? Array.Empty<KeyPoint>();
            if (!keyPoints.Any())
            {
                return Array.Empty<KeyPoint>();
            }

            #endregion

            float scaleX = sourceWidth * 1.0f / scaledWidth;
            float scaleY = sourceHeight * 1.0f / scaledHeight;
            IList<KeyPoint> scaledKeyPoints = new List<KeyPoint>();
            foreach (KeyPoint keyPoint in keyPoints)
            {
                Point2f scaledPoint = new Point2f(keyPoint.Pt.X, keyPoint.Pt.Y);
                scaledPoint.X -= paddingX;
                scaledPoint.Y -= paddingY;
                scaledPoint.X *= scaleX;
                scaledPoint.Y *= scaleY;

                KeyPoint scaledKeyPoint = new KeyPoint(scaledPoint, 0);
                scaledKeyPoints.Add(scaledKeyPoint);
            }

            return scaledKeyPoints;
        }
        #endregion
    }
}
