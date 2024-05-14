using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyStore.Application.Response
{
    public class ApiResponse
    {
        private bool Success { get; set; }
        private string? Message { get; set; }
        public ApiResponse(bool success)
        {
            Success = success;
        }
        public ApiResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }
        public static ApiResponse SuccessResponse()
        {
            return new ApiResponse(true);
        }
        public static ApiResponse SuccessResponse(string message)
        {
            return new ApiResponse(true, message);
        }

        public static ApiResponse FaileResponse()
        {
            return new ApiResponse(false);
        }
        public static ApiResponse FaileResponse(string message)
        {
            return new ApiResponse(false, message);
        }
    }
}
