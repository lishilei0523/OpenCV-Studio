namespace SD.Toolkits.OpenCV.Models
{
    /// <summary>
    /// 相机内参
    /// </summary>
    public class CameraIntrinsics
    {
        #region # 构造器

        #region 00.无参构造器
        /// <summary>
        /// 无参构造器
        /// </summary>
        public CameraIntrinsics() { }
        #endregion

        #region 02.创建相机内参构造器
        /// <summary>
        /// 创建相机内参构造器
        /// </summary>
        /// <param name="cameraId">相机Id</param>
        /// <param name="calibratedReprojectionError">标定重投影误差</param>
        /// <param name="reprojectionError">重投影误差</param>
        /// <param name="distortionVector">畸变向量</param>
        /// <param name="intrinsicMatrix">内参矩阵</param>
        public CameraIntrinsics(string cameraId, double calibratedReprojectionError, double reprojectionError, double[] distortionVector, double[,] intrinsicMatrix)
            : this()
        {
            this.CameraId = cameraId;
            this.CalibratedReprojectionError = calibratedReprojectionError;
            this.ReprojectionError = reprojectionError;
            this.DistortionVector = distortionVector;
            this.IntrinsicMatrix = intrinsicMatrix;
        }
        #endregion

        #endregion

        #region # 属性

        #region 相机Id —— string CameraId
        /// <summary>
        /// 相机Id
        /// </summary>
        public string CameraId { get; private set; }
        #endregion

        #region 标定重投影误差 —— double CalibratedReprojectionError
        /// <summary>
        /// 标定重投影误差
        /// </summary>
        public double CalibratedReprojectionError { get; private set; }
        #endregion

        #region 重投影误差 —— double ReprojectionError
        /// <summary>
        /// 重投影误差
        /// </summary>
        public double ReprojectionError { get; private set; }
        #endregion

        #region 畸变向量 —— double[] DistortionVector
        /// <summary>
        /// 畸变向量
        /// </summary>
        /// <remarks>5x1向量: 一维数组</remarks>
        public double[] DistortionVector { get; private set; }
        #endregion 

        #region 内参矩阵 —— double[,] IntrinsicMatrix
        /// <summary>
        /// 内参矩阵
        /// </summary>
        /// <remarks>3x3矩阵: 二维数组</remarks>
        public double[,] IntrinsicMatrix { get; private set; }
        #endregion

        #endregion
    }
}
