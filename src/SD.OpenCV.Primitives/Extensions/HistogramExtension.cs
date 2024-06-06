using OpenCvSharp;
using System;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 直方图扩展
    /// </summary>
    public static class HistogramExtension
    {
        #region # 生成直方图 —— static Mat GenerateHistogram(this Mat matrix)
        /// <summary>
        /// 生成直方图
        /// </summary>
        public static Mat GenerateHistogram(this Mat matrix)
        {
            Mat[] images = { matrix };
            Mat histogram = new Mat();
            int[] channels = { 0 };
            int[] histSize = { 256 };
            Rangef[] histRange = { new Rangef(0, 256) };
            Cv2.CalcHist(images, channels, null, histogram, 1, histSize, histRange);

            return histogram;
        }
        #endregion

        #region # 映射直方图 —— static Mat MapHistogram(this Mat sourceMatrix, Mat referenceMatrix)
        /// <summary>
        /// 映射直方图
        /// </summary>
        /// <param name="sourceMatrix">源图像矩阵</param>
        /// <param name="referenceMatrix">参考图像矩阵</param>
        /// <returns>映射图像矩阵</returns>
        public static Mat MapHistogram(this Mat sourceMatrix, Mat referenceMatrix)
        {
            //计算源图像累计概率
            Size sourceSize = sourceMatrix.Size();
            using Mat sourceHist = GenerateHistogram(sourceMatrix);
            using Mat sourceRatios = GetHistAccumulations(sourceHist, sourceSize);

            //计算参考图像累计概率
            Size referenceSize = referenceMatrix.Size();
            using Mat referenceHist = GenerateHistogram(referenceMatrix);
            using Mat referenceRatios = GetHistAccumulations(referenceHist, referenceSize);

            //执行关系映射
            using Mat map = new Mat(256, 1, MatType.CV_8UC1);
            for (int i = 0; i < 256; i++)
            {
                float sourceRatio = sourceRatios.At<float>(i);
                float min = 10;
                byte k = 0;

                //去参考图的直方图中查找最小的差值
                for (int j = 0; j < 256; j++)
                {
                    float referenceRatio = referenceRatios.At<float>(j);
                    //计算差值
                    float diff = Math.Abs(sourceRatio - referenceRatio);
                    if (diff < min)
                    {
                        min = diff;
                        k = (byte)j;
                    }
                }

                // 当上面循环执行完毕，就找到了最小的差值  120 ---》 200
                map.At<byte>(i) = k;
            }

            Mat result = new Mat(sourceSize, sourceMatrix.Type());

            //将原图的颜色映射到新的颜色空间
            for (int row = 0; row < result.Rows; row++)
            {
                for (int col = 0; col < result.Cols; col++)
                {
                    byte color = sourceMatrix.At<byte>(row, col);
                    byte newColor = map.At<byte>(color);
                    result.At<byte>(row, col) = newColor;
                }
            }

            return result;
        }
        #endregion

        #region # 自适应直方图均衡化 —— static Mat AdaptiveEqualizeHist(this Mat matrix, double clipLimit...
        /// <summary>
        /// 阴影变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="clipLimit">裁剪限制</param>
        /// <param name="tileGridSize">块尺寸</param>
        /// <returns>变换图像矩阵</returns>
        public static Mat AdaptiveEqualizeHist(this Mat matrix, double clipLimit = 2.0d, Size? tileGridSize = null)
        {
            using CLAHE clahe = Cv2.CreateCLAHE(clipLimit, tileGridSize);

            Mat result = new Mat();
            clahe.Apply(matrix, result);

            return result;
        }
        #endregion


        //Private

        #region # 获取直方图累计概率 —— Mat GetHistAccumulations(this Mat histogram, Size matrixSize)
        /// <summary>
        /// 获取直方图累计概率
        /// </summary>
        /// <param name="histogram">直方图</param>
        /// <param name="matrixSize">图像尺寸</param>
        /// <returns>直方图累计概率矩阵</returns>
        private static Mat GetHistAccumulations(this Mat histogram, Size matrixSize)
        {
            Mat ratios = histogram / (matrixSize.Width * matrixSize.Height);

            float seed = 0;
            for (int index = 0; index < histogram.Rows; index++)
            {
                seed += ratios.At<float>(index);
                ratios.At<float>(index) = seed;
            }

            return ratios;
        }
        #endregion
    }
}
