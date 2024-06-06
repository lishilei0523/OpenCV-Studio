using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 图像分割扩展
    /// </summary>
    public static class SegmentExtension
    {
        #region # 生成掩膜 —— static Mat GenerateMask(this Mat matrix, Rect rectangle)
        /// <summary>
        /// 生成掩膜
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">矩形</param>
        /// <returns>掩膜矩阵</returns>
        public static Mat GenerateMask(this Mat matrix, Rect rectangle)
        {
            Mat mask = Mat.Zeros(matrix.Size(), MatType.CV_8UC1);
            mask[rectangle].SetTo(255);

            return mask;
        }
        #endregion

        #region # 生成掩膜 —— static Mat GenerateMask(this Mat matrix, IEnumerable<Point> contourPoints)
        /// <summary>
        /// 生成掩膜
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <returns>掩膜矩阵</returns>
        public static Mat GenerateMask(this Mat matrix, IEnumerable<Point> contourPoints)
        {
            #region # 验证

            contourPoints = contourPoints?.Distinct().ToArray() ?? Array.Empty<Point>();
            if (!contourPoints.Any())
            {
                throw new ArgumentNullException(nameof(contourPoints), "轮廓坐标点集不可为空！");
            }

            #endregion

            Mat mask = Mat.Zeros(matrix.Size(), MatType.CV_8UC1);
            Cv2.DrawContours(mask, new[] { contourPoints }, 0, Scalar.White, -1);

            return mask;
        }
        #endregion

        #region # 适用掩膜 —— static Mat ApplyMask(this Mat matrix, Rect rectangle)
        /// <summary>
        /// 适用掩膜
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">矩形</param>
        /// <returns>结果图像矩阵</returns>
        public static Mat ApplyMask(this Mat matrix, Rect rectangle)
        {
            using Mat mask = matrix.GenerateMask(rectangle);
            Mat result = new Mat();
            matrix.CopyTo(result, mask);

            return result;
        }
        #endregion

        #region # 适用掩膜 —— static Mat ApplyMask(this Mat matrix, IEnumerable<Point> contourPoints)
        /// <summary>
        /// 适用掩膜
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <returns>结果图像矩阵</returns>
        public static Mat ApplyMask(this Mat matrix, IEnumerable<Point> contourPoints)
        {
            using Mat mask = matrix.GenerateMask(contourPoints);
            Mat result = new Mat();
            matrix.CopyTo(result, mask);

            return result;
        }
        #endregion

        #region # 提取矩形内图像 —— static Mat ExtractMatrixInRectangle(this Mat matrix...
        /// <summary>
        /// 提取矩形内图像
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">矩形</param>
        /// <returns>矩形内图像矩阵</returns>
        public static Mat ExtractMatrixInRectangle(this Mat matrix, Rect rectangle)
        {
            //制作掩膜
            using Mat mask = matrix.GenerateMask(rectangle);

            //提取有效区域
            using Mat canvas = new Mat();
            matrix.CopyTo(canvas, mask);

            //矩形截取
            Mat result = canvas[rectangle];

            return result;
        }
        #endregion

        #region # 提取矩形内像素 —— static IEnumerable<byte> ExtractPixelsInRectangle(this Mat matrix...
        /// <summary>
        /// 提取矩形内像素
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">轮廓坐标点集</param>
        /// <returns>矩形内像素列表</returns>
        public static unsafe IEnumerable<byte> ExtractPixelsInRectangle(this Mat matrix, Rect rectangle)
        {
            //制作掩膜
            using Mat mask = matrix.GenerateMask(rectangle);

            //提取有效区域
            using Mat canvas = new Mat();
            matrix.CopyTo(canvas, mask);

            //有效像素点
            ConcurrentBag<byte> availablePixels = new ConcurrentBag<byte>();
            using Mat result = canvas[rectangle];
            result.ForEachAsByte((valuePtr, positionPtr) =>
            {
                byte pixelValue = *valuePtr;
                availablePixels.Add(pixelValue);
            });

            return availablePixels;
        }
        #endregion

        #region # 提取轮廓内图像 —— static Mat ExtractMatrixInContour(this Mat matrix...
        /// <summary>
        /// 提取轮廓内图像
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <returns>轮廓内图像矩阵</returns>
        public static Mat ExtractMatrixInContour(this Mat matrix, IEnumerable<Point> contourPoints)
        {
            #region # 验证

            contourPoints = contourPoints?.Distinct().ToArray() ?? Array.Empty<Point>();
            if (!contourPoints.Any())
            {
                throw new ArgumentNullException(nameof(contourPoints), "轮廓坐标点集不可为空！");
            }

            #endregion

            //制作掩膜
            using Mat mask = matrix.GenerateMask(contourPoints);

            //提取有效区域
            using Mat canvas = new Mat();
            matrix.CopyTo(canvas, mask);

            //外接矩形截取
            Rect boundingRect = Cv2.BoundingRect(contourPoints);
            Mat result = canvas[boundingRect];

            return result;
        }
        #endregion

        #region # 提取轮廓内像素 —— static IEnumerable<byte> ExtractPixelsInContour(this Mat matrix...
        /// <summary>
        /// 提取轮廓内像素
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <returns>轮廓内像素列表</returns>
        public static unsafe IEnumerable<byte> ExtractPixelsInContour(this Mat matrix, IEnumerable<Point> contourPoints)
        {
            #region # 验证

            contourPoints = contourPoints?.Distinct().ToArray() ?? Array.Empty<Point>();
            if (!contourPoints.Any())
            {
                throw new ArgumentNullException(nameof(contourPoints), "轮廓坐标点集不可为空！");
            }

            #endregion

            //制作掩膜
            using Mat mask = matrix.GenerateMask(contourPoints);

            //提取有效区域
            using Mat canvas = new Mat();
            matrix.CopyTo(canvas, mask);

            //有效像素点
            ConcurrentBag<byte> availablePixels = new ConcurrentBag<byte>();
            canvas.ForEachAsByte((valuePtr, positionPtr) =>
            {
                byte pixelValue = *valuePtr;
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Point2f point = new Point2f(rowIndex, colIndex);
                double distance = Cv2.PointPolygonTest(contourPoints, point, false);
                if (!distance.Equals(-1))
                {
                    availablePixels.Add(pixelValue);
                }
            });

            return availablePixels;
        }
        #endregion

        #region # 颜色分割 —— static Mat ColorSegment(this Mat hsvMatrix, Scalar lowerScalar...
        /// <summary>
        /// 颜色分割
        /// </summary>
        /// <param name="hsvMatrix">HSV图像矩阵</param>
        /// <param name="lowerScalar">颜色下限</param>
        /// <param name="upperScalar">颜色上限</param>
        /// <returns>分割结果图像矩阵</returns>
        public static Mat ColorSegment(this Mat hsvMatrix, Scalar lowerScalar, Scalar upperScalar)
        {
            using Mat mask = new Mat();
            Cv2.InRange(hsvMatrix, lowerScalar, upperScalar, mask);

            Mat result = new Mat();
            Cv2.BitwiseAnd(hsvMatrix, hsvMatrix, result, mask);
            result = result.CvtColor(ColorConversionCodes.HSV2BGR);

            return result;
        }
        #endregion

        #region # 滑动窗口 —— static void SlideWindow(this Mat matrix, int windowWidth, int windowHeight...
        /// <summary>
        /// 滑动窗口
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="windowWidth">窗口宽度</param>
        /// <param name="windowHeight">窗口高度</param>
        /// <param name="step">步长</param>
        /// <param name="action">操作</param>
        public static void SlideWindow(this Mat matrix, int windowWidth, int windowHeight, int step, Action<Rect, Mat> action)
        {
            for (int colIndex = 0; colIndex < matrix.Cols - step; colIndex += step)
            {
                for (int rowIndex = 0; rowIndex < matrix.Rows - step; rowIndex += step)
                {
                    int x = colIndex;
                    int y = rowIndex;
                    if (x + windowWidth > matrix.Width)
                    {
                        x -= x + windowWidth - matrix.Width;
                    }
                    if (y + windowHeight > matrix.Height)
                    {
                        y -= y + windowHeight - matrix.Height;
                    }

                    Rect window = new Rect(x, y, windowWidth, windowHeight);
                    Mat roi = matrix[window];
                    action.Invoke(window, roi);
                }
            }
        }
        #endregion
    }
}
