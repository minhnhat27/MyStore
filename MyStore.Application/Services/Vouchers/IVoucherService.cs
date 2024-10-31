using MyStore.Application.DTOs;

namespace MyStore.Application.Services.Vouchers
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDTO>> GetAllVoucher();
        Task<VoucherDTO> CreateVoucher(VoucherDTO request);
        Task DeleteVoucher(string code);
        Task<bool> UpdateIsGlobal(string code, bool value);
        Task<UserVoucherResponse> GetUserVoucher(string code);
        Task<IEnumerable<string>> UpdateUserHaveVoucher(string code, IEnumerable<string> userIds);

        Task<IEnumerable<VoucherDTO>> GetVoucherByUser(string userId);
        Task<VoucherDTO> GetCommonVoucher(string userId, string code);
    }
}
