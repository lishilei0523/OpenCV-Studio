using OpenCvSharp;
using System;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 灰度变换扩展
    /// </summary>
    public static class GrayscaleExtension
    {
        #region # 线性变换 —— static Mat LinearTransform(this Mat matrix, float alpha, float beta)
        /// <summary>
        /// 线性变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="alpha">对比度</param>
        /// <param name="beta">亮度</param>
        /// <returns>变换图像矩阵</returns>
        public static unsafe Mat LinearTransform(this Mat matrix, float alpha, float beta)
        {
            Mat result = matrix.Clone();

            byte[] bins = new byte[256];
            for (int index = 0; index < bins.Length; index++)
            {
                double value = index * alpha + beta;
                bins[index] = value >= byte.MaxValue ? byte.MaxValue : (byte)Math.Ceiling(value);
            }

            int channelCount = result.Channels();
            if (channelCount == 1)
            {
                result.ForEachAsByte((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<byte>(rowIndex, colIndex) = bins[*valuePtr];
                });
            }
            if (channelCount == 3)
            {
                result.ForEachAsVec3b((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<Vec3b>(rowIndex, colIndex)[0] = bins[(*valuePtr)[0]];
                    result.At<Vec3b>(rowIndex, colIndex)[1] = bins[(*valuePtr)[1]];
                    result.At<Vec3b>(rowIndex, colIndex)[2] = bins[(*valuePtr)[2]];
                });
            }

            return result;
        }
        #endregion

        #region # 伽马变换 —— static Mat GammaTransform(this Mat matrix, float gamma)
        /// <summary>
        /// 伽马变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="gamma">伽马值</param>
        /// <returns>变换图像矩阵</returns>
        public static unsafe Mat GammaTransform(this Mat matrix, float gamma)
        {
            Mat result = matrix.Clone();

            byte[] bins = new byte[256];
            for (int index = 0; index < bins.Length; index++)
            {
                double value = Math.Pow(index / 255.0f, gamma) * 255.0f;
                bins[index] = value >= byte.MaxValue ? byte.MaxValue : (byte)Math.Ceiling(value);
            }

            int channelCount = result.Channels();
            if (channelCount == 1)
            {
                result.ForEachAsByte((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<byte>(rowIndex, colIndex) = bins[*valuePtr];
                });
            }
            if (channelCount == 3)
            {
                result.ForEachAsVec3b((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<Vec3b>(rowIndex, colIndex)[0] = bins[(*valuePtr)[0]];
                    result.At<Vec3b>(rowIndex, colIndex)[1] = bins[(*valuePtr)[1]];
                    result.At<Vec3b>(rowIndex, colIndex)[2] = bins[(*valuePtr)[2]];
                });
            }

            return result;
        }
        #endregion

        #region # 对数变换 —— static Mat LogarithmicTransform(this Mat matrix, float gamma)
        /// <summary>
        /// 对数变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="gamma">伽马值</param>
        /// <returns>变换图像矩阵</returns>
        public static unsafe Mat LogarithmicTransform(this Mat matrix, float gamma)
        {
            Mat result = matrix.Clone();

            byte[] bins = new byte[256];
            for (int index = 0; index < bins.Length; index++)
            {
                double value = Math.Log(index / 255.0f + 1.0f) / Math.Log(gamma + 1.0f) * 255.0f;
                bins[index] = value >= byte.MaxValue ? byte.MaxValue : (byte)Math.Ceiling(value);
            }

            int channelCount = result.Channels();
            if (channelCount == 1)
            {
                result.ForEachAsByte((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<byte>(rowIndex, colIndex) = bins[*valuePtr];
                });
            }
            if (channelCount == 3)
            {
                result.ForEachAsVec3b((valuePtr, positionPtr) =>
                {
                    int rowIndex = positionPtr[0];
                    int colIndex = positionPtr[1];
                    result.At<Vec3b>(rowIndex, colIndex)[0] = bins[(*valuePtr)[0]];
                    result.At<Vec3b>(rowIndex, colIndex)[1] = bins[(*valuePtr)[1]];
                    result.At<Vec3b>(rowIndex, colIndex)[2] = bins[(*valuePtr)[2]];
                });
            }

            return result;
        }
        #endregion

        #region # 阴影变换 —— static Mat ShadingTransform(this Mat matrix, Size kernelSize...
        /// <summary>
        /// 阴影变换
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">滤波核尺寸</param>
        /// <param name="gain">增益</param>
        /// <param name="norse">噪声</param>
        /// <param name="offset">亮度补偿</param>
        /// <returns>变换图像矩阵</returns>
        public static unsafe Mat ShadingTransform(this Mat matrix, Size kernelSize, byte gain = 60, byte norse = 0, byte offset = 140)
        {
            //克隆前景图，转32F1
            Mat foreMatrix = matrix.Clone();
            foreMatrix.ConvertTo(foreMatrix, MatType.CV_32FC1);

            //滤波取背景图
            using Mat backMatrix = foreMatrix.GaussianBlur(kernelSize, 1);

            //计算差值图
            foreMatrix.ForEachAsFloat((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                float foreValue = *valuePtr;
                float backValue = backMatrix.At<float>(rowIndex, colIndex);
                if (foreValue > backValue)
                {
                    foreMatrix.At<float>(rowIndex, colIndex) = gain * (foreValue - backValue - norse) + offset;
                }
                else
                {
                    foreMatrix.At<float>(rowIndex, colIndex) = gain * (foreValue - backValue + norse) + offset;
                }
            });

            //再次滤波，转回8UC1
            foreMatrix = foreMatrix.GaussianBlur(kernelSize, 1);
            foreMatrix.ConvertTo(foreMatrix, MatType.CV_8UC1);

            return foreMatrix;
        }
        #endregion
    }
}
