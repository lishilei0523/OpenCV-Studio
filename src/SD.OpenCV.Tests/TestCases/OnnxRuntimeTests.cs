using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using SD.OpenCV.OnnxRuntime.Models;
using SD.OpenCV.OnnxRuntime.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SD.OpenCV.Tests.TestCases
{
    /// <summary>
    /// OnnxRuntime测试
    /// </summary>
    [TestClass]
    public class OnnxRuntimeTests
    {
        #region # 测试ResNet —— void TestResNet()
        /// <summary>
        /// 测试ResNet
        /// </summary>
        [TestMethod]
        public void TestResNet()
        {
            const string modelPath = @"F:\Files\Models\ResNet-ONNX\resnet50-v2-7.onnx";
            const string imagePath = "Content/Images/dog.jpg";

            using Mat image = Cv2.ImRead(imagePath);

            Stopwatch stopwatch1 = new Stopwatch();
            Stopwatch stopwatch2 = new Stopwatch();

            stopwatch1.Start();
            ResNet resNet = new ResNet(modelPath);
            stopwatch1.Stop();
            Trace.WriteLine($"构造器耗时: {stopwatch1.Elapsed}");

            stopwatch2.Start();
            resNet.StartSession();
            stopwatch2.Stop();
            Trace.WriteLine($"启动会话耗时: {stopwatch2.Elapsed}");

            for (int i = 0; i < 100; i++)
            {
                Stopwatch stopwatch3 = new Stopwatch();

                stopwatch3.Start();
                Prediction[] predictions = resNet.Infer(image);
                stopwatch3.Stop();
                Trace.WriteLine($"推理耗时: {stopwatch3.Elapsed}");

                foreach (Prediction prediction in predictions)
                {
                    Trace.WriteLine($"{prediction.Label}: {prediction.Confidence:F2}");
                }

                Trace.WriteLine("----------------------------------");
                Thread.Sleep(20);
            }

            Stopwatch stopwatch4 = new Stopwatch();
            stopwatch4.Start();
            resNet.Dispose();
            stopwatch4.Stop();
            Console.WriteLine($"释放耗时: {stopwatch4.Elapsed}");
        }
        #endregion

        #region # 测试FasterRCNN —— void TestFasterRcnn()
        /// <summary>
        /// 测试FasterRCNN
        /// </summary>
        [TestMethod]
        public void TestFasterRcnn()
        {
            const string modelPath = @"F:\Files\Models\FasterRCNN-ONNX\FasterRCNN-10.onnx";
            const string imagePath = "Content/Images/scene1.jpg";

            using Mat image = Cv2.ImRead(imagePath);
            using FasterRcnn fasterRcnn = new FasterRcnn(modelPath);
            fasterRcnn.StartSession();
            Detection[] detections = fasterRcnn.Infer(image);

            using Mat targetImage = image.Clone();
            foreach (Detection detection in detections)
            {
                //打印结果
                string box = $"({detection.Box.Location.X},{detection.Box.Location.Y})|{detection.Box.Width}*{detection.Box.Height}";
                Trace.WriteLine($"{detection.Label}: {detection.Confidence:F2}, Box: {box}");

                //绘制矩形框
                targetImage.Rectangle(detection.Box, Scalar.Red);

                //绘制文本
                string labelConfidence = $"{detection.Label}: {detection.Confidence:F2}";
                targetImage.PutText(labelConfidence, detection.Box.Location, HersheyFonts.HersheyPlain, 1, Scalar.Yellow);
            }

            //Cv2.NamedWindow("原图");
            //Cv2.NamedWindow("效果图");
            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试YOLO目标检测 —— void TestYoloDetector()
        /// <summary>
        /// 测试YOLO目标检测
        /// </summary>
        [TestMethod]
        public void TestYoloDetector()
        {
            const string modelPath = @"F:\Files\Models\YOLOv11-ONNX\yolo11n.onnx";
            const string imagePath = "Content/Images/scene1.jpg";

            using Mat image = Cv2.ImRead(imagePath);
            using YoloDetector yoloDetector = new YoloDetector(modelPath);
            yoloDetector.StartSession();
            Detection[] detections = yoloDetector.Infer(image);

            using Mat targetImage = image.Clone();
            foreach (Detection detection in detections)
            {
                //打印结果
                string box = $"({detection.Box.Location.X},{detection.Box.Location.Y})|{detection.Box.Width}*{detection.Box.Height}";
                Trace.WriteLine($"{detection.Label}: {detection.Confidence:F2}, Box: {box}");

                //绘制矩形框
                targetImage.Rectangle(detection.Box, Scalar.Red);

                //绘制文本
                string labelConfidence = $"{detection.Label}: {detection.Confidence:F2}";
                targetImage.PutText(labelConfidence, detection.Box.Location, HersheyFonts.HersheyPlain, 1, Scalar.Yellow);
            }

            //Cv2.NamedWindow("原图");
            //Cv2.NamedWindow("效果图");
            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试YOLO图像分割 —— void TestYoloSegmenter()
        /// <summary>
        /// 测试YOLO图像分割
        /// </summary>
        [TestMethod]
        public void TestYoloSegmenter()
        {
            const string modelPath = @"F:\Files\Models\YOLOv11-ONNX\yolo11n-seg.onnx";
            const string imagePath = "Content/Images/children.jpg";

            using Mat image = Cv2.ImRead(imagePath);
            using YoloSegmenter yoloSegmenter = new YoloSegmenter(modelPath);
            yoloSegmenter.StartSession();
            Segmentation[] segmentations = yoloSegmenter.Infer(image);

            using Mat targetImage = image.Clone();
            foreach (Segmentation segmentation in segmentations)
            {
                //打印结果
                string box = $"({segmentation.Box.Location.X},{segmentation.Box.Location.Y})|{segmentation.Box.Width}*{segmentation.Box.Height}";
                Trace.WriteLine($"{segmentation.Label}: {segmentation.Confidence:F2}, Box: {box}");

                //绘制轮廓
                targetImage.DrawContours([segmentation.Contour], -1, Scalar.Red);

                //绘制文本
                string labelConfidence = $"{segmentation.Label}: {segmentation.Confidence:F2}";
                targetImage.PutText(labelConfidence, segmentation.Box.Location, HersheyFonts.HersheyPlain, 1, Scalar.Yellow);
            }

            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试YOLO定向目标检测 —— void TestYoloObbDetector()
        /// <summary>
        /// 测试YOLO定向目标检测
        /// </summary>
        [TestMethod]
        public void TestYoloObbDetector()
        {
            const string modelPath = @"F:\Files\Models\YOLOv11-ONNX\yolo11s-obb.onnx";
            const string imagePath = "Content/Images/P0009.jpg";

            using Mat image = Cv2.ImRead(imagePath);
            using YoloObbDetector yoloObbDetector = new YoloObbDetector(modelPath);
            yoloObbDetector.StartSession();
            ObbDetection[] detections = yoloObbDetector.Infer(image, 0);

            using Mat targetImage = image.Clone();
            foreach (ObbDetection detection in detections)
            {
                //打印结果
                string box = $"({detection.RotatedBox.Center.X},{detection.RotatedBox.Center.Y})|{detection.RotatedBox.Size.Width}*{detection.RotatedBox.Size.Height}|{detection.RotatedBox.Angle:F3}";
                Trace.WriteLine($"{detection.Label}: {detection.Confidence:F2}, Box: {box}");

                //绘制旋转矩形框
                targetImage.DrawContours([detection.RotatedBox.Points().Select(x => x.ToPoint())], -1, Scalar.Red, 2);

                //绘制文本
                string labelConfidence = $"{detection.Label}: {detection.Confidence:F2}";
                targetImage.PutText(labelConfidence, detection.RotatedBox.Center.ToPoint(), HersheyFonts.HersheyPlain, 1, Scalar.Yellow);
            }

            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试SuperPoint —— void TestSuperPoint()
        /// <summary>
        /// 测试SuperPoint
        /// </summary>
        [TestMethod]
        public void TestSuperPoint()
        {
            const string modelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\superpoint.onnx";
            const string imagePath = "Content/Images/board1.bmp";

            using Mat image = Cv2.ImRead(imagePath, ImreadModes.Grayscale);
            using SuperPoint superPoint = new SuperPoint(modelPath);
            superPoint.StartSession();
            Feature[] features = superPoint.Infer(image, 0);

            Trace.WriteLine($"关键点数量: {features.Length}");
            Trace.WriteLine($"------------------------------------");

            using Mat targetImage = Cv2.ImRead(imagePath);
            foreach (Feature feature in features)
            {
                //打印结果
                string point = $"({feature.ScaledKeyPoint.X},{feature.ScaledKeyPoint.Y})";
                Trace.WriteLine($"{feature.Confidence:F2}: {point}");

                //绘制关键点
                targetImage.Circle(feature.ScaledKeyPoint, 2, Scalar.Red);
            }

            Cv2.NamedWindow("原图");
            Cv2.NamedWindow("效果图");
            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试DiskPoint —— void TestDiskPoint()
        /// <summary>
        /// 测试DiskPoint
        /// </summary>
        [TestMethod]
        public void TestDiskPoint()
        {
            const string modelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\disk.onnx";
            const string imagePath = "Content/Images/board1.bmp";

            using Mat image = Cv2.ImRead(imagePath);
            using DiskPoint diskPoint = new DiskPoint(modelPath);
            diskPoint.StartSession();
            Feature[] features = diskPoint.Infer(image, 0);

            Trace.WriteLine($"关键点数量: {features.Length}");
            Trace.WriteLine($"------------------------------------");

            using Mat targetImage = image.Clone();
            foreach (Feature feature in features.OrderByDescending(x => x.Confidence))
            {
                //打印结果
                string point = $"({feature.ScaledKeyPoint.X},{feature.ScaledKeyPoint.Y})";
                Trace.WriteLine($"{feature.Confidence:F2}: {point}");

                //绘制矩形框
                targetImage.Circle(feature.ScaledKeyPoint, 2, Scalar.Red);
            }

            Cv2.NamedWindow("原图");
            Cv2.NamedWindow("效果图");
            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试XFeatPoint —— void TestXFeatPoint()
        /// <summary>
        /// 测试XFeatPoint
        /// </summary>
        [TestMethod]
        public void TestXFeatPoint()
        {
            const string modelPath = @"F:\Files\Models\XFeat-ONNX_v1.0.0\xfeat.onnx";
            const string imagePath = "Content/Images/board1.bmp";

            using Mat image = Cv2.ImRead(imagePath);
            using XFeatPoint xFeatPoint = new XFeatPoint(modelPath);
            xFeatPoint.StartSession();
            IList<Feature> features = xFeatPoint.Infer(image, 0);

            Trace.WriteLine($"关键点数量: {features.Count}");
            Trace.WriteLine($"------------------------------------");

            using Mat targetImage = image.Clone();
            foreach (Feature feature in features.OrderByDescending(x => x.Confidence))
            {
                //打印结果
                string point = $"({feature.ScaledKeyPoint.X},{feature.ScaledKeyPoint.Y})";
                Trace.WriteLine($"{feature.Confidence:F2}: {point}");

                //绘制矩形框
                targetImage.Circle(feature.ScaledKeyPoint, 2, Scalar.Red);
            }

            Cv2.NamedWindow("原图");
            Cv2.NamedWindow("效果图");
            Cv2.ImShow("原图", image);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试SuperPoint-LightGlueV0 —— void TestSuperLightGlueV0()
        /// <summary>
        /// 测试SuperPoint-LightGlueV0
        /// </summary>
        [TestMethod]
        public void TestSuperLightGlueV0()
        {
            const string fModelPath = @"F:\Files\Models\LightGlue-ONNX_v0.1.3_TensorRT\superpoint.onnx";
            const string mModelPath = @"F:\Files\Models\LightGlue-ONNX_v0.1.3_TensorRT\superpoint_lightglue.onnx";
            const string image1Path = "Content/Images/board1.bmp";
            const string image2Path = "Content/Images/board2.bmp";
            using Mat sourceImage = Cv2.ImRead(image1Path, ImreadModes.Grayscale);
            using Mat targetImage = Cv2.ImRead(image2Path, ImreadModes.Grayscale);

            //特征部分
            using SuperPoint superPoint = new SuperPoint(fModelPath);
            superPoint.StartSession();
            Feature[] features1 = superPoint.Infer(sourceImage, 0);
            Feature[] features2 = superPoint.Infer(targetImage, 0);

            //匹配部分
            using SuperLightGlue lightGlue = new SuperLightGlue(mModelPath);
            lightGlue.StartSession();
            DMatch[] matches = lightGlue.Infer((features1, features2), 0.9f);

            //绘制匹配结果
            IEnumerable<KeyPoint> sourceKeyPoints = features1.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));
            IEnumerable<KeyPoint> targetKeyPoints = features2.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));

            using Mat resultImage = new Mat();
            Cv2.DrawMatches(sourceImage, sourceKeyPoints, targetImage, targetKeyPoints, matches, resultImage);

            Cv2.NamedWindow("效果图");
            Cv2.ImShow("效果图", resultImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试SuperPoint-LightGlueV1 —— void TestSuperLightGlueV1()
        /// <summary>
        /// 测试SuperPoint-LightGlueV1
        /// </summary>
        [TestMethod]
        public void TestSuperLightGlueV1()
        {
            const string fModelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\superpoint.onnx";
            const string mModelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\superpoint_lightglue_fused_cpu.onnx";
            const string image1Path = "Content/Images/board1.bmp";
            const string image2Path = "Content/Images/board2.bmp";
            using Mat sourceImage = Cv2.ImRead(image1Path, ImreadModes.Grayscale);
            using Mat targetImage = Cv2.ImRead(image2Path, ImreadModes.Grayscale);

            //特征部分
            using SuperPoint superPoint = new SuperPoint(fModelPath);
            superPoint.StartSession();
            Feature[] features1 = superPoint.Infer(sourceImage, 0);
            Feature[] features2 = superPoint.Infer(targetImage, 0);

            //匹配部分
            using SuperLightGlue lightGlue = new SuperLightGlue(mModelPath);
            lightGlue.StartSession();
            DMatch[] matches = lightGlue.Infer((features1, features2), 0.9f);

            //绘制匹配结果
            IEnumerable<KeyPoint> sourceKeyPoints = features1.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));
            IEnumerable<KeyPoint> targetKeyPoints = features2.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));

            using Mat resultImage = new Mat();
            Cv2.DrawMatches(sourceImage, sourceKeyPoints, targetImage, targetKeyPoints, matches, resultImage);

            Cv2.NamedWindow("效果图");
            Cv2.ImShow("效果图", resultImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Disk-LightGlue —— void TestDiskLightGlue()
        /// <summary>
        /// 测试Disk-LightGlue
        /// </summary>
        [TestMethod]
        public void TestDiskLightGlue()
        {
            const string fModelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\disk.onnx";
            const string mModelPath = @"F:\Files\Models\LightGlue-ONNX_v1.0.0_Fused\disk_lightglue_fused_cpu.onnx";
            const string image1Path = "Content/Images/board1.bmp";
            const string image2Path = "Content/Images/board2.bmp";
            using Mat sourceImage = Cv2.ImRead(image1Path);
            using Mat targetImage = Cv2.ImRead(image2Path);

            //特征部分
            using DiskPoint diskPoint = new DiskPoint(fModelPath);
            diskPoint.StartSession();
            Feature[] features1 = diskPoint.Infer(sourceImage, 0);
            Feature[] features2 = diskPoint.Infer(targetImage, 0);

            //匹配部分
            using DiskLightGlue lightGlue = new DiskLightGlue(mModelPath);
            lightGlue.StartSession();
            DMatch[] matches = lightGlue.Infer((features1, features2), 0.9f);

            //绘制匹配结果
            IEnumerable<KeyPoint> sourceKeyPoints = features1.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));
            IEnumerable<KeyPoint> targetKeyPoints = features2.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));

            using Mat resultImage = new Mat();
            Cv2.DrawMatches(sourceImage, sourceKeyPoints, targetImage, targetKeyPoints, matches, resultImage);

            Cv2.NamedWindow("效果图");
            Cv2.ImShow("效果图", resultImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试XFeatMatcher —— void TestXFeatMatcher()
        /// <summary>
        /// 测试XFeatMatcher
        /// </summary>
        [TestMethod]
        public void TestXFeatMatcher()
        {
            const string fModelPath = @"F:\Files\Models\XFeat-ONNX_v1.0.0\xfeat.onnx";
            const string mModelPath = @"F:\Files\Models\XFeat-ONNX_v1.0.0\matching.onnx";
            const string image1Path = "Content/Images/board1.bmp";
            const string image2Path = "Content/Images/board2.bmp";
            using Mat sourceImage = Cv2.ImRead(image1Path);
            using Mat targetImage = Cv2.ImRead(image2Path);

            //特征部分
            using XFeatPoint xFeatPoint = new XFeatPoint(fModelPath);
            xFeatPoint.StartSession();
            Feature[] features1 = xFeatPoint.Infer(sourceImage, 0);
            Feature[] features2 = xFeatPoint.Infer(targetImage, 0);

            //匹配部分
            using XFeatMatcher xFeatMatcher = new XFeatMatcher(mModelPath);
            xFeatMatcher.StartSession();
            DMatch[] matches = xFeatMatcher.Infer((features1, features2), 0);

            //绘制匹配结果
            IEnumerable<KeyPoint> sourceKeyPoints = features1.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));
            IEnumerable<KeyPoint> targetKeyPoints = features2.Select(x => new KeyPoint(x.ScaledKeyPoint, 2));

            using Mat resultImage = new Mat();
            Cv2.DrawMatches(sourceImage, sourceKeyPoints, targetImage, targetKeyPoints, matches, resultImage);

            Cv2.NamedWindow("效果图");
            Cv2.ImShow("效果图", resultImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Paddle文本检测 —— void TestPaddleDetector()
        /// <summary>
        /// 测试Paddle文本检测
        /// </summary>
        [TestMethod]
        public void TestPaddleDetector()
        {
            const string modelPath = @"F:\Files\Models\PaddleOCR-ONNX\ch_PP-OCRv4_det_infer.onnx";
            const string imagePath = "Content/Images/brand.png";

            using Mat sourceImage = Cv2.ImRead(imagePath);
            using PaddleDetector detector = new PaddleDetector(modelPath);
            detector.StartSession();
            IList<PaddleContour> contours = detector.Infer(sourceImage);

            using Mat targetImage = sourceImage.Clone();
            foreach (PaddleContour contour in contours)
            {
                IEnumerable<Point> points = contour.Points.Select(point2F => point2F.ToPoint());
                targetImage.DrawContours([points], -1, Scalar.Red);
            }

            Cv2.ImShow("原图", sourceImage);
            Cv2.ImShow("效果图", targetImage);
            Cv2.WaitKey();
        }
        #endregion

        #region # 测试Paddle文本识别 —— void TestPaddleRecognizer()
        /// <summary>
        /// 测试Paddle文本识别
        /// </summary>
        [TestMethod]
        public void TestPaddleRecognizer()
        {
            const string modelPath = @"F:\Files\Models\PaddleOCR-ONNX\ch_PP-OCRv4_rec_infer.onnx";
            const string imagePath = "Content/Images/text.jpg";

            using Mat image = Cv2.ImRead(imagePath);
            using PaddleRecognizer recognizer = new PaddleRecognizer(modelPath);
            recognizer.StartSession();
            string text = recognizer.Infer(image);
            Trace.WriteLine(text);
        }
        #endregion

        #region # 测试Paddle文本分类 —— void TestPaddleClassifier()
        /// <summary>
        /// 测试Paddle文本分类
        /// </summary>
        [TestMethod]
        public void TestPaddleClassifier()
        {
            const string modelPath = @"F:\Files\Models\PaddleOCR-ONNX\ch_ppocr_mobile_v2.0_cls_infer.onnx";
            const string imagePath = "Content/Images/text_180.jpg";
            using Mat image = Cv2.ImRead(imagePath);

            using PaddleClassifier classifier = new PaddleClassifier(modelPath);
            classifier.StartSession();
            PaddleTextDirection textDirection = classifier.Infer(image);
            Trace.WriteLine(textDirection);
        }
        #endregion
    }
}
