using MyStore.Application.ICaching;
using MyStore.Application.ILibrary;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.ModelView;
using MyStore.Domain.Constants;
using MyStore.Domain.Enumerations;
using Net.payOS.Types;
using Net.payOS;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using MyStore.Application.Services.Orders;
using MyStore.Domain.Entities;

namespace MyStore.Application.Services.Payments
{
    public class PaymentProcessor : IPaymentProcessor
    {
        private readonly IConfiguration _configuration;
        private readonly IVNPayLibrary _vnPayLibrary;
        private readonly PayOS _payOS;

        public PaymentProcessor(IConfiguration configuration, IVNPayLibrary vnPayLibrary, PayOS payOS)
        {
            _configuration = configuration;
            _vnPayLibrary = vnPayLibrary;
            _payOS = payOS;
        }
        public string GetVNPayURL(Order order, DateTime deadline, string? orderDesc, string? ipAddress, string? locale)
        {
            var orderInfo = new VNPayOrderInfo
            {
                OrderId = order.Id,
                Amount = order.Total - order.AmountPaid,
                CreatedDate = order.OrderDate,
                Status = order.OrderStatus?.ToString() ?? "0",
                OrderDesc = orderDesc ?? "Thanh toan don hang: " + order.Id,
            };

            string? vnp_ReturnUrl = _configuration["VNPay:vnp_ReturnUrl"];
            string? vnp_Url = _configuration["VNPay:vnp_Url"];
            string? vnp_TmnCode = _configuration["VNPay:vnp_TmnCode"];
            string? vnp_HashSecret = _configuration["VNPay:vnp_HashSecret"];

            if (string.IsNullOrEmpty(vnp_ReturnUrl) || string.IsNullOrEmpty(vnp_Url)
                || string.IsNullOrEmpty(vnp_HashSecret) || string.IsNullOrEmpty(vnp_TmnCode))
            {
                throw new ArgumentException("Thiếu tham số");
            }

            var vnpay = new VNPay()
            {
                vnp_TmnCode = vnp_TmnCode,
                vnp_Version = _vnPayLibrary.VERSION,
                vnp_Locale = locale ?? "vn",
                vnp_ReturnUrl = vnp_ReturnUrl,
                vnp_Command = "pay",
                vnp_Amount = (orderInfo.Amount * 100).ToString(),
                vnp_CreateDate = orderInfo.CreatedDate.ToString("yyyyMMddHHmmss"),
                vnp_CurrCode = "VND",
                vnp_IpAddr = ipAddress ?? "127.0.0.1",
                vnp_OrderInfo = orderInfo.OrderDesc,
                vnp_OrderType = "200000",
                vnp_TxnRef = orderInfo.OrderId.ToString(),
                vnp_ExpireDate = deadline.ToString("yyyyMMddHHmmss"),
            };

            return _vnPayLibrary.CreateRequestUrl(vnpay, vnp_Url, vnp_HashSecret);
        }
        public async Task<string> GetPayOSURL(Order order, IEnumerable<OrderDetail> orderDetails)
        {
            var orderInfo = new PayOSOrderInfo
            {
                OrderId = order.Id,
                Amount = order.Total - order.AmountPaid,
                Products = orderDetails.Select(e => new ProductInfo
                {
                    Name = e.ProductName,
                    Price = e.Price,
                    Quantity = e.Quantity
                })
            };
            var cancelUrl = _configuration["PayOS:cancelUrl"];
            var returnUrl = _configuration["PayOS:returnUrl"];

            if (cancelUrl == null || returnUrl == null)
            {
                throw new ArgumentNullException(ErrorMessage.ARGUMENT_NULL);
            }

            List<ItemData> items = orderInfo.Products
                .Select(e => new ItemData(e.Name, e.Quantity, (int)Math.Floor(e.Price))).ToList();

            PaymentData paymentData = new(orderInfo.OrderId, (int)Math.Floor(orderInfo.Amount),
                "Thanh toan don hang: " + orderInfo.OrderId, items, cancelUrl, returnUrl);

            CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentData);
            return createPayment.checkoutUrl;
        }

    }
}
