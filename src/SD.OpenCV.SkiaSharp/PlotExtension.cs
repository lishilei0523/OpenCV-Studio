using OpenCvSharp;
using ScottPlot;
using System;
using System.Linq;

namespace SD.OpenCV.SkiaSharp
{
    /// <summary>
    /// 绘图扩展
    /// </summary>
    public static class PlotExtension
    {
        #region # 添加描述子 —— static void AddDescriptors(this Plot plot, Mat descriptors)
        /// <summary>
        /// 添加描述子
        /// </summary>
        /// <param name="plot">绘图器</param>
        /// <param name="descriptors">描述子矩阵</param>
        public static void AddDescriptors(this Plot plot, Mat descriptors)
        {
            #region # 验证

            if (descriptors == null)
            {
                throw new ArgumentNullException(nameof(descriptors), "描述子不可为空！");
            }

            #endregion

            double[] xs = Enumerable.Range(1, descriptors.Cols).Select(x => (double)x).ToArray();
            for (int rowIndex = 0; rowIndex < descriptors.Rows; rowIndex++)
            {
                double[] ys = new double[descriptors.Cols];
                for (int colIndex = 0; colIndex < descriptors.Cols; colIndex++)
                {
                    ys[colIndex] = descriptors.At<float>(rowIndex, colIndex);
                }
                plot.Add.ScatterLine(xs, ys);
            }
        }
        #endregion
    }
}
