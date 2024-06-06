using OpenCvSharp;
using System;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 边缘检测扩展
    /// </summary>
    public static class EdgeExtension
    {
        #region # 适用Robert边缘检测算子 —— static Mat ApplyRobert(this Mat matrix)
        /// <summary>
        /// 适用Robert边缘检测算子
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <returns>边缘检测图像矩阵</returns>
        public static unsafe Mat ApplyRobert(this Mat matrix)
        {
            Mat result = matrix.Clone();
            matrix.ForEachAsByte((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];

                #region # 验证

                if (rowIndex == matrix.Rows - 1)
                {
                    return;
                }
                if (colIndex == matrix.Cols - 1)
                {
                    return;
                }

                #endregion

                double dx = Math.Pow(matrix.At<byte>(rowIndex, colIndex) - matrix.At<byte>(rowIndex + 1, colIndex + 1), 2);
                double dy = Math.Pow(matrix.At<byte>(rowIndex + 1, colIndex) - matrix.At<byte>(rowIndex, colIndex + 1), 2);
                double value = Math.Sqrt(dx + dy);
                byte pixelValue = value >= byte.MaxValue ? byte.MaxValue : (byte)Math.Ceiling(value);
                result.At<byte>(rowIndex, colIndex) = pixelValue;
            });

            return result;
        }
        #endregion

        #region # 适用Sobel边缘检测算子 —— static Mat ApplySobel(this Mat matrix, int kernalSize...
        /// <summary>
        /// 适用Sobel边缘检测算子
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernalSize">卷积核尺寸</param>
        /// <param name="alpha">X轴卷积权重</param>
        /// <param name="beta">Y轴卷积权重</param>
        /// <param name="gamma">伽马值</param>
        /// <returns>边缘检测图像矩阵</returns>
        /// <remarks>卷积核尺寸必须为奇数</remarks>
        public static Mat ApplySobel(this Mat matrix, int kernalSize = 3, double alpha = 0.5, double beta = 0.5, double gamma = 0)
        {
            //计算Sobel卷积结果
            using Mat x = new Mat();
            using Mat y = new Mat();
            Cv2.Sobel(matrix, x, MatType.CV_16S, 1, 0, kernalSize);
            Cv2.Sobel(matrix, y, MatType.CV_16S, 0, 1, kernalSize);

            //数据转换
            using Mat absX = new Mat();
            using Mat absY = new Mat();
            Cv2.ConvertScaleAbs(x, absX);
            Cv2.ConvertScaleAbs(y, absY);

            Mat result = new Mat();
            Cv2.AddWeighted(absX, alpha, absY, beta, gamma, result);

            return result;
        }
        #endregion

        #region # 适用Scharr边缘检测算子 —— static Mat ApplyScharr(this Mat matrix, double alpha...
        /// <summary>
        /// 适用Scharr边缘检测算子
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="alpha">X轴卷积权重</param>
        /// <param name="beta">Y轴卷积权重</param>
        /// <param name="gamma">伽马值</param>
        /// <returns>边缘检测图像矩阵</returns>
        public static Mat ApplyScharr(this Mat matrix, double alpha = 0.5, double beta = 0.5, double gamma = 0)
        {
            //计算Scharr卷积结果
            using Mat x = new Mat();
            using Mat y = new Mat();
            Cv2.Sobel(matrix, x, MatType.CV_16S, 1, 0, -1);
            Cv2.Sobel(matrix, y, MatType.CV_16S, 0, 1, -1);

            //数据转换
            using Mat absX = new Mat();
            using Mat absY = new Mat();
            Cv2.ConvertScaleAbs(x, absX);
            Cv2.ConvertScaleAbs(y, absY);

            Mat result = new Mat();
            Cv2.AddWeighted(absX, alpha, absY, beta, gamma, result);

            return result;
        }
        #endregion

        #region # 适用Laplacian边缘检测算子 —— static Mat ApplyLaplacian(this Mat matrix, int kernalSize...
        /// <summary>
        /// 适用Laplacian边缘检测算子
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernalSize">卷积核尺寸</param>
        /// <returns>边缘检测图像矩阵</returns>
        /// <remarks>卷积核尺寸必须为奇数</remarks>
        public static Mat ApplyLaplacian(this Mat matrix, int kernalSize = 3)
        {
            Mat result = new Mat();
            Cv2.Laplacian(matrix, result, -1, kernalSize);
            Cv2.ConvertScaleAbs(result, result);

            return result;
        }
        #endregion
    }
}
