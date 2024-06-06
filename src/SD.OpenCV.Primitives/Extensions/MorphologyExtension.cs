using OpenCvSharp;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 形态学扩展
    /// </summary>
    public static class MorphologyExtension
    {
        #region # 腐蚀 —— static Mat MorphErode(this Mat matrix, int kernelSize)
        /// <summary>
        /// 腐蚀
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>腐蚀图像矩阵</returns>
        /// <remarks>局部最大值</remarks>
        public static Mat MorphErode(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.Erode, kernel);

            return result;
        }
        #endregion

        #region # 膨胀 —— static Mat MorphDilate(this Mat matrix, int kernelSize)
        /// <summary>
        /// 膨胀
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>膨胀图像矩阵</returns>
        /// <remarks>局部最小值</remarks>
        public static Mat MorphDilate(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.Dilate, kernel);

            return result;
        }
        #endregion

        #region # 开运算 —— static Mat MorphOpen(this Mat matrix, int kernelSize)
        /// <summary>
        /// 开运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>开运算图像矩阵</returns>
        /// <remarks>先腐蚀后膨胀</remarks>
        public static Mat MorphOpen(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.Open, kernel);

            return result;
        }
        #endregion

        #region # 闭运算 —— static Mat MorphClose(this Mat matrix, int kernelSize)
        /// <summary>
        /// 闭运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>闭运算图像矩阵</returns>
        /// <remarks>先膨胀后腐蚀</remarks>
        public static Mat MorphClose(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.Close, kernel);

            return result;
        }
        #endregion

        #region # 梯度运算 —— static Mat MorphGradient(this Mat matrix, int kernelSize)
        /// <summary>
        /// 梯度运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>梯度运算图像矩阵</returns>
        /// <remarks>膨胀图像与腐蚀图像差值</remarks>
        public static Mat MorphGradient(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.Gradient, kernel);

            return result;
        }
        #endregion

        #region # 礼帽运算 —— static Mat MorphTopHat(this Mat matrix, int kernelSize)
        /// <summary>
        /// 礼帽运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>礼帽运算图像矩阵</returns>
        /// <remarks>原图像与开运算的差值</remarks>
        public static Mat MorphTopHat(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.TopHat, kernel);

            return result;
        }
        #endregion

        #region # 黑帽运算 —— static Mat MorphBlackHat(this Mat matrix, int kernelSize)
        /// <summary>
        /// 黑帽运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>黑帽运算图像矩阵</returns>
        /// <remarks>闭运算与原图像的差值</remarks>
        public static Mat MorphBlackHat(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.BlackHat, kernel);

            return result;
        }
        #endregion

        #region # 击中与否运算 —— static Mat MorphHitMiss(this Mat matrix, int kernelSize)
        /// <summary>
        /// 击中与否运算
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <returns>击中与否运算图像矩阵</returns>
        public static Mat MorphHitMiss(this Mat matrix, int kernelSize = 3)
        {
            using Mat kernel = Mat.Ones(kernelSize, kernelSize, MatType.CV_8UC1);

            Mat result = new Mat();
            Cv2.MorphologyEx(matrix, result, MorphTypes.HitMiss, kernel);

            return result;
        }
        #endregion
    }
}
