using MyStore.Application.DTO;

namespace MyStore.Application.Services.Vouchers
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDTO>> GetVoucherByUser(string userId);

    }
}
