﻿using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SD.OpenCV.Primitives.Extensions
{
    /// <summary>
    /// 恢复扩展
    /// </summary>
    public static class RecoveryExtension
    {
        #region # 拼接图像 —— static Mat Stitch(this IEnumerable<Mat> matrices)
        /// <summary>
        /// 拼接图像
        /// </summary>
        /// <param name="matrices">图像矩阵集</param>
        /// <returns>融合后图像矩阵</returns>
        public static Mat Stitch(this IEnumerable<Mat> matrices)
        {
            #region # 验证

            matrices = matrices?.ToArray() ?? Array.Empty<Mat>();
            if (!matrices.Any())
            {
                throw new ArgumentNullException(nameof(matrices), "要拼接的图像不可为空！");
            }

            #endregion

            using Stitcher stitcher = Stitcher.Create();
            Mat result = new Mat();
            stitcher.Stitch(matrices, result);

            return result;
        }
        #endregion

        #region # 修复图像 —— static Mat Inpaint(this Mat matrix, Rect rectangle)
        /// <summary>
        /// 修复图像
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="rectangle">修复区域</param>
        /// <returns>修复后图像矩阵</returns>
        public static Mat Inpaint(this Mat matrix, Rect rectangle)
        {
            //定义掩膜
            using Mat mask = matrix.GenerateMask(rectangle);

            //修复图像
            Mat result = new Mat();
            Cv2.Inpaint(matrix, mask, result, 5, InpaintMethod.Telea);

            return result;
        }
        #endregion

        #region # 修复图像 —— static Mat Inpaint(this Mat matrix, IEnumerable<Point> contourPoints)
        /// <summary>
        /// 修复图像
        /// </summary>
        /// <param name="matrix">图像矩阵</param>
        /// <param name="contourPoints">轮廓坐标点集</param>
        /// <returns>修复后图像矩阵</returns>
        public static Mat Inpaint(this Mat matrix, IEnumerable<Point> contourPoints)
        {
            //定义掩膜
            using Mat mask = matrix.GenerateMask(contourPoints);

            //修复图像
            Mat result = new Mat();
            Cv2.Inpaint(matrix, mask, result, 5, InpaintMethod.Telea);

            return result;
        }
        #endregion

        #region # 曝光融合 —— static Mat ExposureFusion(this IEnumerable<Mat> matrices)
        /// <summary>
        /// 曝光融合
        /// </summary>
        /// <param name="matrices">图像矩阵集</param>
        /// <returns>融合后图像矩阵</returns>
        public static Mat ExposureFusion(this IEnumerable<Mat> matrices)
        {
            #region # 验证

            matrices = matrices?.ToArray() ?? Array.Empty<Mat>();
            if (!matrices.Any())
            {
                throw new ArgumentNullException(nameof(matrices), "要融合的图像不可为空！");
            }

            #endregion

            using MergeMertens mergeMertens = MergeMertens.Create();

            //曝光融合
            using Mat mergedResult = new Mat();
            mergeMertens.Process(matrices, mergedResult);

            //标准化
            using Mat normalizedResult = mergedResult * 255;

            //转换格式
            Mat result = new Mat();
            normalizedResult.ConvertTo(result, MatType.CV_8UC3);

            return result;
        }
        #endregion
    }
}
