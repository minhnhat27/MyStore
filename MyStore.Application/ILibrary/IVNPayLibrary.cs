using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;

namespace MyStore.Application.ILibrary
{
    public interface IVNPayLibrary
    {
        string VERSION { get; }
        string CreateRequestUrl(VNPay vnPAY, string baseUrl, string vnp_HashSecret);
        bool ValidateSignature(VNPayRequest request, string vnp_SecureHash, string vnp_HashSecret);
        bool ValidateQueryDrSignature(VNPayQueryDrResponse response, string vnp_SecureHash, string vnp_HashSecret);
        string CreateSecureHashQueryDr(VNPayQueryDr queryDr, string vnp_HashSecret);
    }
}
