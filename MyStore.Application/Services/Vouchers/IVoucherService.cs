﻿using MyStore.Application.DTOs;

namespace MyStore.Application.Services.Vouchers
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDTO>> GetVoucherByUser(string userId);
        Task<VoucherDTO> GetCommonVoucher(string userId, string code);
    }
}
