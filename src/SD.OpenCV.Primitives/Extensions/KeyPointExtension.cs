using OpenCvSharp;
using System.Collections.Concurrent;

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
    }
}
