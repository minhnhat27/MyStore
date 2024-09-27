using AutoMapper;
using MyStore.Application.DTO;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Users;

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
            try
            {
                var vouchers = (await _userVoucherRepository.GetAsync(e => e.UserId == userId && !e.Used))
                        .Select(e => e.Voucher);
                
                return _mapper.Map<IEnumerable<VoucherDTO>>(vouchers);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
