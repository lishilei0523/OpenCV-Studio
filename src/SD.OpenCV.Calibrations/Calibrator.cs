using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using OpenCvSharp;
using SD.Toolkits.Mathematics.Extensions;
using SD.Toolkits.Mathematics.Models;
using SD.Toolkits.OpenCV.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.Toolkits.OpenCV.Calibrations
{
    /// <summary>
    /// 相机标定者
    /// </summary>
    public static class Calibrator
    {
        #region # 单目标定 —— static CameraIntrinsics MonoCalibrate(string cameraId, float patternSideSize...
        /// <summary>
        /// 单目标定
        /// </summary>
        /// <param name="cameraId">相机Id</param>
        /// <param name="patternSideSize">标定板方格边长</param>
        /// <param name="patternSize">标定板尺寸</param>
        /// <param name="patternType">标定板类型</param>
        /// <param name="maxCount">优化迭代次数</param>
        /// <param name="epsilon">优化误差</param>
        /// <param name="imageSize">图像尺寸</param>
        /// <param name="images">图像字典</param> 
        /// <param name="extrinsicMatrices">外参矩阵字典</param>
        /// <param name="failedImageKeys">失败图像键列表</param>
        /// <returns>相机内参</returns>
        public static CameraIntrinsics MonoCalibrate(string cameraId, float patternSideSize, Size patternSize, PatternType patternType, int maxCount, double epsilon, Size imageSize, IDictionary<string, Mat> images, out IDictionary<string, Matrix<double>> extrinsicMatrices, out ICollection<string> failedImageKeys)
        {
            #region # 验证

            images ??= new Dictionary<string, Mat>();
            if (!images.Any())
            {
                throw new ArgumentNullException(nameof(images), "图片不可为空！");
            }
            if (images.Count < 6)
            {
                throw new ArgumentNullException(nameof(images), "图片不可小于6张！");
            }

            #endregion

            IDictionary<string, ICollection<Point3f>> patternPointsGroup = new Dictionary<string, ICollection<Point3f>>();
            IDictionary<string, ICollection<Point2f>> cornerPointsGroup = new Dictionary<string, ICollection<Point2f>>();
            extrinsicMatrices = new Dictionary<string, Matrix<double>>();
            failedImageKeys = new HashSet<string>();

            //世界坐标系点列表
            ICollection<Point3f> patternPoints = GeneratePatternPoints(patternSideSize, patternSize);

            //获取图像角点
            foreach (KeyValuePair<string, Mat> kv in images)
            {
                bool success;
                ICollection<Point2f> cornerPoints;
                if (patternType == PatternType.Chessboard)
                {
                    success = kv.Value.GetOptimizedChessboardCorners(patternSize, maxCount, epsilon, out cornerPoints);
                }
                else if (patternType == PatternType.CirclesGrid)
                {
                    success = kv.Value.GetOptimizedCirclesGridCorners(patternSize, maxCount, epsilon, out cornerPoints);
                }
                else
                {
                    throw new NotSupportedException("不支持的标定板格式！");
                }

                if (success)
                {
                    patternPointsGroup.Add(kv.Key, patternPoints);
                    cornerPointsGroup.Add(kv.Key, cornerPoints);
                }
                else
                {
                    failedImageKeys.Add(kv.Key);
                }
            }

            //标定相机
            double[,] cameraArray3x3 = new double[3, 3];//内参二维数组3x3
            double[] distortionArray5x1 = new double[5];//畸变一位数组5x1
            double calibratedReprojectionError = Cv2.CalibrateCamera(patternPointsGroup.Values, cornerPointsGroup.Values, imageSize, cameraArray3x3, distortionArray5x1, out Vec3d[] rVecs, out Vec3d[] tVecs);

            //重投影误差
            double reprojectionError = 0;
            for (int i = 0; i < patternPointsGroup.Count; i++)
            {
                KeyValuePair<string, ICollection<Point3f>> patternPointsKV = patternPointsGroup.ElementAt(i);
                KeyValuePair<string, ICollection<Point2f>> cornerPointsKV = cornerPointsGroup.ElementAt(i);
                double[] rArray3x1 = { rVecs[i].Item0, rVecs[i].Item1, rVecs[i].Item2 };
                double[] tArray3x1 = { tVecs[i].Item0, tVecs[i].Item1, tVecs[i].Item2 };

                //计算重投影误差
                Cv2.ProjectPoints(patternPointsKV.Value, rArray3x1, tArray3x1, cameraArray3x3, distortionArray5x1, out Point2f[] imagePoints, out double[,] jacobian);
                using Mat cornerPointsMat = Mat.FromArray(cornerPointsKV.Value);
                using Mat imagePointsMat = Mat.FromArray(imagePoints);
                double unitError = Cv2.Norm(cornerPointsMat, imagePointsMat) / imagePoints.Length;
                reprojectionError += unitError;

                //外参矩阵字典
                Cv2.Rodrigues(rArray3x1, out double[,] rArray3x3, out _);

                Matrix<double> rMatrix = DenseMatrix.OfArray(rArray3x3);
                Vector3D tVector = new Vector3D(tArray3x1[0], tArray3x1[1], tArray3x1[2]);
                Matrix<double> rtMatrix = SpacialExtension.BuildRotationTranslationMatrix(rMatrix, tVector);
                extrinsicMatrices.Add(patternPointsKV.Key, rtMatrix);
            }

            reprojectionError /= patternPointsGroup.Count;
            CameraIntrinsics cameraIntrinsics = new CameraIntrinsics(cameraId, calibratedReprojectionError, reprojectionError, distortionArray5x1, cameraArray3x3);

            return cameraIntrinsics;
        }
        #endregion

        #region # 标定眼在手上 —— static Matrix<double> CalibrateEyeInHand(HandEyeCalibrationMethod calibrationMode...
        /// <summary>
        /// 标定眼在手上
        /// </summary>
        /// <param name="calibrationMode">标定模式</param>
        /// <param name="robotPoses">机器人位姿字典</param>
        /// <param name="extrinsicMatrices">外参矩阵字典</param>
        /// <returns>相机到抓手旋转平移矩阵</returns>
        public static Matrix<double> CalibrateEyeInHand(HandEyeCalibrationMethod calibrationMode, IDictionary<string, Pose> robotPoses, IDictionary<string, Matrix<double>> extrinsicMatrices)
        {
            #region # 验证

            robotPoses ??= new Dictionary<string, Pose>();
            extrinsicMatrices ??= new Dictionary<string, Matrix<double>>();
            if (!robotPoses.Any())
            {
                throw new ArgumentNullException(nameof(robotPoses), "机器人位姿字典不可为空！");
            }
            if (!extrinsicMatrices.Any())
            {
                throw new ArgumentNullException(nameof(extrinsicMatrices), "外参矩阵字典不可为空！");
            }
            if (robotPoses.Count != extrinsicMatrices.Count)
            {
                throw new InvalidOperationException("机器人位姿字典与外参矩阵字典长度不一致！");
            }
            if (robotPoses.Keys.Except(extrinsicMatrices.Keys).Any() || extrinsicMatrices.Keys.Except(robotPoses.Keys).Any())
            {
                throw new InvalidOperationException("机器人位姿字典与外参矩阵字典键不一致！");
            }

            #endregion

            List<Mat> rEndToBaseMats = new List<Mat>();         //抓手到基底旋转矩阵列表
            List<Mat> tEndToBaseMats = new List<Mat>();         //抓手到基底平移向量列表
            List<Mat> rPatternToCameraMats = new List<Mat>();   //标定板到相机旋转矩阵列表
            List<Mat> tPatternToCameraMats = new List<Mat>();   //标定板到相机平移向量列表
            foreach (KeyValuePair<string, Pose> kv in robotPoses)
            {
                Pose pose = kv.Value;
                Matrix<double> extrinsicMatrix = extrinsicMatrices[kv.Key];

                //抓手到基底旋转矩阵
                EulerAngles eulerAngles = pose.GetEulerAngles();
                Matrix<double> rMatrix = eulerAngles.ToRotationMatrix();
                Mat rEndToBaseMat = rMatrix.ToMat();

                //抓手到基底平移向量
                Vector3D tVector = pose.GetTranslationVector();
                Mat tEndToBaseMat = Mat.FromArray(tVector.X, tVector.Y, tVector.Z);

                //分离旋转平移矩阵
                extrinsicMatrix.SplitRotationTranslationMatrix(out Matrix<double> rExtrinsicMatrix, out Vector3D tExtrinsicVector);

                //标定板到相机旋转矩阵
                Mat rPatternToCameraMat = rExtrinsicMatrix.ToMat();

                //标定板到相机平移向量
                Mat tPatternToCameraMat = tExtrinsicVector.ToMat();

                rEndToBaseMats.Add(rEndToBaseMat);
                tEndToBaseMats.Add(tEndToBaseMat);
                rPatternToCameraMats.Add(rPatternToCameraMat);
                tPatternToCameraMats.Add(tPatternToCameraMat);
            }

            //手眼标定
            using Mat rCameraToEndMat = new Mat();
            using Mat tCameraToEndMat = new Mat();
            Cv2.CalibrateHandEye(rEndToBaseMats, tEndToBaseMats, rPatternToCameraMats, tPatternToCameraMats, rCameraToEndMat, tCameraToEndMat, calibrationMode);

            //释放资源
            rEndToBaseMats.ForEach(mat => mat.Dispose());
            tEndToBaseMats.ForEach(mat => mat.Dispose());
            rPatternToCameraMats.ForEach(mat => mat.Dispose());
            tPatternToCameraMats.ForEach(mat => mat.Dispose());

            //相机到抓手旋转平移矩阵
            Matrix<double> rtCameraToEndMatrix = BuildRotationTranslationMatrix(rCameraToEndMat, tCameraToEndMat);

            return rtCameraToEndMatrix;
        }
        #endregion

        #region # 标定眼在手外 —— static Matrix<double> CalibrateEyeToHand(HandEyeCalibrationMethod calibrationMode...
        /// <summary>
        /// 标定眼在手外
        /// </summary>
        /// <param name="calibrationMode">标定模式</param>
        /// <param name="robotPoses">机器人位姿字典</param>
        /// <param name="extrinsicMatrices">外参矩阵字典</param>
        /// <returns>相机到基底旋转平移矩阵</returns>
        public static Matrix<double> CalibrateEyeToHand(HandEyeCalibrationMethod calibrationMode, IDictionary<string, Pose> robotPoses, IDictionary<string, Matrix<double>> extrinsicMatrices)
        {
            #region # 验证

            robotPoses ??= new Dictionary<string, Pose>();
            extrinsicMatrices ??= new Dictionary<string, Matrix<double>>();
            if (!robotPoses.Any())
            {
                throw new ArgumentNullException(nameof(robotPoses), "机器人位姿字典不可为空！");
            }
            if (!extrinsicMatrices.Any())
            {
                throw new ArgumentNullException(nameof(extrinsicMatrices), "外参矩阵字典不可为空！");
            }
            if (robotPoses.Count != extrinsicMatrices.Count)
            {
                throw new InvalidOperationException("机器人位姿字典与外参矩阵字典长度不一致！");
            }
            if (robotPoses.Keys.Except(extrinsicMatrices.Keys).Any() || extrinsicMatrices.Keys.Except(robotPoses.Keys).Any())
            {
                throw new InvalidOperationException("机器人位姿字典与外参矩阵字典键不一致！");
            }

            #endregion

            List<Mat> rBaseToEndMats = new List<Mat>();        //基底到抓手旋转矩阵列表
            List<Mat> tBaseToEndMats = new List<Mat>();        //基底到抓手平移向量列表
            List<Mat> rPatternToCameraMats = new List<Mat>();  //标定板到相机旋转矩阵列表
            List<Mat> tPatternToCameraMats = new List<Mat>();  //标定板到相机平移向量列表
            foreach (KeyValuePair<string, Pose> kv in robotPoses)
            {
                Pose pose = kv.Value;
                Matrix<double> extrinsicMatrix = extrinsicMatrices[kv.Key];

                //抓手到基底旋转平移矩阵
                Matrix<double> rtBaseToEndMatrix = pose.ToRotationTranslationMatrix();

                //基底到抓手旋转平移矩阵
                Matrix<double> rtEndToBaseMatrix = rtBaseToEndMatrix.Inverse();
                rtEndToBaseMatrix.SplitRotationTranslationMatrix(out Matrix<double> rEndToBaseMatrix, out Vector3D tEndToBaseVector);

                //基底到抓手旋转矩阵
                Mat rEndToBaseMat = rEndToBaseMatrix.ToMat();

                //基底到抓手平移向量
                Mat tEndToBaseMat = tEndToBaseVector.ToMat();

                //分离旋转平移矩阵
                extrinsicMatrix.SplitRotationTranslationMatrix(out Matrix<double> rExtrinsicMatrix, out Vector3D tExtrinsicVector);

                //标定板到相机旋转矩阵
                Mat rPatternToCameraMat = rExtrinsicMatrix.ToMat();

                //标定板到相机平移向量
                Mat tPatternToCameraMat = tExtrinsicVector.ToMat();

                rBaseToEndMats.Add(rEndToBaseMat);
                tBaseToEndMats.Add(tEndToBaseMat);
                rPatternToCameraMats.Add(rPatternToCameraMat);
                tPatternToCameraMats.Add(tPatternToCameraMat);
            }

            //手眼标定
            using Mat rCameraToBaseMat = new Mat();
            using Mat tCameraToBaseMat = new Mat();
            Cv2.CalibrateHandEye(rBaseToEndMats, tBaseToEndMats, rPatternToCameraMats, tPatternToCameraMats, rCameraToBaseMat, tCameraToBaseMat, calibrationMode);

            //释放资源
            rBaseToEndMats.ForEach(mat => mat.Dispose());
            tBaseToEndMats.ForEach(mat => mat.Dispose());
            rPatternToCameraMats.ForEach(mat => mat.Dispose());
            tPatternToCameraMats.ForEach(mat => mat.Dispose());

            //相机到基底旋转平移矩阵
            Matrix<double> rtCameraToBaseMatrix = BuildRotationTranslationMatrix(rCameraToBaseMat, tCameraToBaseMat);

            return rtCameraToBaseMatrix;
        }
        #endregion

        #region # 解析外参矩阵 —— static Matrix<double> SolveExtrinsicMatrix(int patternSideSize, Size patternSize...
        /// <summary>
        /// 解析外参矩阵
        /// </summary>
        /// <param name="patternSideSize">标定板方格边长</param>
        /// <param name="patternSize">标定板尺寸</param>
        /// <param name="patternType">标定板类型</param>
        /// <param name="maxCount">优化迭代次数</param>
        /// <param name="epsilon">优化误差</param>
        /// <param name="image">图像</param>
        /// <param name="cameraIntrinsics">相机内参</param>
        /// <returns>外参矩阵</returns>
        public static Matrix<double> SolveExtrinsicMatrix(int patternSideSize, Size patternSize, PatternType patternType, int maxCount, double epsilon, Mat image, CameraIntrinsics cameraIntrinsics)
        {
            #region # 验证

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "图像不可为空！");
            }
            if (cameraIntrinsics == null)
            {
                throw new ArgumentNullException(nameof(cameraIntrinsics), "相机内参不可为空！");
            }

            #endregion

            bool success;
            ICollection<Point2f> cornerPoints;
            if (patternType == PatternType.Chessboard)
            {
                success = image.GetOptimizedChessboardCorners(patternSize, maxCount, epsilon, out cornerPoints);
            }
            else if (patternType == PatternType.CirclesGrid)
            {
                success = image.GetOptimizedCirclesGridCorners(patternSize, maxCount, epsilon, out cornerPoints);
            }
            else
            {
                throw new NotSupportedException("不支持的标定板格式！");
            }

            if (success)
            {
                ICollection<Point3f> patternPoints = GeneratePatternPoints(patternSideSize, patternSize);
                Matrix<double> extrinsicMatrix = SolveExtrinsicMatrix(patternPoints, cornerPoints, cameraIntrinsics);

                return extrinsicMatrix;
            }

            throw new ArithmeticException("未获取足够角点！");
        }
        #endregion

        #region # 解析外参矩阵 —— static Matrix<double> SolveExtrinsicMatrix(IEnumerable<Point3f> patternPoints...
        /// <summary>
        /// 解析外参矩阵
        /// </summary>
        /// <param name="patternPoints">世界坐标系点列表</param>
        /// <param name="cornerPoints">图像角点列表</param>
        /// <param name="cameraIntrinsics">相机内参</param>
        /// <returns>外参矩阵</returns>
        public static Matrix<double> SolveExtrinsicMatrix(IEnumerable<Point3f> patternPoints, IEnumerable<Point2f> cornerPoints, CameraIntrinsics cameraIntrinsics)
        {
            #region # 验证

            patternPoints = patternPoints?.ToArray() ?? Array.Empty<Point3f>();
            cornerPoints = cornerPoints?.ToArray() ?? Array.Empty<Point2f>();
            if (!patternPoints.Any())
            {
                throw new ArgumentNullException(nameof(patternPoints), "世界坐标系点列表不可为空！");
            }
            if (!cornerPoints.Any())
            {
                throw new ArgumentNullException(nameof(cornerPoints), "图像角点列表不可为空！");
            }
            if (patternPoints.Count() != cornerPoints.Count())
            {
                throw new InvalidOperationException("世界坐标系点与图像角点长度不一致！");
            }

            #endregion

            double[] rArray3x1 = { };
            double[] tArray3x1 = { };
            Cv2.SolvePnP(patternPoints, cornerPoints, cameraIntrinsics.IntrinsicMatrix, cameraIntrinsics.DistortionVector.ToArray(), ref rArray3x1, ref tArray3x1);

            //旋转向量转旋转矩阵
            Cv2.Rodrigues(rArray3x1, out double[,] rArray3x3, out _);

            //构造RT矩阵
            Matrix<double> rMatrix = DenseMatrix.OfArray(rArray3x3);
            Vector3D tVector = new Vector3D(tArray3x1[0], tArray3x1[1], tArray3x1[2]);
            Matrix<double> rtPatternToCameraMatrix = SpacialExtension.BuildRotationTranslationMatrix(rMatrix, tVector);

            return rtPatternToCameraMatrix;
        }
        #endregion

        #region # 矫正畸变 —— static Mat RectifyDistortions(this Mat image, CameraIntrinsics cameraIntrinsics)
        /// <summary>
        /// 矫正畸变
        /// </summary>
        /// <param name="image">图像矩阵</param>
        /// <param name="cameraIntrinsics">相机内参</param>
        /// <returns>矫正图像矩阵</returns>
        public static Mat RectifyDistortions(this Mat image, CameraIntrinsics cameraIntrinsics)
        {
            #region # 验证

            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "图像不可为空！");
            }
            if (cameraIntrinsics == null)
            {
                throw new ArgumentNullException(nameof(cameraIntrinsics), "相机内参不可为空！");
            }

            #endregion

            Mat result = new Mat();
            Cv2.Undistort(image, result, InputArray.Create(cameraIntrinsics.IntrinsicMatrix), InputArray.Create(cameraIntrinsics.DistortionVector));

            return result;
        }
        #endregion

        #region # 矫正畸变 —— static IDictionary<string, Mat> RectifyDistortions(this IDictionary<string, Mat> images...
        /// <summary>
        /// 矫正畸变
        /// </summary>
        /// <param name="images">图像矩阵字典</param>
        /// <param name="cameraIntrinsics">相机内参</param>
        /// <returns>矫正图像矩阵字典</returns>
        public static IDictionary<string, Mat> RectifyDistortions(this IDictionary<string, Mat> images, CameraIntrinsics cameraIntrinsics)
        {
            #region # 验证

            images ??= new Dictionary<string, Mat>();
            if (!images.Any())
            {
                return new Dictionary<string, Mat>();
            }
            if (images.Values.Select(x => x.Width).Distinct().Count() != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(images), "图像宽度不一致");
            }
            if (images.Values.Select(x => x.Height).Distinct().Count() != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(images), "图像高度不一致");
            }
            if (cameraIntrinsics == null)
            {
                throw new ArgumentNullException(nameof(cameraIntrinsics), "相机内参不可为空！");
            }

            #endregion

            using Mat map1 = new Mat();
            using Mat map2 = new Mat();
            using Mat r = new Mat();
            Size imageSize = images.Values.First().Size();
            Cv2.InitUndistortRectifyMap(InputArray.Create(cameraIntrinsics.IntrinsicMatrix), InputArray.Create(cameraIntrinsics.DistortionVector), r, InputArray.Create(cameraIntrinsics.IntrinsicMatrix), imageSize, MatType.CV_16SC2, map1, map2);

            IDictionary<string, Mat> results = new Dictionary<string, Mat>();
            foreach (KeyValuePair<string, Mat> kv in images)
            {
                Mat result = new Mat();
                Cv2.Remap(kv.Value, result, map1, map2);
                results.Add(kv.Key, result);
            }

            return results;
        }
        #endregion

        #region # 验证对极约束 —— static double[] CheckEpipolarConstraints(Mat cameraMat...
        /// <summary>
        /// 验证对极约束
        /// </summary>
        /// <param name="cameraMat">相机内参矩阵</param>
        /// <param name="sourcePoints">源关键点集</param>
        /// <param name="targetPoints">目标关键点集</param>
        /// <param name="rMat">旋转矩阵</param>
        /// <param name="tMat">平移矩阵</param>
        /// <returns>对极约束列表</returns>
        public static double[] CheckEpipolarConstraints(Mat cameraMat, IList<Point2f> sourcePoints, IList<Point2f> targetPoints, Mat rMat, Mat tMat)
        {
            IList<double> epipolarConstraints = new List<double>();
            for (int i = 0; i < sourcePoints.Count; i++)
            {
                double[] y1Array = { sourcePoints[i].X, sourcePoints[i].Y, 1 };
                double[] y2Array = { targetPoints[i].X, targetPoints[i].Y, 1 };
                Mat y1 = Mat.FromArray(y1Array);
                Mat y2 = Mat.FromArray(y2Array);

                double[,] txArray =
                {
                    {0, -tMat.At<double>(2, 0), tMat.At<double>(1, 0)},
                    {tMat.At<double>(2, 0), 0, -tMat.At<double>(0, 0)},
                    {-tMat.At<double>(1, 0), tMat.At<double>(0, 0), 0}
                };
                Mat txMat = Mat.FromArray(txArray);

                Mat d = y2.T() * cameraMat.Inv().T() * txMat * rMat * cameraMat.Inv() * y1;
                epipolarConstraints.Add(d.At<double>(0, 0));
            }

            return epipolarConstraints.ToArray();
        }
        #endregion

        #region # 验证对极约束 —— static double[] CheckEpipolarConstraints(Matrix<double> cameraMatrix...
        /// <summary>
        /// 验证对极约束
        /// </summary>
        /// <param name="cameraMatrix">相机内参矩阵</param>
        /// <param name="sourcePoints">源关键点集</param>
        /// <param name="targetPoints">目标关键点集</param>
        /// <param name="rMatrix">旋转矩阵</param>
        /// <param name="tMatrix">平移矩阵</param>
        /// <returns>对极约束列表</returns>
        public static double[] CheckEpipolarConstraints(Matrix<double> cameraMatrix, IList<Point2f> sourcePoints, IList<Point2f> targetPoints, Matrix<double> rMatrix, Matrix<double> tMatrix)
        {
            IList<double> epipolarConstraints = new List<double>();
            for (int i = 0; i < sourcePoints.Count; i++)
            {
                double[] y1Array = { sourcePoints[i].X, sourcePoints[i].Y, 1 };
                double[] y2Array = { targetPoints[i].X, targetPoints[i].Y, 1 };
                Vector<double> y1 = DenseVector.OfArray(y1Array);
                Vector<double> y2 = DenseVector.OfArray(y2Array);

                double[,] txArray =
                {
                    { 0, -tMatrix[2, 0], tMatrix[1, 0] },
                    { tMatrix[2, 0], 0, -tMatrix[0, 0] },
                    { -tMatrix[1, 0], tMatrix[0, 0], 0 }
                };
                Matrix<double> txMatrix = DenseMatrix.OfArray(txArray);

                Vector<double> d = y2.ToRowMatrix() * cameraMatrix.Inverse().Transpose() * txMatrix * rMatrix * cameraMatrix.Inverse() * y1;
                epipolarConstraints.Add(d.Single());
            }

            return epipolarConstraints.ToArray();
        }
        #endregion


        #region # 生成标定板世界坐标点列表 —— static ICollection<Point3f> GeneratePatternPoints(float patternSideSize...
        /// <summary>
        /// 生成标定板世界坐标点列表
        /// </summary>
        /// <param name="patternSideSize">标定板方格边长</param>
        /// <param name="patternSize">标定板尺寸</param>
        /// <returns>标定板世界坐标点列表</returns>
        public static ICollection<Point3f> GeneratePatternPoints(float patternSideSize, Size patternSize)
        {
            IList<Point3f> patternPoints = new List<Point3f>();
            for (int rowIndex = 0; rowIndex < patternSize.Height; rowIndex++)
            {
                for (int colIndex = 0; colIndex < patternSize.Width; colIndex++)
                {
                    Point3f point = new Point3f(colIndex * patternSideSize, rowIndex * patternSideSize, 0);
                    patternPoints.Add(point);
                }
            }

            return patternPoints;
        }
        #endregion

        #region # 获取优化的棋盘格角点列表 —— static bool GetOptimizedChessboardCorners(this Mat image, Size patternSize...
        /// <summary>
        /// 获取优化的棋盘格角点列表
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="patternSize">标定板尺寸</param>
        /// <param name="cornerPoints">角点列表</param>
        /// <param name="maxCount">优化迭代次数</param>
        /// <param name="epsilon">优化误差</param>
        /// <returns>是否成功</returns>
        public static bool GetOptimizedChessboardCorners(this Mat image, Size patternSize, int maxCount, double epsilon, out ICollection<Point2f> cornerPoints)
        {
            bool success = Cv2.FindChessboardCorners(image, patternSize, out Point2f[] keyPoints);
            if (success)
            {
                //优化角点
                TermCriteria criteria = new TermCriteria(CriteriaTypes.MaxIter | CriteriaTypes.Eps, maxCount, epsilon);
                cornerPoints = Cv2.CornerSubPix(image, keyPoints, new Size(11, 11), new Size(-1, -1), criteria);
            }
            else
            {
                cornerPoints = new List<Point2f>();
            }

            return success;
        }
        #endregion

        #region # 获取优化的圆形格角点列表 —— static bool GetOptimizedCirclesGridCorners(this Mat image, Size patternSize...
        /// <summary>
        /// 获取优化的圆形格角点列表
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="patternSize">标定板尺寸</param>
        /// <param name="cornerPoints">角点列表</param>
        /// <param name="maxCount">优化迭代次数</param>
        /// <param name="epsilon">优化误差</param>
        /// <returns>是否成功</returns>
        public static bool GetOptimizedCirclesGridCorners(this Mat image, Size patternSize, int maxCount, double epsilon, out ICollection<Point2f> cornerPoints)
        {
            bool success = Cv2.FindCirclesGrid(image, patternSize, out Point2f[] keyPoints);
            if (success)
            {
                //优化角点
                TermCriteria criteria = new TermCriteria(CriteriaTypes.Eps | CriteriaTypes.MaxIter, maxCount, epsilon);
                cornerPoints = Cv2.CornerSubPix(image, keyPoints, new Size(11, 11), new Size(-1, -1), criteria);
            }
            else
            {
                cornerPoints = new List<Point2f>();
            }

            return success;
        }
        #endregion


        #region # Vector3D转Mat —— static Mat ToMat(this Vector3D vector3D)
        /// <summary>
        /// Vector3D转Mat
        /// </summary>
        public static Mat ToMat(this Vector3D vector3D)
        {
            Mat mat = new Mat(3, 1, MatType.CV_64FC1);
            mat.Set(0, 0, vector3D.X);
            mat.Set(1, 0, vector3D.Y);
            mat.Set(2, 0, vector3D.Z);

            return mat;
        }
        #endregion

        #region # Mat转Vector3D —— static Vector3D ToVector3D(this Mat mat)
        /// <summary>
        /// Mat转Vector3D
        /// </summary>
        public static Vector3D ToVector3D(this Mat mat)
        {
            Vector3D vector3D = new Vector3D(mat.At<double>(0, 0), mat.At<double>(1, 0), mat.At<double>(2, 0));

            return vector3D;
        }
        #endregion

        #region # Matrix转Mat —— static Mat ToMat(this Matrix<double> matrix)
        /// <summary>
        /// Matrix转Mat
        /// </summary>
        public static Mat ToMat(this Matrix<double> matrix)
        {
            Mat mat = new Mat(matrix.RowCount, matrix.ColumnCount, MatType.CV_64FC1);
            for (int rowIndex = 0; rowIndex < matrix.RowCount; rowIndex++)
            {
                for (int colIndex = 0; colIndex < matrix.ColumnCount; colIndex++)
                {
                    mat.Set(rowIndex, colIndex, matrix[rowIndex, colIndex]);
                }
            }

            return mat;
        }
        #endregion

        #region # Mat转Matrix —— static Matrix ToMatrix(this Mat mat)
        /// <summary>
        /// Mat转Matrix
        /// </summary>
        public static Matrix ToMatrix(this Mat mat)
        {
            Matrix matrix = DenseMatrix.Create(mat.Rows, mat.Cols, 0);
            for (int rowIndex = 0; rowIndex < mat.Rows; rowIndex++)
            {
                for (int colIndex = 0; colIndex < mat.Cols; colIndex++)
                {
                    matrix[rowIndex, colIndex] = mat.At<double>(rowIndex, colIndex);
                }
            }

            return matrix;
        }
        #endregion

        #region # 构造旋转平移矩阵 —— static Matrix<double> BuildRotationTranslationMatrix(Mat rMat, Mat tMat)
        /// <summary>
        /// 构造旋转平移矩阵
        /// </summary>
        /// <param name="rMat">旋转矩阵3x3</param>
        /// <param name="tMat">平移矩阵3x1</param>
        /// <returns>旋转平移矩阵</returns>
        public static Matrix<double> BuildRotationTranslationMatrix(Mat rMat, Mat tMat)
        {
            Matrix<double> rMatrix = rMat.ToMatrix();
            Vector3D tVector = tMat.ToVector3D();
            Matrix<double> rtMatrix = SpacialExtension.BuildRotationTranslationMatrix(rMatrix, tVector);

            return rtMatrix;
        }
        #endregion
    }
}
