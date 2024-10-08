﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Extensions
{
    /// <summary>
    /// 几何变换扩展
    /// </summary>
    public static class GeometryExtension
    {
        #region # 绝对缩放 —— static Mat ResizeAbsolutely(this Mat matrix, int width...
        /// <summary>
        /// 绝对缩放
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="width">目标宽度</param>
        /// <param name="height">目标高度</param>
        /// <param name="mode">插值模式</param>
        /// <returns>缩放后图像矩阵</returns>
        public static Mat ResizeAbsolutely(this Mat matrix, int width, int height, InterpolationFlags mode = InterpolationFlags.Area)
        {
            Mat result = new Mat();
            Cv2.Resize(matrix, result, new Size(width, height), 0, 0, mode);

            return result;
        }
        #endregion

        #region # 相对缩放 —— static Mat ResizeRelatively(this Mat matrix, float scale...
        /// <summary>
        /// 相对缩放
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="scale">缩放尺度</param>
        /// <param name="mode">插值模式</param>
        /// <returns>缩放后图像矩阵</returns>
        public static Mat ResizeRelatively(this Mat matrix, float scale, InterpolationFlags mode = InterpolationFlags.Area)
        {
            Mat result = new Mat();
            Cv2.Resize(matrix, result, new Size(), scale, scale, mode);

            return result;
        }
        #endregion

        #region # 自适应缩放 —— static Mat ResizeAdaptively(this Mat matrix, int scaledSize...
        /// <summary>
        /// 自适应缩放
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="scaledSize">缩放尺寸</param>
        /// <param name="mode">插值模式</param>
        /// <returns>缩放后图像矩阵</returns>
        /// <remarks>缩放为正方形缩放，缩放尺寸为目标边长</remarks>
        public static Mat ResizeAdaptively(this Mat matrix, int scaledSize, InterpolationFlags mode = InterpolationFlags.Area)
        {
            using Mat scaledImage = new Mat();
            Mat borderedImage = new Mat();
            if (matrix.Width > matrix.Height)
            {
                int width = scaledSize;
                int height = (int)Math.Ceiling(scaledSize * 1.0 / matrix.Width * matrix.Height);
                Size size = new Size(width, height);
                Cv2.Resize(matrix, scaledImage, size, 0, 0, mode);

                int surplusY = (width - height) / 2;
                Cv2.CopyMakeBorder(scaledImage, borderedImage, surplusY, surplusY, 0, 0, BorderTypes.Constant);
            }
            else
            {
                int height = scaledSize;
                int width = (int)Math.Ceiling(scaledSize * 1.0 / matrix.Height * matrix.Width);
                Size size = new Size(width, height);
                Cv2.Resize(matrix, scaledImage, size, 0, 0, mode);

                int surplusX = (height - width) / 2;
                Cv2.CopyMakeBorder(scaledImage, borderedImage, 0, 0, surplusX, surplusX, BorderTypes.Constant);
            }

            return borderedImage;
        }
        #endregion

        #region # 旋转变换 —— static Mat RotateTrans(this Mat matrix, float angle)
        /// <summary>
        /// 旋转变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="angle">旋转角度</param>
        /// <returns>变换后图像矩阵</returns>
        public static Mat RotateTrans(this Mat matrix, float angle)
        {
            Point center = new Point(matrix.Cols / 2.0f, matrix.Rows / 2.0f);
            using Mat rotatedMatrix = Cv2.GetRotationMatrix2D(center, angle, 1);

            Mat result = new Mat();
            Cv2.WarpAffine(matrix, result, rotatedMatrix, matrix.Size());

            return result;
        }
        #endregion

        #region # 平移变换 —— static Mat TranslateTrans(this Mat matrix, float offsetX...
        /// <summary>
        /// 平移变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="offsetX">X轴平移量</param>
        /// <param name="offsetY">Y轴平移量</param>
        /// <returns>变换后图像矩阵</returns>
        public static Mat TranslateTrans(this Mat matrix, float offsetX, float offsetY)
        {
            float[,] translationArray =
            {
                {1, 0, offsetX},
                {0, 1, offsetY}
            };
            using Mat translationMatrix = Mat.FromArray(translationArray);

            Mat result = new Mat();
            Cv2.WarpAffine(matrix, result, translationMatrix, matrix.Size());

            return result;
        }
        #endregion

        #region # 仿射变换 —— static Mat AffineTrans(this Mat matrix, IEnumerable<Point2f> sourcePoints...
        /// <summary>
        /// 仿射变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sourcePoints">源定位点集</param>
        /// <param name="targetPoints">目标定位点集</param>
        /// <returns>变换后图像矩阵</returns>
        /// <remarks>源/目标定位点必须大于等于3个，且数量必须相等</remarks>
        public static Mat AffineTrans(this Mat matrix, IEnumerable<Point2f> sourcePoints, IEnumerable<Point2f> targetPoints)
        {
            #region # 验证

            sourcePoints = sourcePoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            targetPoints = targetPoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            if (sourcePoints.Count() < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePoints), "源定位点集数量不可小于3！");
            }
            if (targetPoints.Count() < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints), "目标定位点集数量不可小于3！");
            }
            if (sourcePoints.Count() != targetPoints.Count())
            {
                throw new ArgumentOutOfRangeException("源定位点集数量与目标定位点集数量必须相等！", (Exception)null);
            }

            #endregion

            using Mat affineMatrix = Cv2.GetAffineTransform(sourcePoints, targetPoints);
            Mat result = new Mat();
            Cv2.WarpAffine(matrix, result, affineMatrix, matrix.Size());

            return result;
        }
        #endregion

        #region # 透视变换 —— static Mat PerspectiveTrans(this Mat matrix, IEnumerable<Point2f> sourcePoints...
        /// <summary>
        /// 透视变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sourcePoints">源定位点集</param>
        /// <param name="targetPoints">目标定位点集</param>
        /// <returns>变换后图像矩阵</returns>
        /// <remarks>源/目标定位点必须大于等于4个，且数量必须相等</remarks>
        public static Mat PerspectiveTrans(this Mat matrix, IEnumerable<Point2f> sourcePoints, IEnumerable<Point2f> targetPoints)
        {
            #region # 验证

            sourcePoints = sourcePoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            targetPoints = targetPoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            if (sourcePoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePoints), "源定位点集数量不可小于4！");
            }
            if (targetPoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints), "目标定位点集数量不可小于4！");
            }
            if (sourcePoints.Count() != targetPoints.Count())
            {
                throw new ArgumentOutOfRangeException("源定位点集数量与目标定位点集数量必须相等！", (Exception)null);
            }

            #endregion

            using Mat perspectiveMatrix = Cv2.GetPerspectiveTransform(sourcePoints, targetPoints);
            Mat result = new Mat();
            Cv2.WarpPerspective(matrix, result, perspectiveMatrix, matrix.Size());

            return result;
        }
        #endregion

        #region # 透视变换 —— static Point2f[] PerspectiveTrans(this IEnumerable<Point2f> contourPoints...
        /// <summary>
        /// 透视变换
        /// </summary>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <param name="sourcePoints">源定位点集</param>
        /// <param name="targetPoints">目标定位点集</param>
        /// <returns>变换后轮廓坐标点集</returns>
        /// <remarks>源/目标定位点必须大于等于4个，且数量必须相等</remarks>
        public static Point2f[] PerspectiveTrans(this IEnumerable<Point2f> contourPoints, IEnumerable<Point2f> sourcePoints, IEnumerable<Point2f> targetPoints)
        {
            #region # 验证

            sourcePoints = sourcePoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            targetPoints = targetPoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            if (sourcePoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePoints), "源定位点集数量不可小于4！");
            }
            if (targetPoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints), "目标定位点集数量不可小于4！");
            }
            if (sourcePoints.Count() != targetPoints.Count())
            {
                throw new ArgumentOutOfRangeException("源定位点集数量与目标定位点集数量必须相等！", (Exception)null);
            }

            #endregion

            using Mat perspectiveMatrix = Cv2.GetPerspectiveTransform(sourcePoints, targetPoints);
            Point2f[] targetCoutour = Cv2.PerspectiveTransform(contourPoints, perspectiveMatrix);

            return targetCoutour;
        }
        #endregion

        #region # 透视变换 —— static Point2d[] PerspectiveTrans(this IEnumerable<Point2d> contourPoints...
        /// <summary>
        /// 透视变换
        /// </summary>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <param name="sourcePoints">源定位点集</param>
        /// <param name="targetPoints">目标定位点集</param>
        /// <returns>变换后轮廓坐标点集</returns>
        /// <remarks>源/目标定位点必须大于等于4个，且数量必须相等</remarks>
        public static Point2d[] PerspectiveTrans(this IEnumerable<Point2d> contourPoints, IEnumerable<Point2f> sourcePoints, IEnumerable<Point2f> targetPoints)
        {
            #region # 验证

            sourcePoints = sourcePoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            targetPoints = targetPoints?.Distinct().ToArray() ?? Array.Empty<Point2f>();
            if (sourcePoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(sourcePoints), "源定位点集数量不可小于4！");
            }
            if (targetPoints.Count() < 4)
            {
                throw new ArgumentOutOfRangeException(nameof(targetPoints), "目标定位点集数量不可小于4！");
            }
            if (sourcePoints.Count() != targetPoints.Count())
            {
                throw new ArgumentOutOfRangeException("源定位点集数量与目标定位点集数量必须相等！", (Exception)null);
            }

            #endregion

            using Mat perspectiveMatrix = Cv2.GetPerspectiveTransform(sourcePoints, targetPoints);
            Point2d[] targetCoutour = Cv2.PerspectiveTransform(contourPoints, perspectiveMatrix);

            return targetCoutour;
        }
        #endregion

        #region # 缩放关键点列表 —— static IList<KeyPoint> ScaleKeyPoints(this IEnumerable<KeyPoint>...
        /// <summary>
        /// 缩放关键点列表
        /// </summary>
        /// <param name="keyPoints">关键点列表</param>
        /// <param name="imageWidth">原图像宽度</param>
        /// <param name="imageHeight">原图像高度</param>
        /// <param name="scaledSize">缩放尺寸</param>
        /// <returns>缩放关键点列表</returns>
        /// <remarks>缩放为正方形缩放，缩放尺寸为目标边长</remarks>
        public static IList<KeyPoint> ScaleKeyPoints(this IEnumerable<KeyPoint> keyPoints, int imageWidth,
            int imageHeight, int scaledSize)
        {
            #region # 验证

            keyPoints = keyPoints?.ToArray() ?? Array.Empty<KeyPoint>();
            if (!keyPoints.Any())
            {
                return Array.Empty<KeyPoint>();
            }

            #endregion

            int surplusX = 0;
            int surplusY = 0;
            if (imageWidth > imageHeight)
            {
                int width = scaledSize;
                int height = (int)Math.Ceiling(scaledSize * 1.0 / imageWidth * imageHeight);
                surplusY = (width - height) / 2;
            }
            else
            {
                int height = scaledSize;
                int width = (int)Math.Ceiling(scaledSize * 1.0 / imageHeight * imageWidth);
                surplusX = (height - width) / 2;
            }

            float scaleX = imageWidth * 1.0f / (scaledSize - surplusX * 2);
            float scaleY = imageHeight * 1.0f / (scaledSize - surplusY * 2);

            IList<KeyPoint> scaledKeyPoints = new List<KeyPoint>();
            foreach (KeyPoint keyPoint in keyPoints)
            {
                Point2f scaledPoint = new Point2f(keyPoint.Pt.X, keyPoint.Pt.Y);
                scaledPoint.X -= surplusX;
                scaledPoint.Y -= surplusY;
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
