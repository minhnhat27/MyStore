using AutoMapper;
using MyStore.Application.DTOs;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Users;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Vouchers
{
    public class VoucherService : IVoucherService
    {
        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IVoucherRepository _voucherRepository;

        private readonly IMapper _mapper;
        public VoucherService(IUserVoucherRepository userVoucherRepository,
            IVoucherRepository voucherRepository,
            IMapper mapper)
        {
            _userVoucherRepository = userVoucherRepository;
            _voucherRepository = voucherRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<VoucherDTO>> GetVoucherByUser(string userId)
        {
            var today = DateTime.Now;
            var vouchers = (await _userVoucherRepository
                .GetAsync(e => e.UserId == userId && !e.Used && e.Voucher.EndDate > today))
                .Select(e => e.Voucher);

            return _mapper.Map<IEnumerable<VoucherDTO>>(vouchers);
        }

        public async Task<VoucherDTO> GetCommonVoucher(string userId, string code)
        {
            var userVoucher = await _userVoucherRepository.FindAsync(userId, code);
            if (userVoucher != null)
            {
                if (userVoucher.Used)
                {
                    throw new InvalidDataException("Mã giảm giá đã được sử dụng.");
                }
                else throw new InvalidDataException("Mã giảm giá đã có sẳn.");
            }

            var today = DateTime.Now;
            var voucher = await _voucherRepository
                .SingleOrDefaultAsync(e => e.Code.ToUpper() == code.ToUpper() && e.IsGlobal)
                    ?? throw new InvalidOperationException(ErrorMessage.INVALID_VOUCHER);
            
            if(voucher.EndDate < today)
            {
                throw new InvalidDataException(ErrorMessage.VOUCHER_DUE);
            }
            await _userVoucherRepository.AddAsync(new UserVoucher
            {
                UserId = userId,
                VoucherCode = code
            });
            return _mapper.Map<VoucherDTO>(voucher);
        }
    }
}
