using OpenCvSharp;
using ScottPlot;
using SD.OpenCV.Primitives.Extensions;
using SkiaSharp;
using System;
using System.Linq;

namespace SD.OpenCV.SkiaSharp
{
    /// <summary>
    /// SkiaSharp扩展
    /// </summary>
    public static class SkiaSharpExtension
    {
        #region # OpenCV矩阵映射SKBitmap —— static SKBitmap ToSKBitmap(this Mat matrix)
        /// <summary>
        /// OpenCV矩阵映射SKBitmap
        /// </summary>
        public static SKBitmap ToSKBitmap(this Mat matrix)
        {
            int channelsCount = matrix.Channels();
            Mat cvtMat;
            SKColorType colorType;
            if (channelsCount == 1)
            {
                cvtMat = matrix.Clone();
                colorType = SKColorType.Gray8;
            }
            else if (channelsCount == 3)
            {
                cvtMat = matrix.CvtColor(ColorConversionCodes.BGR2BGRA);
                colorType = SKColorType.Bgra8888;
            }
            else
            {
                throw new NotSupportedException("不支持的通道数！");
            }

            SKBitmapReleaseDelegate releaseAction = (address, context) =>
            {
                cvtMat.Dispose();
            };

            SKImageInfo imageInfo = new SKImageInfo(matrix.Width, matrix.Height, colorType);
            SKBitmap bitmap = new SKBitmap();
            bool success = bitmap.InstallPixels(imageInfo, cvtMat.Data, imageInfo.RowBytes, releaseAction);
            if (!success)
            {
                throw new InvalidCastException("写入像素失败！");
            }

            return bitmap;
        }
        #endregion

        #region # SKBitmap映射OpenCV矩阵 —— static Mat ToMat(this SKBitmap bitmap)
        /// <summary>
        /// SKBitmap映射OpenCV矩阵
        /// </summary>
        public static Mat ToMat(this SKBitmap bitmap)
        {
            Mat matrix;
            IntPtr pixelsPtr = bitmap.GetPixels();
            if (bitmap.ColorType == SKColorType.Gray8)
            {
                matrix = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC1, pixelsPtr, bitmap.RowBytes);
            }
            else if (bitmap.ColorType == SKColorType.Bgra8888)
            {
                using Mat matrix8UC4 = Mat.FromPixelData(bitmap.Height, bitmap.Width, MatType.CV_8UC4, pixelsPtr, bitmap.RowBytes);
                matrix = matrix8UC4.CvtColor(ColorConversionCodes.BGRA2BGR);
            }
            else
            {
                throw new NotSupportedException("不支持的像素格式！");
            }

            return matrix;
        }
        #endregion

        #region # 生成直方图图像 —— static Mat GenerateHistogramImage(this Mat matrix...
        /// <summary>
        /// 生成直方图图像
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="width">直方图图像宽度</param>
        /// <param name="height">直方图图像高度</param>
        /// <returns>直方图图像矩阵</returns>
        public static Mat GenerateHistogramImage(this Mat matrix, int width = 1024, int height = 768)
        {
            //生成直方图矩阵
            using Mat histogram = matrix.GenerateHistogram();
            histogram.GetArray(out float[] histVector);

            //ScottPlot绘图
            double[] values = histVector.Select(x => (double)x).ToArray();
            double[] positions = Enumerable.Range(1, histVector.Length).Select(x => (double)x).ToArray();
            using Plot plot = new Plot();
            plot.Add.Bars(positions, values);
            byte[] imageBytes = plot.GetImageBytes(width, height);

            //转换OpenCV图像矩阵
            Mat histogramImage = Cv2.ImDecode(imageBytes, ImreadModes.Color);

            return histogramImage;
        }
        #endregion
    }
}
