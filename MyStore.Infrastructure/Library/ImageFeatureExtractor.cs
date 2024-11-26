using Microsoft.ML;
using Microsoft.ML.Data;
using MyStore.Application.ILibrary;
using Microsoft.ML.Transforms;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Drawing;
using Emgu.CV.Util;
using System.Runtime.InteropServices;
using NumSharp;
using Microsoft.ML.OnnxRuntime;

namespace MyStore.Infrastructure.Library
{
    public class ImageData
    {
        [LoadColumn(0)]
        public string? ImagePath;

        [LoadColumn(1)]
        public string? Label;
    }

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

    public class ImagePrediction : ImageData
    {
        public float[]? Score;

        public string? PredictedLabelValue;
    }
    public class Prediction
    {
        [VectorType(2048)]
        public float[] Features { get; set; }
    }
    public class ImageFeatureExtractor : IImageFeatureExtractor
    {
        private readonly MLContext mlContext;
        private ITransformer _model;

        private string _assetsPath => Path.Combine(Environment.CurrentDirectory, "wwwroot", "assets");
        private string _modelPath => Path.Combine(_assetsPath, "fashionmnist_cnn_model.onnx");
        private string[] _outputColumnNames => ["dense_1"];
        private string[] _inputColumnNames => ["conv2d_input"];
        private string _testFile => Path.Combine(_assetsPath, "test_image.npz");

        private string _imagesFolder => Path.Combine(_assetsPath, "images");
        private string _trainTagsTsv => Path.Combine(_imagesFolder, "tags.tsv");
        private string _testTagsTsv => Path.Combine(_imagesFolder, "test-tags.tsv");
        private string _fashionTagsTsv => Path.Combine(_imagesFolder, "fashion-tags.tsv");
        private string _predictSingleImage => Path.Combine(_imagesFolder, "cd409f97-6e12-4446-a23d-8f0a706c9de9.jpg");
        private string _inceptionTensorFlowModel => Path.Combine(_assetsPath, "inception", "tensorflow_inception_graph.pb");


        public ImageFeatureExtractor()
        {
            mlContext = new MLContext();
            _model = GenerateFashionModel();
            //ClassifySingleImage(_model);
            //_model = Keras.Models.Model.LoadModel(_modelPath);
            //var croppedClothes = ExtractClothesFromImage(_predictSingleImage);

            //if (croppedClothes != null)
            //{
            //    CvInvoke.Imshow("Cropped Clothes", croppedClothes);
            //    CvInvoke.WaitKey(0);
            //}
        }

        public Task Initialize() => Task.CompletedTask;

        struct InceptionSettings
        {
            public const int ImageHeight = 224;
            public const int ImageWidth = 224;
            public const float Mean = 117;
            public const float Scale = 1;
            public const bool ChannelsLast = true;
        }

        private void ClassifySingleImage(ITransformer model)
        {
            var imageData = new ImageData()
            {
                ImagePath = _predictSingleImage
            };

            //// Make prediction function (input = ImageData, output = ImagePrediction)
            var predictor = mlContext.Model.CreatePredictionEngine<ImageData, ImagePrediction>(model);
            var predicted = predictor.Predict(imageData);

            Console.WriteLine("-------------------------------------------------------------------");
            DisplayResults([predicted]);
            Console.WriteLine("-------------------------------------------------------------------");
        }

