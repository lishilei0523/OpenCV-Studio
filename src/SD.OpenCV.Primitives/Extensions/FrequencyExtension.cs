using OpenCvSharp;

namespace SD.Toolkits.OpenCV.Extensions
{
    /// <summary>
    /// 频率域扩展
    /// </summary>
    public static class FrequencyExtension
    {
        #region # 频率域滤波 —— static Mat FrequencyBlur(this Mat borderedMatrix, Mat kernelMatrix)
        /// <summary>
        /// 频率域滤波
        /// </summary>
        /// <param name="borderedMatrix">傅里叶边框矩阵</param>
        /// <param name="kernelMatrix">滤波核矩阵</param>
        /// <returns>滤波效果矩阵</returns>
        public static Mat FrequencyBlur(this Mat borderedMatrix, Mat kernelMatrix)
        {
            using Mat zeroMatrix = Mat.Zeros(borderedMatrix.Size(), MatType.CV_32FC1);
            using Mat complexMatrix = new Mat();
            Mat[] planes = { borderedMatrix, zeroMatrix };
            Cv2.Merge(planes, complexMatrix);

            //离散傅里叶变换
            Cv2.Dft(complexMatrix, complexMatrix, DftFlags.ComplexOutput);

            //分割实部与虚部，planes[0]为实部，planes[1]为虚部
            Cv2.Split(complexMatrix, out planes);

            //迁移频域
            planes[0].ShiftDFT();
            planes[1].ShiftDFT();

            //滤波器函数与DFT结果的乘积
            using Mat blurReal = new Mat();
            using Mat blurImag = new Mat();
            Mat blur = new Mat();
            Cv2.Multiply(planes[0], kernelMatrix, blurReal);  // 滤波（实部与滤波器模板对应元素相乘）
            Cv2.Multiply(planes[1], kernelMatrix, blurImag);  // 滤波（虚部与滤波器模板对应元素相乘）
            Mat[] blurs = { blurReal, blurImag };

            //再次迁移进行逆变换
            blurReal.ShiftDFT();
            blurImag.ShiftDFT();

            //实部与虚部合并
            Cv2.Merge(blurs, blur);

            //离散傅里叶逆变换
            Cv2.Idft(blur, blur, DftFlags.ComplexOutput);
            blur = blur / blur.Rows / blur.Cols;

            //分离通道，主要获取通道
            Cv2.Split(blur, out Mat[] blurPlanes);

            blur.Dispose();
            blurPlanes[1].Dispose();

            return blurPlanes[0];
        }
        #endregion

        #region # 生成傅里叶变换边框图像矩阵 —— static Mat GenerateDFTBorderedMatrix(this Mat matrix)
        /// <summary>
        /// 生成傅里叶变换边框图像矩阵
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <returns>边框图像矩阵</returns>
        public static Mat GenerateDFTBorderedMatrix(this Mat matrix)
        {
            Size matrixSize = matrix.Size();
            int originalWidth = matrixSize.Width;
            int originalHeight = matrixSize.Height;
            int optimalWidth = Cv2.GetOptimalDFTSize(matrixSize.Width);    //获取DFT变换的最佳宽度
            int optimalHeight = Cv2.GetOptimalDFTSize(matrixSize.Height);  //获取DFT变换的最佳高度

            Mat borderedMatrix = new Mat();
            Cv2.CopyMakeBorder(matrix, borderedMatrix, 0, optimalHeight - originalHeight, 0, optimalWidth - originalWidth, BorderTypes.Constant, Scalar.All(0));
            borderedMatrix.ConvertTo(borderedMatrix, MatType.CV_32FC1);

            return borderedMatrix;
        }
        #endregion

        #region # 迁移频域 —— static void ShiftDFT(this Mat magnitudeMatrix)
        /// <summary>
        /// 迁移频域
        /// </summary>
        /// <param name="magnitudeMatrix">频谱矩阵</param>
        public static void ShiftDFT(this Mat magnitudeMatrix)
        {
            //一分为四，左上与右下交换，右上与左下交换
            //重新排列傅里叶图像中的象限，使原点位于图像中心
            int magnitudeCx = magnitudeMatrix.Cols / 2;
            int magnitudeCy = magnitudeMatrix.Rows / 2;
            using Mat magnitudeQ0 = new Mat(magnitudeMatrix, new Rect(0, 0, magnitudeCx, magnitudeCy));                         //左上
            using Mat magnitudeQ1 = new Mat(magnitudeMatrix, new Rect(magnitudeCx, 0, magnitudeCx, magnitudeCy));               //右上
            using Mat magnitudeQ2 = new Mat(magnitudeMatrix, new Rect(0, magnitudeCy, magnitudeCx, magnitudeCy));               //左下
            using Mat magnitudeQ3 = new Mat(magnitudeMatrix, new Rect(magnitudeCx, magnitudeCy, magnitudeCx, magnitudeCy));     //右下

            //交换象限
            using Mat exchangedMatrix = new Mat();

            //左上角与右下角对换
            magnitudeQ0.CopyTo(exchangedMatrix);
            magnitudeQ3.CopyTo(magnitudeQ0);
            exchangedMatrix.CopyTo(magnitudeQ3);

            //右上角与左下角对换
            magnitudeQ1.CopyTo(exchangedMatrix);
            magnitudeQ2.CopyTo(magnitudeQ1);
            exchangedMatrix.CopyTo(magnitudeQ2);
        }
        #endregion

