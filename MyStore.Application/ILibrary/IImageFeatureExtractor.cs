namespace MyStore.Application.ILibrary
{
    public class ImagePrediction
    {
        public int PredictedLabelValue { get; set; }
        public double Red { get; set; }
        public double Green { get; set; }
        public double Blue { get; set; }
    }
    public interface IImageFeatureExtractor
    {
        /// <summary>
        /// Fashion Image Classification with 10 labels (0-9), Red, Green, Blue
        /// </summary>
        /// <param name="imagePath"></param>
        /// <returns></returns>
        ImagePrediction? ImageClassificationPrediction(string imagePath);
        Task Initialize();
    }
}
