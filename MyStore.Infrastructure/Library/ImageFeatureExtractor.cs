using Microsoft.ML;
using Microsoft.ML.Data;
using MyStore.Application.ILibrary;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Util;
using Microsoft.ML.Transforms.Onnx;
using System.Runtime.InteropServices;

namespace MyStore.Infrastructure.Library
{
    public class Input
    {
        [VectorType(28 * 28 * 1)]
        [ColumnName("conv2d_input")]
        public float[] Data { get; set; }
    }
    public class Output
    {
        [VectorType(10)]
        [ColumnName("dense_1")]
        public float[] Data { get; set; }
    }
    
    public class ImageFeatureExtractor : IImageFeatureExtractor
    {
        private readonly MLContext _mlContext;
        private OnnxTransformer _model;
        private PredictionEngine<Input, Output> _predictor;

        private string _assetsPath => Path.Combine(Environment.CurrentDirectory, "wwwroot", "assets");
        private string _imagesFolder => Path.Combine(_assetsPath, "images");
        private string _modelPath => Path.Combine(_assetsPath, "fashionmnist_cnn_model.onnx");
        private string[] _outputColumnNames => ["dense_1"];
        private string[] _inputColumnNames => ["conv2d_input"];
        private string _testFile => Path.Combine(_imagesFolder, "products", "16", "6cc529da-6108-4b15-a572-fb111f3b78dd.jpg");
        //private string _testFile => Path.Combine(_imagesFolder, "products", "3", "dd642b09-0b49-43d0-af4f-eca76dc7a0e5.webp");

        //private string _trainTagsTsv => Path.Combine(_imagesFolder, "tags.tsv");
        //private string _testTagsTsv => Path.Combine(_imagesFolder, "test-tags.tsv");
        //private string _fashionTagsTsv => Path.Combine(_imagesFolder, "fashion-tags.tsv");
        //private string _predictSingleImage => Path.Combine(_imagesFolder, "cd409f97-6e12-4446-a23d-8f0a706c9de9.jpg");
        //private string _inceptionTensorFlowModel => Path.Combine(_assetsPath, "inception", "tensorflow_inception_graph.pb");

        public ImageFeatureExtractor()
        {
            _mlContext = new MLContext();
            _model = GenerateFashionModel();
            _predictor = _mlContext.Model.CreatePredictionEngine<Input, Output>(_model);
            //var test = ImageClassificationPrediction(_testFile);
            //CvInvoke.Imshow("x", image.Resize(360, 360, Inter.Linear));
            //CvInvoke.WaitKey(0);
            //Console.WriteLine("Lable: " + test.PredictedLabelValue);
        }

        public Task Initialize() => Task.CompletedTask;
        private OnnxTransformer GenerateFashionModel()
        {
            var pipeline = _mlContext.Transforms
                .ApplyOnnxModel(_outputColumnNames, _inputColumnNames, _modelPath);
            var emptyInput = Array.Empty<Input>();
            var dataView = _mlContext.Data.LoadFromEnumerable(emptyInput);
            var model = pipeline.Fit(dataView);
            return model;
        }