        #region # 生成频率谱图 —— static Mat GenerateFrequencySpectrum(this Mat matrix)
        /// <summary>
        /// 生成频率谱图
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <returns>频率谱图矩阵</returns>
        public static Mat GenerateFrequencySpectrum(this Mat matrix)
        {
            //为傅立叶变换的结果分配存储空间
            //将planes数组组合成一个多通道的数组，两个通道搭配，分别保存实部和虚部
            //傅里叶变换的结果是复数，这就是说对于每个图像原像素值，会有两个图像值
            //此外，频域值范围远远超过图象值范围，因此至少将频域储存在float中
            //所以我们将输入图像转换成浮点型，并且多加一个额外通道来存储复数部分
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat zeroMatrix = Mat.Zeros(borderedMatrix.Size(), MatType.CV_32FC1);
            using Mat complexMatrix = new Mat();
            Mat[] planes = { borderedMatrix, zeroMatrix };
            Cv2.Merge(planes, complexMatrix);

            //离散傅里叶变换
            Cv2.Dft(complexMatrix, complexMatrix, DftFlags.ComplexOutput);

            //分割实部与虚部，planes[0]为实部，planes[1]为虚部
            Cv2.Split(complexMatrix, out planes);

            //将复数转为幅值，保存在planes[0]
            Cv2.Magnitude(planes[0], planes[1], planes[0]);
            Mat magnitudeMatrix = planes[0];

            //幅度太大，适用自然对数尺度替换线性尺度
            magnitudeMatrix += Scalar.All(1);
            Cv2.Log(magnitudeMatrix + 1, magnitudeMatrix);

            //行、列都为偶数，如果有奇数行或奇数列，进行频谱裁剪
            magnitudeMatrix = magnitudeMatrix[new Rect(0, 0, magnitudeMatrix.Cols & -2, magnitudeMatrix.Rows & -2)];

            //迁移频域
            magnitudeMatrix.ShiftDFT();

            //原幅度值仍更不便于显示，归一化后方便显示
            Cv2.Normalize(magnitudeMatrix, magnitudeMatrix, 0, 1, NormTypes.MinMax);

            return magnitudeMatrix;
        }
        #endregion

        #region # 生成相位谱图 —— static Mat GeneratePhaseSpectrum(this Mat matrix)
        /// <summary>
        /// 生成相位谱图
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <returns>相位谱图矩阵</returns>
        public static Mat GeneratePhaseSpectrum(this Mat matrix)
        {
            //为傅立叶变换的结果分配存储空间
            //将planes数组组合成一个多通道的数组，两个通道搭配，分别保存实部和虚部
            //傅里叶变换的结果是复数，这就是说对于每个图像原像素值，会有两个图像值
            //此外，频域值范围远远超过图象值范围，因此至少将频域储存在float中
            //所以我们将输入图像转换成浮点型，并且多加一个额外通道来存储复数部分
            using Mat borderedMatrix = matrix.GenerateDFTBorderedMatrix();
            using Mat zeroMatrix = Mat.Zeros(borderedMatrix.Size(), MatType.CV_32FC1);
            using Mat complexMatrix = new Mat();
            Mat[] planes = { borderedMatrix, zeroMatrix };
            Cv2.Merge(planes, complexMatrix);

            //离散傅里叶变换
            Cv2.Dft(complexMatrix, complexMatrix, DftFlags.ComplexOutput);

            //分割实部与虚部，planes[0]为实部，planes[1]为虚部
            Cv2.Split(complexMatrix, out planes);

            //将复数转为相位值，保存在planes[0]
            Cv2.Phase(planes[0], planes[1], planes[0]);
            Mat phaseMatrix = planes[0];

            //适用自然对数尺度替换线性尺度
            phaseMatrix += Scalar.All(1);
            Cv2.Log(phaseMatrix + 1, phaseMatrix);

            //行、列都为偶数，如果有奇数行或奇数列，进行频谱裁剪
            phaseMatrix = phaseMatrix[new Rect(0, 0, phaseMatrix.Cols & -2, phaseMatrix.Rows & -2)];

            //迁移频域
            phaseMatrix.ShiftDFT();

            //归一化后方便显示
            Cv2.Normalize(phaseMatrix, phaseMatrix, 0, 1, NormTypes.MinMax);

            return phaseMatrix;
        }
        #endregion
    }
}
