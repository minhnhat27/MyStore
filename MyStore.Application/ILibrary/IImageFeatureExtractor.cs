namespace MyStore.Application.ILibrary
{
    public interface IImageFeatureExtractor
    {
        float[] ExtractFeatures(string imagePath);
        Task Initialize();
    }
}
