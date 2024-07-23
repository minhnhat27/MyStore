using MyStore.Domain.Entities;

namespace MyStore.Domain.Constants
{
    public static class ErrorMessage
    {
        public const string USER_NOT_FOUND = "User not found";
        public const string NOT_FOUND = "Not found";
        public const string UNAUTHORIZED = "Unauthorized";
        public const string INCORRECT_PASSWORD = "User or password incorrect";
        public const string INVALID_TOKEN = "Invalid token";
        public const string EXISTED_USER = "User already exists";
        public const string CANNOT_CANCEL = "Cannot be cancelled";
    }
}