        private TransformerChain<KeyToValueMappingTransformer> GenerateModel()
        {
            var pipeline = mlContext.Transforms.LoadImages(outputColumnName: "input",
                            imageFolder: _imagesFolder,
                            inputColumnName: nameof(ImageData.ImagePath))
                .Append(mlContext.Transforms.ResizeImages(outputColumnName: "input",
                            imageWidth: InceptionSettings.ImageWidth,
                            imageHeight: InceptionSettings.ImageHeight,
                            inputColumnName: "input"))
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input",
                            interleavePixelColors: InceptionSettings.ChannelsLast,
                            offsetImage: InceptionSettings.Mean))
                .Append(mlContext.Model.LoadTensorFlowModel(_inceptionTensorFlowModel)
                        .ScoreTensorFlowModel(outputColumnNames: ["softmax2_pre_activation"],
                            inputColumnNames: ["input"], addBatchDimensionInput: true))
                .Append(mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: "LabelKey",
                            inputColumnName: "Label"))
                .Append(mlContext.MulticlassClassification.Trainers.LbfgsMaximumEntropy(labelColumnName: "LabelKey",
                            featureColumnName: "softmax2_pre_activation"))
                .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabelValue", "PredictedLabel"))
                .AppendCacheCheckpoint(mlContext);

            IDataView trainingData = mlContext.Data.LoadFromTextFile<ImageData>(path: _trainTagsTsv, hasHeader: false);
            var model = pipeline.Fit(trainingData);

            IDataView testData = mlContext.Data.LoadFromTextFile<ImageData>(path: _testTagsTsv, hasHeader: false);
            IDataView predictions = model.Transform(testData);

            // Create an IEnumerable for the predictions for displaying results
            IEnumerable<ImagePrediction> imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, true);
            DisplayResults(imagePredictionData);

            MulticlassClassificationMetrics metrics =
                mlContext.MulticlassClassification.Evaluate(predictions,
                    labelColumnName: "LabelKey",
                    predictedLabelColumnName: "PredictedLabel");

            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            return model;
        }
        
        private float[] PreprocessImage(string imagePath)
        {
            Image<Gray, byte> image = new(imagePath);
            image = image.Resize(28, 28, Inter.Linear);

            float[] imageArray = new float[28 * 28];
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    imageArray[i] = image.Data[i, j, 0];
                }
            }

            return imageArray;
        }

        public static float[] PreprocessImage(Image<Bgr, byte> img)
        {
            var image = img.Convert<Gray, byte>();
            image = image.Resize(28, 28, Inter.Linear);

            float[] imageArray = new float[28 * 28];
            for (int i = 0; i < image.Height; i++)
            {
                for (int j = 0; j < image.Width; j++)
                {
                    imageArray[i] = image.Data[i, j, 0];
                }
            }

            return imageArray;
        }

        private ITransformer GenerateFashionModel()
        {
            using var session = new InferenceSession(_modelPath);

            var pipeline = mlContext.Transforms
                .ApplyOnnxModel(_outputColumnNames, _inputColumnNames, _modelPath);

            var croppedClothes = ExtractClothesFromImage(_predictSingleImage);
            CvInvoke.Imshow("Cropped Clothes", croppedClothes.Resize(360, 360, Inter.Linear));
            CvInvoke.WaitKey(0);

            var input = new Input { Data = PreprocessImage(croppedClothes) };
            var dataView = mlContext.Data.LoadFromEnumerable([input]);

            var model = pipeline.Fit(dataView);

            var transformedData = model.Transform(dataView);
            var predictions = mlContext.Data.CreateEnumerable<Output>(transformedData, reuseRowObject: false);

            foreach (var d in predictions.Single().Data)
            {
                Console.WriteLine(d);
            }
            Console.WriteLine("Label: " + Array.IndexOf(predictions.Single().Data, predictions.Single().Data.Max()));



            //IDataView testData = mlContext.Data.LoadFromTextFile<ImageData>(path: _testTagsTsv, hasHeader: false);
            //IDataView predictions = model.Transform(testData);

            //// Create an IEnumerable for the predictions for displaying results
            //IEnumerable<ImagePrediction> imagePredictionData = mlContext.Data.CreateEnumerable<ImagePrediction>(predictions, true);
            //DisplayResults(imagePredictionData);

            //MulticlassClassificationMetrics metrics =
            //    mlContext.MulticlassClassification.Evaluate(predictions,
            //        labelColumnName: "LabelKey",
            //        predictedLabelColumnName: "PredictedLabel");

            //Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            //Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");

            return model;
        }

        public static Image<Bgr, byte>? ExtractClothesFromImage(string imagePath)
        {
            var img = CvInvoke.Imread(imagePath, ImreadModes.Color);
            if (img.IsEmpty)
            {
                return null;
            }

            // Chuyển đổi ảnh sang không gian màu HSV
            var hsvImage = new Mat();
            CvInvoke.CvtColor(img, hsvImage, ColorConversion.Bgr2Hsv);

            //// Tạo một mặt nạ (mask) để chỉ giữ những vùng có màu giống quần áo (ví dụ, màu sắc đặc trưng của quần áo)
            var lowerBound = new ScalarArray(new MCvScalar(0, 0, 30));  // Màu sắc thấp (hue, saturation, value)
            var upperBound = new ScalarArray(new MCvScalar(180, 255, 255));  // Màu sắc cao
            var clothesMask = new Mat();
            CvInvoke.InRange(hsvImage, lowerBound, upperBound, clothesMask);

            //// Sử dụng phép toán morpological (erosion và dilation) để làm sạch mask
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(-1, -1));
            CvInvoke.MorphologyEx(clothesMask, clothesMask, MorphOp.Open, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));
            CvInvoke.MorphologyEx(clothesMask, clothesMask, MorphOp.Close, kernel, new Point(-1, -1), 1, BorderType.Reflect, new MCvScalar(0));

            //// Sử dụng mask để tách quần áo ra khỏi nền
            //var clothesImage = new Image<Bgr, byte>(img.Width, img.Height);
            //img.CopyTo(clothesImage, clothesMask);

            //// Tìm các contours (đường viền) trong ảnh đã tách
            //var contours = new VectorOfVectorOfPoint();
            //var hierarchy = new Mat();
            //CvInvoke.FindContours(clothesMask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            var contours = new VectorOfVectorOfPoint();
            var hierarchy = new Mat();
            CvInvoke.FindContours(clothesMask, contours, hierarchy, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            // Tìm contour lớn nhất
            if (contours.Length > 0)
            {
                var largestContour = contours[0];
                for (int i = 1; i < contours.Size; i++)
                {
                    var contour = contours[i];
                    if (CvInvoke.ContourArea(contour) > CvInvoke.ContourArea(largestContour))
                    {
                        largestContour = contour;
                    }
                }

                // Tạo bounding box xung quanh contour lớn nhất
                var boundingBox = CvInvoke.BoundingRectangle(largestContour);

                // Cắt vật thể (quần áo) từ ảnh gốc dựa trên bounding box
                var croppedClothes = new Mat(img, boundingBox);

                // Trả về ảnh đã cắt quần áo
                return croppedClothes.ToImage<Bgr, byte>();
            }
            return null;
        }
         
        private static string ExtractPrimaryColor(string imagePath)
        {
            
            return "Unknown";
        }

        private static string AnalyzePatterns(string imagePath)
        {
            
            return "Solid";
        }
        private static string AnalyzeTexture(string imagePath)
        {
            
            return "Leather";
        }

        private static string ExtractMetadata(string imagePath)
        {
            // Logic để trích xuất metadata như kích thước, kiểu chụp, v.v.
            return "Additional Metadata";
        }

        public float[] ExtractFeatures(string imagePath)
        {
            
            //var imageData = new ImageData
            //{
            //    ImagePath = imagePath,
            //};
            //var dataView = _mlContext.Data.LoadFromEnumerable(new[] { imageData });

            //var pipeline = _mlContext.Transforms.Conversion.MapValueToKey("ImagePath")
            //    .Append(_mlContext.Transforms.LoadImages("input", "", nameof(ImageData.ImagePath))
            //    .Append(_mlContext.Transforms.ResizeImages("input", InceptionSettings.ImageWidth, InceptionSettings.ImageHeight))
            //    .Append(_mlContext.Transforms.ExtractPixels("input"))
            //    .Append(_mlContext.Transforms.ApplyOnnxModel("output", "input", modelFile: "mobilenetv2_050_Opset16.onnx"))
            //);

            //// Áp dụng pipeline và trích xuất đặc trưng
            //var transformedData = pipeline.Fit(dataView).Transform(dataView);

            //// Lấy kết quả dự đoán
            //var predictions = _mlContext.Data.CreateEnumerable<Prediction>(transformedData, reuseRowObject: false);

            //return predictions.First().Features;
            return [];
        }

        private void DisplayResults(IEnumerable<ImagePrediction> imagePredictionData)
        {
            foreach (ImagePrediction prediction in imagePredictionData)
            {
                Console.WriteLine($"Image: {Path.GetFileName(prediction.ImagePath)} predicted as: {prediction.PredictedLabelValue} with score: {prediction.Score?.Max()} ");
            }
        }
    }
   
}
