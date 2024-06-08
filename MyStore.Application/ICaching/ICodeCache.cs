namespace MyStore.Application.ICaching
{
    public interface ICodeCache
    {
        int SetCodeForEmail(string email);
        int GetCodeFromEmail(string email);
        void RemoveCode(string email);
    }
}
