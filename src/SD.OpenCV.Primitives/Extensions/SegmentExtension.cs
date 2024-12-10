using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Extensions
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

        #region # 获取轮廓置信度 —— static float GetContourConfidence(this Mat matrix, Point[] contour)
        /// <summary>
        /// 获取轮廓置信度
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contour">轮廓</param>
        /// <returns>轮廓置信度</returns>
        public static float GetContourConfidence(this Mat matrix, Point[] contour)
        {
            //获取轮廓点的外接矩形
            Rect boundingBox = Cv2.BoundingRect(contour);
            int xMin = Math.Max(boundingBox.X, 0);
            int yMin = Math.Max(boundingBox.Y, 0);
            int xMax = Math.Min(boundingBox.X + boundingBox.Width, matrix.Width - 1);
            int yMax = Math.Min(boundingBox.Y + boundingBox.Height, matrix.Height - 1);

            //填充外接矩形内，由轮廓点围成的多边形
            Size roiSize = new Size(xMax - xMin + 1, yMax - yMin + 1);
            Rect roiBox = new Rect(xMin, yMin, roiSize.Width, roiSize.Height);
            using Mat roi = matrix[roiBox];
            using Mat mask = Mat.Zeros(roiSize, MatType.CV_8UC1);
            IEnumerable<Point> roiContour = contour.Select(point => new Point(point.X - xMin, point.Y - yMin));
            Cv2.FillPoly(mask, new[] { roiContour }, new Scalar(1));

            //计算填充多边形区域的均值 
            Scalar mean = Cv2.Mean(roi, mask);
            float confidence = (float)(mean.Val0 / 255.0f);

            return confidence;
        }
        #endregion

        #region # 扩张轮廓 —— static IList<Point2f> ExpandContour(this IList<Point2f> contour...
        /// <summary>
        /// 扩张轮廓
        /// </summary>
        /// <param name="contour">轮廓</param>
        /// <param name="expandRatio">扩张率</param>
        /// <returns>扩张后轮廓</returns>
        public static IList<Point2f> ExpandContour(this IList<Point2f> contour, float expandRatio = 1.6f)
        {
            float area = (float)Cv2.ContourArea(contour);//轮廓面积
            float length = (float)Cv2.ArcLength(contour, true);//轮廓周长
            float distance = area * expandRatio / length;

            IList<IList<Point2f>> lines = new List<IList<Point2f>>();
            for (int index = 0; index < contour.Count; index++)
            {
                IList<Point2f> line = new List<Point2f>();
                Point2f point1 = contour[index];
                Point2f point2 = contour[(index - 1 + contour.Count) % contour.Count];
                Point2f vec = point1 - point2;
                float expandDis = (float)(distance / Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y));

                Point2f rotateVec = new Point2f(vec.Y * expandDis, -vec.X * expandDis);
                line.Add(new Point2f(point1.X + rotateVec.X, point1.Y + rotateVec.Y));
                line.Add(new Point2f(point2.X + rotateVec.X, point2.Y + rotateVec.Y));
                lines.Add(line);
            }

            IList<Point2f> expandedContour = new List<Point2f>();
            for (int index = 0; index < lines.Count; index++)
            {
                Point2f a = lines[index][0];
                Point2f b = lines[index][1];
                Point2f c = lines[(index + 1) % lines.Count][0];
                Point2f d = lines[(index + 1) % lines.Count][1];
                Point2f v1 = b - a;
                Point2f v2 = d - c;
                float cosAngle = (float)((v1.X * v2.X + v1.Y * v2.Y) / (Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y) * Math.Sqrt(v2.X * v2.X + v2.Y * v2.Y)));

                Point2f point = new Point2f();
                if (Math.Abs(cosAngle) > 0.7)
                {
                    point.X = (b.X + c.X) * 0.5f;
                    point.Y = (b.Y + c.Y) * 0.5f;
                }
                else
                {
                    float denom = a.X * (d.Y - c.Y) + b.X * (c.Y - d.Y) + d.X * (b.Y - a.Y) + c.X * (a.Y - b.Y);
                    float num = a.X * (d.Y - c.Y) + c.X * (a.Y - d.Y) + d.X * (c.Y - a.Y);
                    float s = num / denom;

                    point.X = a.X + s * (b.X - a.X);
                    point.Y = a.Y + s * (b.Y - a.Y);
                }

                expandedContour.Add(point);
            }

            return expandedContour;
        }
        #endregion

        #region # 修正矩形 —— static Rect CorrectRectangle(this Mat matrix, Rect rectangle)
        /// <summary>
        /// 修正矩形
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">矩形</param>
        /// <returns>修正后矩形</returns>
        public static Rect CorrectRectangle(this Mat matrix, Rect rectangle)
        {
            float xMin = rectangle.X;
            float yMin = rectangle.Y;
            float xMax = xMin + rectangle.Width;
            float yMax = yMin + rectangle.Height;
            float correctXMin = xMin < 0 ? 0 : xMin;
            float correctYMin = yMin < 0 ? 0 : yMin;
            float correctXMax = xMax > matrix.Width ? matrix.Width : xMax;
            float correctYMax = yMax > matrix.Height ? matrix.Height : yMax;

            Point location = new Point(correctXMin, correctYMin);
            Size size = new Size(correctXMax - correctXMin, correctYMax - correctYMin);
            Rect correctRect = new Rect(location, size);

            return correctRect;
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

            using Mat hsvResult = new Mat();
            Cv2.BitwiseAnd(hsvMatrix, hsvMatrix, hsvResult, mask);

            Mat result = hsvResult.CvtColor(ColorConversionCodes.HSV2BGR);

            return result;
        }
        #endregion

        #region # K-Means聚类分割 —— static Mat KMeansSegment(this Mat matrix, int clustersCount...
        /// <summary>
        /// K-Means聚类分割
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="clustersCount">簇数量</param>
        /// <param name="criteriaMaxCount">优化迭代次数</param>
        /// <param name="criteriaEpsilon">优化误差</param>
        /// <param name="attemptsCount">重复试验次数</param>
        /// <param name="kMeansFlag">初始中心类型</param>
        /// <returns>分割结果图像矩阵</returns>
        public static unsafe Mat KMeansSegment(this Mat matrix, int clustersCount, int criteriaMaxCount = 10, double criteriaEpsilon = 0.1, int attemptsCount = 3, KMeansFlags kMeansFlag = KMeansFlags.PpCenters)
        {
            //定义颜色
            Scalar[] colors;
            if (clustersCount < 9)
            {
                colors = new[]
                {
                    new Scalar(0, 0, 0),
                    new Scalar(0, 0, 255),
                    new Scalar(0, 255, 0),
                    new Scalar(255, 0, 0),
                    new Scalar(0, 255, 255),
                    new Scalar(255, 0, 255),
                    new Scalar(255, 255, 0),
                    new Scalar(204, 204, 204),
                    new Scalar(255, 255, 255)
                };
            }
            else
            {
                Random random = new Random((int)(DateTime.Now.Ticks / 1000));
                colors = new Scalar[clustersCount];
                for (int index = 0; index < clustersCount; index++)
                {
                    colors[index] = new Scalar(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255));
                }
            }

            //初始化参数
            int channelsCount = matrix.Channels();
            int samplesCount = matrix.Rows * matrix.Cols;
            using Mat points = new Mat(samplesCount, channelsCount, MatType.CV_32F, new Scalar(10));
            using Mat labels = new Mat();
            using Mat centers = new Mat(clustersCount, 1, points.Type());

            if (channelsCount == 1)
            {
                //GRAY数据转换到样本数据
                matrix.ForEachAsByte((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    byte value = *valuePtr;

                    int index = rowIndex * matrix.Cols + colIndex;
                    points.At<float>(index, 0) = value;
                });
            }
            else if (channelsCount == 3)
            {
                //BGR数据转换到样本数据
                matrix.ForEachAsVec3b((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    Vec3b vector = *valuePtr;

                    int index = rowIndex * matrix.Cols + colIndex;
                    points.At<float>(index, 0) = vector[0];
                    points.At<float>(index, 1) = vector[1];
                    points.At<float>(index, 2) = vector[2];
                });
            }
            else
            {
                throw new NotSupportedException($"不支持的像素格式！Channels: {channelsCount}");
            }

            //执行K-Means聚类
            TermCriteria termCriteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.Count, criteriaMaxCount, criteriaEpsilon);
            Cv2.Kmeans(points, clustersCount, labels, termCriteria, attemptsCount, kMeansFlag, centers);

            //绘制图像分割结果
            Mat result = Mat.Zeros(matrix.Size(), MatType.CV_8UC3);
            if (channelsCount == 1)
            {
                matrix.ForEachAsByte((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];

                    int index = rowIndex * matrix.Cols + colIndex;
                    int label = labels.At<int>(index, 0);
                    result.At<Vec3b>(rowIndex, colIndex) = new Vec3b((byte)colors[label][0], (byte)colors[label][1], (byte)colors[label][2]);
                });
            }
            else
            {
                matrix.ForEachAsVec3b((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];

                    int index = rowIndex * matrix.Cols + colIndex;
                    int label = labels.At<int>(index, 0);
                    result.At<Vec3b>(rowIndex, colIndex) = new Vec3b((byte)colors[label][0], (byte)colors[label][1], (byte)colors[label][2]);
                });
            }

            return result;
        }
        #endregion

        #region # GrabCut分割 —— static Mat GrabCutSegment(this Mat matrix, Rect rectangle...
        /// <summary>
        /// GrabCut分割
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">矩形</param>
        /// <param name="mask">掩膜</param>
        /// <param name="iterationsCount">迭代次数</param>
        /// <returns>结果图像</returns>
        public static unsafe Mat GrabCutSegment(this Mat matrix, Rect rectangle, out Mat mask, int iterationsCount = 5)
        {
            using Mat background = new Mat();
            using Mat foreground = new Mat();
            Mat innerMask = new Mat();
            Cv2.GrabCut(matrix, innerMask, rectangle, background, foreground, 5, GrabCutModes.InitWithRect);

            //将分割出的前景绘制回来
            innerMask.ForEachAsByte((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                byte value = *valuePtr;

                //将明显是前景和可能是前景的区域都保留
                if (value == 1 || value == 3)
                {
                    innerMask.At<byte>(rowIndex, colIndex) = byte.MaxValue;
                }
                //将明显是背景和可能是背景的区域都删除
                else
                {
                    innerMask.At<byte>(rowIndex, colIndex) = 0;
                }
            });

            mask = innerMask;
            Mat result = new Mat();
            Cv2.BitwiseAnd(matrix, matrix, result, mask);

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
