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
    }
}
