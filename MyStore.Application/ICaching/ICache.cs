namespace MyStore.Application.ICaching
{
    public interface ICache
    {
        T? Get<T>(string cacheKey);
        void Set<T>(string cacheKey, T value);
        void Remove(string cacheKey);

    }
}
