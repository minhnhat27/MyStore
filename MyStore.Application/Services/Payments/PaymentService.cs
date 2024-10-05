using AutoMapper;
using Microsoft.Extensions.Configuration;
using MyStore.Application.DTOs;
using MyStore.Application.ICaching;
using MyStore.Application.ILibrary;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.ModelView;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using Net.payOS;
using Net.payOS.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyStore.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IVNPayLibrary _vnPayLibrary;
        private readonly IOrderRepository _orderRepository;
        private readonly ICache _cache;
        private readonly PayOS _payOS;

        public PaymentService(IConfiguration configuration, IVNPayLibrary vnPayLibrary,
                              IOrderRepository orderRepository, ICache cache,
                              IPaymentMethodRepository paymentMethodRepository,
                              PayOS payOS,
                              IMapper mapper)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _configuration = configuration;
            _vnPayLibrary = vnPayLibrary;
            _orderRepository = orderRepository;
            _cache = cache;
            _payOS = payOS;
        }

        public async Task<IEnumerable<PaymentMethodDTO>> GetPaymentMethods()
        {
            var payment = await _paymentMethodRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentMethodDTO>>(payment);
        }


        public async Task<PaymentMethodDTO> CreatePaymentMethod(CreatePaymentMethodRequest request)
        {
            try
            {
                var pMethod = await _paymentMethodRepository.SingleOrDefaultAsync(e => e.Name == request.Name);
                if (pMethod == null)
                {
                    PaymentMethod paymentMethod = new()
                    {
                        Name = request.Name,
                        IsActive = request.IsActive,
                    };

                    await _paymentMethodRepository.AddAsync(paymentMethod);
                    return _mapper.Map<PaymentMethodDTO>(paymentMethod);
                }
                else throw new InvalidDataException(ErrorMessage.EXISTED);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


        public async Task<PaymentMethodDTO> UpdatePaymentMethod(int id, UpdatePaymentMethodRequest request)
        {
            try
            {
                var pMethod = await _paymentMethodRepository.FindAsync(id);
                if (pMethod != null)
                {
                    if (request.IsActive.HasValue)
                    {
                        pMethod.IsActive = request.IsActive.Value;
                    }
                    if (request.Name != null)
                    {
                        pMethod.Name = request.Name;
                    }
                    await _paymentMethodRepository.UpdateAsync(pMethod);
                    return _mapper.Map<PaymentMethodDTO>(pMethod);
                }
                throw new ArgumentException(ErrorMessage.NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task DeletePaymentMethod(int id) => await _paymentMethodRepository.DeleteAsync(id);

        public string GetVNPayURL(VNPayOrderInfo order, string ipAddress, string? locale)
        {
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
                vnp_Amount = (order.Amount * 100).ToString(),
                vnp_CreateDate = order.CreatedDate.ToString("yyyyMMddHHmmss"),
                vnp_CurrCode = "VND",
                vnp_IpAddr = ipAddress,
                vnp_OrderInfo = order.OrderDesc,
                vnp_OrderType = "200000",
                vnp_TxnRef = order.OrderId.ToString(),
                vnp_ExpireDate = order.CreatedDate.AddMinutes(15).ToString("yyyyMMddHHmmss"),
            };

            return _vnPayLibrary.CreateRequestUrl(vnpay, vnp_Url, vnp_HashSecret);
        }

        public async Task<string?> IsActivePaymentMethod(int id)
        {
            var result = await _paymentMethodRepository
                    .SingleOrDefaultAsync(e => e.Id == id && e.IsActive);
            return result?.Name;
        }

        public async Task VNPayCallback(VNPayRequest request)
        {
            string vnp_HashSecret = _configuration["VNPay:vnp_HashSecret"] ?? "";

            long orderId = Convert.ToInt32(request.vnp_TxnRef);
            long vnp_Amount = Convert.ToInt64(request.vnp_Amount) / 100;

            string vnp_ResponseCode = request.vnp_ResponseCode;
            string vnp_TransactionStatus = request.vnp_TransactionStatus;

            string vnp_SecureHash = request.vnp_SecureHash;

            bool checkSignature = _vnPayLibrary.ValidateSignature(request, vnp_SecureHash, vnp_HashSecret);
            if (checkSignature)
            {
                var order = await _orderRepository.FindAsync(orderId)
                    ?? throw new ArgumentException($"Order {orderId}" + ErrorMessage.NOT_FOUND);

                if (order.Total == vnp_Amount)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        order.PaymentTranId = request.vnp_TransactionNo;

                        order.AmountPaid = vnp_Amount;
                        order.OrderStatus = DeliveryStatusEnum.Confirmed;

                        await _orderRepository.UpdateAsync(order);
                        _cache.Remove("Order " + orderId);
                    }
                    else throw new Exception(ErrorMessage.PAYMENT_FAILED);
                }
                else throw new ArgumentException("Số tiền " + ErrorMessage.INVALID);
            }
        }

        public async Task<string> GetPayOSURL(PayOSOrderInfo orderInfo)
        {
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

        public async Task PayOSCallback(PayOSRequest request)
        {
            if (request.Code == "00")
            {
                var order = await _orderRepository.FindAsync(request.OrderCode);
                if (order != null)
                {
                    var paymentInfo = await _payOS.getPaymentLinkInformation(request.OrderCode);
                    if (request.Cancel || request.Status == "CANCELLED")
                    {
                        _cache.Remove("Order " + request.OrderCode);
                        order.OrderStatus = DeliveryStatusEnum.Canceled;
                        await _orderRepository.UpdateAsync(order);
                        throw new InvalidDataException(ErrorMessage.PAYMENT_FAILED);
                    }
                    else
                    {
                        if (paymentInfo.amount == order.Total)
                        {
                            if (paymentInfo.status == "PAID")
                            {
                                order.PaymentTranId = request.Id;
                                order.AmountPaid = paymentInfo.amountPaid;
                                order.OrderStatus = DeliveryStatusEnum.Confirmed;
                            }
                            await _orderRepository.UpdateAsync(order);
                        }
                        else throw new ArgumentException("Số tiền " + ErrorMessage.INVALID);
                    }
                }
                else throw new InvalidOperationException(ErrorMessage.NOT_FOUND + " đơn hàng");
            }
            else
            {
                _cache.Remove("Order " + request.OrderCode);
                throw new Exception(ErrorMessage.ERROR);
            }
        }
    }
}
