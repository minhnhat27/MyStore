using MyStore.Application.ModelView;
using MyStore.Application.Request;

namespace MyStore.Application.ILibrary
{
    public interface IVNPayLibrary
    {
        string VERSION { get; }
        string CreateRequestUrl(VNPay vnPAY, string baseUrl, string vnp_HashSecret);
        bool ValidateSignature(VNPayRequest request, string vnp_SecureHash, string vnp_HashSecret);
    }
}
