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
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;

        private readonly IMapper _mapper;
        public VoucherService(IUserVoucherRepository userVoucherRepository, ITransactionRepository transactionRepository,
            IVoucherRepository voucherRepository, IUserRepository userRepository,
            IMapper mapper)
        {
            _userVoucherRepository = userVoucherRepository;
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _transactionRepository = transactionRepository;
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<VoucherDTO>> GetAllVoucher()
        {
            var vouchers = await _voucherRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<VoucherDTO>>(vouchers);
        }

        public async Task<VoucherDTO> CreateVoucher(VoucherDTO request)
        {
            var exist = await _voucherRepository.FindAsync(request.Code);
            if (exist != null)
            {
                throw new InvalidDataException($"Mã {request.Code} " + ErrorMessage.EXISTED);
            }
            if(request.DiscountAmount == null && request.DiscountPercent == null)
            {
                throw new InvalidOperationException(ErrorMessage.BAD_REQUEST);
            }

            var voucher = _mapper.Map<Voucher>(request);
            await _voucherRepository.AddAsync(voucher);
            return request;
        }

        public async Task DeleteVoucher(string code)
        {
            await _voucherRepository.DeleteAsync(code);
        }
        
        public async Task<bool> UpdateIsGlobal(string code, bool value)
        {
            var voucher = await _voucherRepository.FindAsync(code)
                ?? throw new InvalidOperationException(ErrorMessage.VOUCHER_NOT_FOUND);
            voucher.IsGlobal = value;
            await _voucherRepository.UpdateAsync(voucher);
            return value;
        }

        public async Task<UserVoucherResponse> GetUserVoucher(string code)
        {
            var userIds = (await _userVoucherRepository.GetAsync(e => e.VoucherCode == code)).Select(e => e.UserId);
            var users = await _userRepository.GetAllAsync();

            return new UserVoucherResponse
            {
                HaveVoucher = userIds,
                UserVoucher = _mapper.Map<IEnumerable<UserResponse>>(users)
            };
        }

        public async Task<IEnumerable<string>> UpdateUserHaveVoucher(string code, IEnumerable<string> userIds)
        {
            using var transaction = await _transactionRepository.BeginTransactionAsync();
            try
            {
                var uVouchers = await _userVoucherRepository
                    .GetAsync(x => x.VoucherCode == code && userIds.Contains(x.UserId));

                var listDelete = uVouchers
                    .Where(x => x.VoucherCode == code && !userIds.Contains(x.UserId));
                var listAdd = userIds.Where(id => !uVouchers.Any(x => x.UserId == id && x.VoucherCode == code))
                             .Select(id => new UserVoucher
                             {
                                 UserId = id,
                                 VoucherCode = code,
                             });
                if (listDelete.Any())
                {
                    await _userVoucherRepository.DeleteRangeAsync(listDelete);
                }
                if (listAdd.Any())
                {
                    await _userVoucherRepository.AddAsync(listAdd);
                }
                await transaction.CommitAsync();
                return userIds;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
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
