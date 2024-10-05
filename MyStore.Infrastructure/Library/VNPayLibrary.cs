using MyStore.Application.ILibrary;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using Newtonsoft.Json;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace MyStore.Infrastructure.Library
{
    public class VNPayLibrary : IVNPayLibrary
    {
        public string VERSION { get; } = "2.1.0";

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }


        public string CreateRequestUrl(VNPay vnPAY, string baseUrl, string vnp_HashSecret)
        {
            var json = JsonConvert.SerializeObject(vnPAY);
            var dictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            if(dictionary != null )
            {
                var data = dictionary.Where(e => !string.IsNullOrEmpty(e.Value))
                                     .OrderBy(e => e.Key)
                                     .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}");

                var dataString = string.Join("&", data);

                var vnp_SecureHash = HmacSHA512(vnp_HashSecret, dataString);
                var requestUrl = $"{baseUrl}?{dataString}&vnp_SecureHash={vnp_SecureHash}";
                return requestUrl;
            }
            throw new Exception(ErrorMessage.INVALID);
        }

        public bool ValidateSignature(VNPayRequest request, string vnp_SecureHash, string vnp_HashSecret)
        {
            var json = JsonConvert.SerializeObject(request);
            var dictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            if (dictionary != null)
            {
                var data = dictionary.Where(e => !string.IsNullOrEmpty(e.Value) && e.Key != "vnp_SecureHash")
                                     .OrderBy(e => e.Key)
                                     .Select(x => $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}");

                var requestString = string.Join("&", data);

                var hash = HmacSHA512(vnp_HashSecret, requestString);
                return hash.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
            }
            throw new Exception(ErrorMessage.INVALID);
        }

        public bool ValidateQueryDrSignature(VNPayQueryDrResponse response, string vnp_SecureHash, string vnp_HashSecret)
        {
            var json = JsonConvert.SerializeObject(response);
            var dictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            if (dictionary != null)
            {
                var data = dictionary.Where(e => e.Key != "vnp_SecureHash")
                                     .Select(x => x.Value);

                var requestString = string.Join("|", data);

                var hash = HmacSHA512(vnp_HashSecret, requestString);
                return hash.Equals(vnp_SecureHash, StringComparison.InvariantCultureIgnoreCase);
            }
            throw new Exception(ErrorMessage.INVALID);
        }

        public string CreateSecureHashQueryDr(VNPayQueryDr queryDr, string vnp_HashSecret)
        {
            var json = JsonConvert.SerializeObject(queryDr);
            var dictionary = JsonConvert.DeserializeObject<IDictionary<string, string>>(json);
            if (dictionary != null)
            {
                var data = dictionary.Where(e => !string.IsNullOrEmpty(e.Value))
                                     //.OrderBy(e => e.Key)
                                     .Select(x => x.Value);

                var dataString = string.Join("|", data);

                var vnp_SecureHash = HmacSHA512(vnp_HashSecret, dataString);
                return vnp_SecureHash;
            }
            throw new Exception(ErrorMessage.INVALID);
        }
    }
}
