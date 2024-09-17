using Microsoft.EntityFrameworkCore;

namespace MyStore.Domain.Entities
{
    [PrimaryKey(nameof(UserId), nameof(VoucherCode))]
    public class UserVoucher
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public string VoucherCode { get; set; }
        public Voucher Voucher { get; set; }

        public bool Used { get; set; }

    }
}