        /// <inheritdoc />
        public ImagePrediction? ImageClassificationPrediction(string imagePath)
        {
            try
            {
                var image = ExtractClothesFromImage(imagePath);
                //var image = new Image<Bgr, byte>(imagePath);

                var imageData = new Input();
                MCvScalar color = new();
                if (image != null)
                {
                    imageData.Data = PreprocessImage(image);
                    color = ExtractMainColor(image);
                }
                else
                {
                    var d = PreprocessImage(imagePath);
                    var c = ExtractMainColor(imagePath);
                    if (d != null && c != null)
                    {
                        imageData.Data = d;
                        color = c.Value;
                    }
                    else return null;
                }
                var predicted = _predictor.Predict(imageData);
                var data = predicted.Data.ToList();
                var label = data.IndexOf(data.Max());
                return new ImagePrediction
                {
                    PredictedLabelValue = label,
                    Blue = color.V0,
                    Green = color.V1,
                    Red = color.V2,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        private MCvScalar? ExtractMainColor(string imagePath)
        {
            Image<Bgr, byte> image = new(imagePath);
            if(image == null)
            {
                return null;
            }
            Mat reshapedImg = image.Mat.Clone().Reshape(1, image.Height * image.Width);

            // Chuyển đổi ma trận sang float
            Mat floatImg = new Mat();
            reshapedImg.ConvertTo(floatImg, DepthType.Cv32F);

            // Áp dụng K-means clustering để tìm màu chủ đạo
            int clusterCount = 3;  // Số cụm màu
            var criteria = new MCvTermCriteria(10, 0.1);  // Điều kiện kết thúc
            var labels = new Mat();
            var centers = new Mat();

            // K-means clustering
            CvInvoke.Kmeans(floatImg, clusterCount, labels, criteria, 1, KMeansInitType.PPCenters, centers);

            // Tính toán số lượng điểm ảnh cho mỗi cụm màu
            var labelsArray = new int[labels.Rows];
            Marshal.Copy(labels.DataPointer, labelsArray, 0, labels.Rows);

            var counts = new int[clusterCount];
            for (int i = 0; i < labelsArray.Length; i++)
            {
                counts[labelsArray[i]]++;
            }

            // Tìm cụm màu có số lượng điểm ảnh nhiều nhất (màu chủ đạo)
            int dominantClusterIndex = Array.IndexOf(counts, counts.Max());

            // Lấy màu chủ đạo từ các trung tâm cụm màu
            float[] centerData = new float[centers.Rows * centers.Cols];
            Marshal.Copy(centers.DataPointer, centerData, 0, centerData.Length);

            double blue = centerData[dominantClusterIndex * 3];
            double green = centerData[dominantClusterIndex * 3 + 1];
            double red = centerData[dominantClusterIndex * 3 + 2];
            var dominantColor = new MCvScalar(blue, green, red);

            return dominantColor;
        }
        private MCvScalar ExtractMainColor(Image<Bgr, byte> image)
        {
            Mat reshapedImg = image.Mat.Clone().Reshape(1, image.Height * image.Width);

            // Chuyển đổi ma trận sang float
            Mat floatImg = new Mat();
            reshapedImg.ConvertTo(floatImg, DepthType.Cv32F);

            // Áp dụng K-means clustering để tìm màu chủ đạo
            int clusterCount = 3;  // Số cụm màu
            var criteria = new MCvTermCriteria(10, 0.1);  // Điều kiện kết thúc
            var labels = new Mat();
            var centers = new Mat();

            // K-means clustering
            CvInvoke.Kmeans(floatImg, clusterCount, labels, criteria, 1, KMeansInitType.PPCenters, centers);

            // Tính toán số lượng điểm ảnh cho mỗi cụm màu
            var labelsArray = new int[labels.Rows];
            Marshal.Copy(labels.DataPointer, labelsArray, 0, labels.Rows);

            var counts = new int[clusterCount];
            for (int i = 0; i < labelsArray.Length; i++)
            {
                counts[labelsArray[i]]++;
            }

            // Tìm cụm màu có số lượng điểm ảnh nhiều nhất (màu chủ đạo)
            int dominantClusterIndex = Array.IndexOf(counts, counts.Max());

            // Lấy màu chủ đạo từ các trung tâm cụm màu
            float[] centerData = new float[centers.Rows * centers.Cols];
            Marshal.Copy(centers.DataPointer, centerData, 0, centerData.Length);

            double blue = centerData[dominantClusterIndex * 3];
            double green = centerData[dominantClusterIndex * 3 + 1];
            double red = centerData[dominantClusterIndex * 3 + 2];
            var dominantColor = new MCvScalar(blue, green, red);

            return dominantColor;
        }
        
        //private TransformerChain<KeyToValueMappingTransformer> GenerateModel()
        //{
        //    var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input",
        //                    imageFolder: _imagesFolder,
        //                    inputColumnName: nameof(ImageData.ImagePath))
        //        .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input",
        //                    imageWidth: InceptionSettings.ImageWidth,
        //                    imageHeight: InceptionSettings.ImageHeight,
        //                    inputColumnName: "input"))
        //        .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input",
        //                    interleavePixelColors: InceptionSettings.ChannelsLast,
        //                    offsetImage: InceptionSettings.Mean))
        //        .Append(mlContext.Model.LoadTensorFlowModel(_inceptionTensorFlowModel)
        //                .ScoreTensorFlowModel(outputColumnNames: ["softmax2_pre_activation"],
        //                    inputColumnNames: ["input"], addBatchDimensionInput: true))
        //        .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey",
        //                    inputColumnName: "Label"))
        //        .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey",
        //                    featureColumnName: "softmax2_pre_activation"))
        //        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"))
        //        .AppendCacheCheckpoint(mlContext);

        //    IDataView trainingData = mlContext.Data.LoadFromTextFile<ImageData>(path: _trainTagsTsv, hasHeader: false);
        //    var model = pipeline.Fit(trainingData);

        //    IDataView testData = mlContext.Data.LoadFromTextFile<ImageData>(path: _testTagsTsv, hasHeader: false);
        //    IDataView predictions = model.Transform(testData);

        //    // Create an IEnumerable for the predictions for displaying results
        //    IEnumerable<ImagePrediction> imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, true);
        //    DisplayResults(imagePredictionData);

        //    MulticlassClassificationMetrics metrics =
        //        mlContext.MulticlassClassification.Evaluate(predictions,
        //            labelColumnName: "LabelKey",
        //            predictedLabelColumnName: "PredictedLabel");

        //    Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
        //    Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

        //    return model;
        //}

        private float[]? PreprocessImage(string imagePath)
        {
            Image<Gray, byte> image = new(imagePath);
            if(image == null)
            {
                return null;
            }
            image = image.Resize(28, 28, Inter.Linear);
            var imageArray = image.Data
                .Cast<byte>()
                .Select(x => (float)x)
                .ToArray();
            return imageArray;
        }
        private float[] PreprocessImage(Image<Bgr, byte> img)
        {
            var image = img.Convert<Gray, byte>()
                .Resize(28, 28, Inter.Linear);
            var imageArray = image.Data
                .Cast<byte>()
                .Select(x => (float)x)
                .ToArray();
            return imageArray;
        }

        public Image<Bgr, byte>? ExtractClothesFromImage(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                return null;
            }

            var img = CvInvoke.Imread(imagePath, ImreadModes.Color);
            if (img.IsEmpty)
            {
                return null;
            }

            // Chuyển đổi ảnh sang không gian màu HSV
            var hsvImage = new Mat();
            CvInvoke.CvtColor(img, hsvImage, ColorConversion.Bgr2Hsv);

            // Tạo một mặt nạ (mask) để chỉ giữ những vùng có màu giống quần áo (ví dụ, màu sắc đặc trưng của quần áo)
            var lowerBound = new ScalarArray(new MCvScalar(0, 50, 50));  // Màu sắc thấp (hue, saturation, value)
            var upperBound = new ScalarArray(new MCvScalar(180, 255, 255));  // Màu sắc cao
            var clothesMask = new Mat();
            CvInvoke.InRange(hsvImage, lowerBound, upperBound, clothesMask);

            // Sử dụng phép toán morpological (erosion và dilation) để làm sạch mask
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(5, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(clothesMask, clothesMask, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
            CvInvoke.MorphologyEx(clothesMask, clothesMask, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(clothesMask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            if (contours.Size == 0)
            {
                return null;
            }

            // Tìm contour lớn nhất dựa trên diện tích
            var largestContour = contours[0];
            double maxArea = CvInvoke.ContourArea(largestContour);
            List<Mat> validContours = new();
            for (int i = 1; i < contours.Size; i++)
            {
                var contour = contours[i];
                double area = CvInvoke.ContourArea(contour);
                if (area > maxArea)
                {
                    largestContour = contour;
                    maxArea = area;
                }
            }

            // Tạo bounding box xung quanh contour lớn nhất
            var boundingBox = CvInvoke.BoundingRectangle(largestContour);
            var croppedClothes = new Mat(img, boundingBox);

            //// Tạo ảnh nền đen với kích thước gốc
            //var blackImage = new Image<Bgr, byte>(img.Width, img.Height, new Bgr(0, 0, 0));

            //// Sao chép đối tượng đã cắt vào ảnh nền đen
            //var regionOfInterest = blackImage.GetSubRect(boundingBox);
            //croppedClothes.CopyTo(regionOfInterest);

            return croppedClothes.ToImage<Bgr, byte>();
        }
    }

}
