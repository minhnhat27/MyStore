namespace MyStore.Domain.Constants
{
    public static class ErrorMessage
    {
        public const string USER_NOT_FOUND = "Không tìm thấy người dùng.";
        public const string NOT_FOUND = "Không tìm thấy";
        public const string UNAUTHORIZED = "Chưa xác thực";
        public const string INCORRECT_PASSWORD = "Tên tài khoản hoặc mật khẩu không chính xác.";
        public const string INVALID_TOKEN = "Token không hợp lệ";
        public const string INVALID_OTP = "Mã xác nhận không chính xác";
        public const string EXISTED_USER = "Tài khoản đã tồn tại.";
        public const string CANNOT_CANCEL = "Không thể hủy";
        public const string INVALID = "Giá trị không hợp lệ";
        public const string SOLDOUT = "Sản phẩm đã hết hàng";
        public const string CART_MAXIMUM = "Đã đạt số lượng tối đa trong giỏ hàng";
        public const string BAD_REQUEST = "Yêu cầu không hợp lệ";
        public const string ERROR = "Có lỗi xảy ra, vui lòng thử lại sau";
        public const string INVALID_VOUCHER = "Mã giảm giá không hợp lệ";
        public const string PAYMENT_DUE = "Đã hết hạn thanh toán";
        public const string PAYMENT_FAILED = "Thanh toán không thành công";
        public const string EXISTED = "Đã tồn tại";
    }
}
