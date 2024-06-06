using OpenCvSharp;
using System;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 滤波扩展
    /// </summary>
    public static class BlurExtension
    {
        #region # 单尺度Retinex增强 —— static Mat SingleScaleRetinex(this Mat matrix, float sigma)
        /// <summary>
        /// 单尺度Retinex增强
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">标准偏差值</param>
        /// <returns>增强图像矩阵</returns>
        public static unsafe Mat SingleScaleRetinex(this Mat matrix, float sigma)
        {
            Size matrixSize = matrix.Size();
            using Mat logMatrix = new Mat(matrixSize, MatType.CV_32FC3);
            using Mat gaussMatrix = new Mat(matrixSize, MatType.CV_32FC3);
            using Mat gaussLogMatrix = new Mat(matrixSize, MatType.CV_32FC3);
            using Mat dstLogMatrix = new Mat(matrixSize, MatType.CV_32FC3);
            Mat result = new Mat(matrixSize, MatType.CV_32FC3);

            float minValue = 0.01f;
            int kernelSize = (int)(sigma * 3 / 2) * 2 + 1;

            //求Log(S(x,y))
            matrix.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b vector = *valuePtr;
                logMatrix.At<Vec3f>(rowIndex, colIndex)[0] = vector[0] <= minValue ? minValue : (float)Math.Log10(vector[0]);
                logMatrix.At<Vec3f>(rowIndex, colIndex)[1] = vector[1] <= minValue ? minValue : (float)Math.Log10(vector[1]);
                logMatrix.At<Vec3f>(rowIndex, colIndex)[2] = vector[2] <= minValue ? minValue : (float)Math.Log10(vector[2]);
            });

            //求Log(L(x,y))
            Cv2.GaussianBlur(matrix, gaussMatrix, new Size(kernelSize, kernelSize), sigma);
            gaussMatrix.ForEachAsVec3b((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3b vector = *valuePtr;
                gaussLogMatrix.At<Vec3f>(rowIndex, colIndex)[0] = vector[0] <= minValue ? minValue : (float)Math.Log10(vector[0]);
                gaussLogMatrix.At<Vec3f>(rowIndex, colIndex)[1] = vector[1] <= minValue ? minValue : (float)Math.Log10(vector[1]);
                gaussLogMatrix.At<Vec3f>(rowIndex, colIndex)[2] = vector[2] <= minValue ? minValue : (float)Math.Log10(vector[2]);
            });

            //求Log(S(x,y)) - Log(L(x,y))
            gaussLogMatrix.ForEachAsVec3f((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3f vector1 = logMatrix.At<Vec3f>(rowIndex, colIndex);
                Vec3f vector2 = gaussLogMatrix.At<Vec3f>(rowIndex, colIndex);
                dstLogMatrix.At<Vec3f>(rowIndex, colIndex)[0] = vector1[0] - vector2[0];
                dstLogMatrix.At<Vec3f>(rowIndex, colIndex)[1] = vector1[1] - vector2[1];
                dstLogMatrix.At<Vec3f>(rowIndex, colIndex)[2] = vector1[2] - vector2[2];
            });

            //求B/G/R三通道的min，max
            Vec3f bgrMin = new Vec3f();
            Vec3f bgrMax = new Vec3f();
            dstLogMatrix.ForEachAsVec3f((valuePtr, positionPtr) =>
            {
                Vec3f vector = *valuePtr;
                if (vector[0] > bgrMax[0])
                {
                    bgrMax[0] = vector[0];
                }
                if (vector[1] > bgrMax[1])
                {
                    bgrMax[1] = vector[1];
                }
                if (vector[2] > bgrMax[2])
                {
                    bgrMax[2] = vector[2];
                }
                if (vector[0] < bgrMin[0])
                {
                    bgrMin[0] = vector[0];
                }
                if (vector[1] < bgrMin[1])
                {
                    bgrMin[1] = vector[1];
                }
                if (vector[2] < bgrMin[2])
                {
                    bgrMin[2] = vector[2];
                }
            });

            //量化处理
            dstLogMatrix.ForEachAsVec3f((valuePtr, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                Vec3f vector = *valuePtr;
                result.At<Vec3f>(rowIndex, colIndex)[0] = 255 * (vector[0] - bgrMin[0]) / (bgrMax[0] - bgrMin[0]);
                result.At<Vec3f>(rowIndex, colIndex)[1] = 255 * (vector[1] - bgrMin[1]) / (bgrMax[1] - bgrMin[1]);
                result.At<Vec3f>(rowIndex, colIndex)[2] = 255 * (vector[2] - bgrMin[2]) / (bgrMax[2] - bgrMin[2]);
            });

            //转回8UC3
            result.ConvertTo(result, MatType.CV_8UC3);

            return result;
        }
        #endregion

        #region # 锐化滤波 —— static Mat SharpBlur(this Mat matrix)
        /// <summary>
        /// 锐化滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat SharpBlur(this Mat matrix)
        {
            double[,] kernelArray =
            {
                {-1, -1, -1},
                {-1, 9, -1},
                {-1, -1, -1},
            };
            using Mat kernel = Mat.FromArray(kernelArray);

            Mat result = new Mat();
            Cv2.Filter2D(matrix, result, matrix.Type(), kernel);

            return result;
        }
        #endregion

        #region # 理想低通滤波 —— static Mat IdealLPBlur(this Mat matrix, float sigma)
        /// <summary>
        /// 理想低通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat IdealLPBlur(this Mat matrix, float sigma)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateIdealLPKernel(borderedMatrix.Size(), sigma);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 理想高通滤波 —— static Mat IdealHPBlur(this Mat matrix, float sigma)
        /// <summary>
        /// 理想高通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat IdealHPBlur(this Mat matrix, float sigma)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateIdealHPKernel(borderedMatrix.Size(), sigma);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 理想带通滤波 —— static Mat IdealBPBlur(this Mat matrix, float sigma, float bandWidth)
        /// <summary>
        /// 理想带通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat IdealBPBlur(this Mat matrix, float sigma, float bandWidth)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateIdealBPKernel(borderedMatrix.Size(), sigma, bandWidth);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 理想带阻滤波 —— static Mat IdealBRBlur(this Mat matrix, float sigma, float bandWidth)
        /// <summary>
        /// 理想带阻滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat IdealBRBlur(this Mat matrix, float sigma, float bandWidth)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateIdealBRKernel(borderedMatrix.Size(), sigma, bandWidth);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 高斯低通滤波 —— static Mat GaussianLPBlur(this Mat matrix, float sigma)
        /// <summary>
        /// 高斯低通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat GaussianLPBlur(this Mat matrix, float sigma)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateGaussianLPKernel(borderedMatrix.Size(), sigma);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 高斯高通滤波 —— static Mat GaussianHPBlur(this Mat matrix, float sigma)
        /// <summary>
        /// 高斯高通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat GaussianHPBlur(this Mat matrix, float sigma)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateGaussianHPKernel(borderedMatrix.Size(), sigma);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 高斯带通滤波 —— static Mat GaussianBPBlur(this Mat matrix, float sigma, float bandWidth)
        /// <summary>
        /// 高斯带通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat GaussianBPBlur(this Mat matrix, float sigma, float bandWidth)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateGaussianBPKernel(borderedMatrix.Size(), sigma, bandWidth);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 高斯带阻滤波 —— static Mat GaussianBRBlur(this Mat matrix, float sigma, float bandWidth)
        /// <summary>
        /// 高斯带阻滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat GaussianBRBlur(this Mat matrix, float sigma, float bandWidth)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateGaussianBRKernel(borderedMatrix.Size(), sigma, bandWidth);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 高斯同态滤波 —— static Mat GaussianHomoBlur(this Mat matrix, float gammaH, float gammaL...
        /// <summary>
        /// 高斯同态滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="gammaH">高频增益</param>
        /// <param name="gammaL">低频增益</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="slope">滤波斜率</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat GaussianHomoBlur(this Mat matrix, float gammaH, float gammaL, float sigma, float slope)
        {
            Mat homoMatrix = matrix.Clone();
            homoMatrix.ConvertTo(homoMatrix, MatType.CV_32FC1);

            //对数化
            homoMatrix += Scalar.All(1);
            Cv2.Log(homoMatrix, homoMatrix);

            //频域滤波
            using Mat borderedMatrix = homoMatrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateGaussianHomoKernel(borderedMatrix.Size(), gammaH, gammaL, sigma, slope);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);

            //指数化
            Cv2.Exp(result, result);
            result -= Scalar.All(1);

            //转换颜色通道
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            //释放资源
            homoMatrix.Dispose();

            return result;
        }
        #endregion

        #region # 巴特沃斯低通滤波 —— static Mat ButterworthLPBlur(this Mat matrix, float sigma, int n)
        /// <summary>
        /// 巴特沃斯低通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="n">阶数</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat ButterworthLPBlur(this Mat matrix, float sigma, int n = 2)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateButterworthLPKernel(borderedMatrix.Size(), sigma, n);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 巴特沃斯高通滤波 —— static Mat ButterworthHPBlur(this Mat matrix, float sigma, int n)
        /// <summary>
        /// 巴特沃斯高通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="n">阶数</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat ButterworthHPBlur(this Mat matrix, float sigma, int n = 2)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateButterworthHPKernel(borderedMatrix.Size(), sigma, n);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 巴特沃斯带通滤波 —— static Mat ButterworthBPBlur(this Mat matrix, float sigma, float bandWidth, int n)
        /// <summary>
        /// 巴特沃斯带通滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <param name="n">阶数</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat ButterworthBPBlur(this Mat matrix, float sigma, float bandWidth, int n = 2)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateButterworthBPKernel(borderedMatrix.Size(), sigma, bandWidth, n);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 巴特沃斯带阻滤波 —— static Mat ButterworthBRBlur(this Mat matrix, float sigma, float bandWidth, int n)
        /// <summary>
        /// 巴特沃斯带阻滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <param name="n">阶数</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat ButterworthBRBlur(this Mat matrix, float sigma, float bandWidth, int n = 2)
        {
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateButterworthBRKernel(borderedMatrix.Size(), sigma, bandWidth, n);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            return result;
        }
        #endregion

        #region # 巴特沃斯同态滤波 —— static Mat ButterworthHomoBlur(this Mat matrix, float gammaH, float gammaL...
        /// <summary>
        /// 巴特沃斯同态滤波
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="gammaH">高频增益</param>
        /// <param name="gammaL">低频增益</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="slope">滤波斜率</param>
        /// <returns>滤波图像矩阵</returns>
        public static Mat ButterworthHomoBlur(this Mat matrix, float gammaH, float gammaL, float sigma, float slope)
        {
            Mat homoMatrix = matrix.Clone();
            homoMatrix.ConvertTo(homoMatrix, MatType.CV_32FC1);

            //对数化
            homoMatrix += Scalar.All(1);
            Cv2.Log(homoMatrix, homoMatrix);

            //频域滤波
            using Mat borderedMatrix = homoMatrix.GenerateDFTBorderedMatrix();
            using Mat kernelMatrix = GenerateButterworthHomoKernel(borderedMatrix.Size(), gammaH, gammaL, sigma, slope);
            Mat result = borderedMatrix.FrequencyBlur(kernelMatrix);

            //指数化
            Cv2.Exp(result, result);
            result -= Scalar.All(1);

            //转换颜色通道
            result = result[matrix.BoundingRect()];
            result.ConvertTo(result, MatType.CV_8UC1);

            //释放资源
            homoMatrix.Dispose();

            return result;
        }
        #endregion


        //Kernel

        #region # 生成理想低通滤波核 —— static Mat GenerateIdealLPKernel(Size kernelSize, float sigma)
        /// <summary>
        /// 生成理想低通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>理想低通滤波核矩阵</returns>
        private static unsafe Mat GenerateIdealLPKernel(Size kernelSize, float sigma)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建理想低通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                if (distance < sigma)
                {
                    kernel.At<float>(rowIndex, colIndex) = 1;
                }
                else
                {
                    kernel.At<float>(rowIndex, colIndex) = 0;
                }
            });

            return kernel;
        }
        #endregion

        #region # 生成理想高通滤波核 —— static Mat GenerateIdealHPKernel(Size kernelSize, float sigma)
        /// <summary>
        /// 生成理想高通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>理想高通滤波核矩阵</returns>
        private static unsafe Mat GenerateIdealHPKernel(Size kernelSize, float sigma)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建理想高通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                if (distance < sigma)
                {
                    kernel.At<float>(rowIndex, colIndex) = 0;
                }
                else
                {
                    kernel.At<float>(rowIndex, colIndex) = 1;
                }
            });

            return kernel;
        }
        #endregion

        #region # 生成理想带通滤波核 —— static Mat GenerateIdealBPKernel(Size kernelSize, float sigma, float bandWidth)
        /// <summary>
        /// 生成理想带通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>理想带通滤波核矩阵</returns>
        private static unsafe Mat GenerateIdealBPKernel(Size kernelSize, float sigma, float bandWidth)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建理想带通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                if (distance >= sigma - bandWidth / 2 && distance <= sigma + bandWidth)
                {
                    kernel.At<float>(rowIndex, colIndex) = 1;
                }
                else
                {
                    kernel.At<float>(rowIndex, colIndex) = 0;
                }
            });

            return kernel;
        }
        #endregion

        #region # 生成理想带阻滤波核 —— static Mat GenerateIdealBRKernel(Size kernelSize, float sigma, float bandWidth)
        /// <summary>
        /// 生成理想带阻滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>理想带阻滤波核矩阵</returns>
        private static unsafe Mat GenerateIdealBRKernel(Size kernelSize, float sigma, float bandWidth)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建理想带阻滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                if (distance >= sigma - bandWidth / 2 && distance <= sigma + bandWidth)
                {
                    kernel.At<float>(rowIndex, colIndex) = 0;
                }
                else
                {
                    kernel.At<float>(rowIndex, colIndex) = 1;
                }
            });

            return kernel;
        }
        #endregion

        #region # 生成高斯低通滤波核 —— static Mat GenerateGaussianLPKernel(Size kernelSize, float sigma)
        /// <summary>
        /// 生成高斯低通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>高斯低通滤波核矩阵</returns>
        private static unsafe Mat GenerateGaussianLPKernel(Size kernelSize, float sigma)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建高斯低通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                kernel.At<float>(rowIndex, colIndex) = (float)Math.Exp(-norm2 / (2.0f * Math.Pow(sigma, 2)));
            });

            return kernel;
        }
        #endregion

        #region # 生成高斯高通滤波核 —— static Mat GenerateGaussianHPKernel(Size kernelSize, float sigma)
        /// <summary>
        /// 生成高斯高通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <returns>高斯高通滤波核矩阵</returns>
        private static unsafe Mat GenerateGaussianHPKernel(Size kernelSize, float sigma)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建高斯高通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                kernel.At<float>(rowIndex, colIndex) = 1.0f - (float)Math.Exp(-norm2 / (2.0f * Math.Pow(sigma, 2.0)));
            });

            return kernel;
        }
        #endregion

        #region # 生成高斯带通滤波核 —— static Mat GenerateGaussianBPKernel(Size kernelSize, float sigma, float bandWidth)
        /// <summary>
        /// 生成高斯带通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>高斯带通滤波核矩阵</returns>
        private static unsafe Mat GenerateGaussianBPKernel(Size kernelSize, float sigma, float bandWidth)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建高斯带通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double distance = Math.Sqrt(norm2);
                double numerator = norm2 - Math.Pow(sigma, 2);
                double denominator = distance * bandWidth;
                double element = -Math.Pow(numerator / denominator, 2);
                kernel.At<float>(rowIndex, colIndex) = (float)Math.Exp(element);
            });

            return kernel;
        }
        #endregion

        #region # 生成高斯带阻滤波核 —— static Mat GenerateGaussianBRKernel(Size kernelSize, float sigma, float bandWidth)
        /// <summary>
        /// 生成高斯带阻滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <returns>高斯带阻滤波核矩阵</returns>
        private static unsafe Mat GenerateGaussianBRKernel(Size kernelSize, float sigma, float bandWidth)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建高斯带阻滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double distance = Math.Sqrt(norm2);
                double numerator = norm2 - Math.Pow(sigma, 2);
                double denominator = distance * bandWidth;
                double element = -Math.Pow(numerator / denominator, 2);
                kernel.At<float>(rowIndex, colIndex) = 1 - (float)Math.Exp(element);
            });

            return kernel;
        }
        #endregion

        #region # 生成高斯同态滤波核 —— static Mat GenerateGaussianHomoKernel(Size kernelSize, float gammaH, float gammaL...
        /// <summary>
        /// 生成高斯同态滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="gammaH">高频增益</param>
        /// <param name="gammaL">低频增益</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="slope">滤波斜率</param>
        /// <returns>高斯同态滤波核矩阵</returns>
        private static unsafe Mat GenerateGaussianHomoKernel(Size kernelSize, float gammaH, float gammaL, float sigma, float slope)
        {
            #region # 验证

            if (gammaH < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(gammaH), "高频增益必须大于等于1！");
            }
            if (gammaL >= 1 || gammaL <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gammaH), "低频增益必须小于1大于0！");
            }
            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slope), "滤波斜率不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);
            double gammaDiff = gammaH - gammaL;

            //构建高斯同态滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double element = gammaDiff * (1 - Math.Exp(-slope * norm2 / (2.0 * Math.Pow(sigma, 2)))) + gammaL;
                kernel.At<float>(rowIndex, colIndex) = (float)element;
            });

            return kernel;
        }
        #endregion

        #region # 生成巴特沃斯低通滤波核 —— static Mat GenerateButterworthLPKernel(Size kernelSize, float sigma, int n)
        /// <summary>
        /// 生成巴特沃斯低通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="n">阶数</param>
        /// <returns>巴特沃斯低通滤波核矩阵</returns>
        private static unsafe Mat GenerateButterworthLPKernel(Size kernelSize, float sigma, int n = 2)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "阶数不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建巴特沃斯低通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                kernel.At<float>(rowIndex, colIndex) = 1.0f / (float)(1.0f + Math.Pow(distance / sigma, 2.0f * n));
            });

            return kernel;
        }
        #endregion

        #region # 生成巴特沃斯高通滤波核 —— static Mat GenerateButterworthHPKernel(Size kernelSize, float sigma, int n)
        /// <summary>
        /// 生成巴特沃斯高通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="n">阶数</param>
        /// <returns>巴特沃斯高通滤波核矩阵</returns>
        private static unsafe Mat GenerateButterworthHPKernel(Size kernelSize, float sigma, int n = 2)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "阶数不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建巴特沃斯高通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double distance = Math.Sqrt(Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2));
                kernel.At<float>(rowIndex, colIndex) = 1.0f - 1.0f / (float)(1.0f + Math.Pow(distance / sigma, 2.0f * n));
            });

            return kernel;
        }
        #endregion

        #region # 生成巴特沃斯带通滤波核 —— static Mat GenerateButterworthBPKernel(Size kernelSize, float sigma, float bandWidth...
        /// <summary>
        /// 生成巴特沃斯带通滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <param name="n">阶数</param>
        /// <returns>巴特沃斯带通滤波核矩阵</returns>
        private static unsafe Mat GenerateButterworthBPKernel(Size kernelSize, float sigma, float bandWidth, int n = 2)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "阶数不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建巴特沃斯带通滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double distance = Math.Sqrt(norm2);
                double element = distance * bandWidth / (norm2 - Math.Pow(sigma, 2));
                double elementPow = Math.Pow(element, 2.0f * n);
                kernel.At<float>(rowIndex, colIndex) = 1.0f - 1.0f / (float)(1.0f + elementPow);
            });

            return kernel;
        }
        #endregion

        #region # 生成巴特沃斯带阻滤波核 —— static Mat GenerateButterworthBRKernel(Size kernelSize, float sigma, float bandWidth...
        /// <summary>
        /// 生成巴特沃斯带阻滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="bandWidth">滤波带宽</param>
        /// <param name="n">阶数</param>
        /// <returns>巴特沃斯带阻滤波核矩阵</returns>
        private static unsafe Mat GenerateButterworthBRKernel(Size kernelSize, float sigma, float bandWidth, int n = 2)
        {
            #region # 验证

            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (bandWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bandWidth), "滤波带宽不可小于等于0！");
            }
            if (n <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(n), "阶数不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);

            //构建巴特沃斯带阻滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double distance = Math.Sqrt(norm2);
                double element = distance * bandWidth / (norm2 - Math.Pow(sigma, 2));
                double elementPow = Math.Pow(element, 2.0f * n);
                kernel.At<float>(rowIndex, colIndex) = 1.0f / (float)(1.0f + elementPow);
            });

            return kernel;
        }
        #endregion

        #region # 生成巴特沃斯同态滤波核 —— static Mat GenerateButterworthHomoKernel(Size kernelSize, float gammaH, float gammaL...
        /// <summary>
        /// 生成巴特沃斯同态滤波核
        /// </summary>
        /// <param name="kernelSize">核矩阵尺寸</param>
        /// <param name="gammaH">高频增益</param>
        /// <param name="gammaL">低频增益</param>
        /// <param name="sigma">滤波半径</param>
        /// <param name="slope">斜率</param>
        /// <returns>巴特沃斯同态滤波核矩阵</returns>
        private static unsafe Mat GenerateButterworthHomoKernel(Size kernelSize, float gammaH, float gammaL, float sigma, float slope)
        {
            #region # 验证

            if (gammaH < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(gammaH), "高频增益必须大于等于1！");
            }
            if (gammaL >= 1 || gammaL <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gammaH), "低频增益必须小于1大于0！");
            }
            if (sigma <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "滤波半径不可小于等于0！");
            }
            if (slope <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slope), "滤波斜率不可小于等于0！");
            }

            #endregion

            Mat kernel = Mat.Zeros(kernelSize, MatType.CV_32FC1);
            Point center = new Point(kernel.Width / 2, kernel.Height / 2);
            double gammaDiff = gammaH - gammaL;

            //构建巴特沃斯同态滤波核
            kernel.ForEachAsFloat((_, positionPtr) =>
            {
                int rowIndex = positionPtr[0];
                int colIndex = positionPtr[1];
                int rowDiff = rowIndex - center.Y;
                int colDiff = colIndex - center.X;
                double norm2 = Math.Pow(rowDiff, 2) + Math.Pow(colDiff, 2);
                double distance = Math.Sqrt(norm2);
                double denominator = 1 + Math.Pow(sigma / slope * distance, 2);
                double element = gammaDiff * (1 / denominator) + gammaL;
                kernel.At<float>(rowIndex, colIndex) = (float)element;
            });

            return kernel;
        }
        #endregion
    }
}
