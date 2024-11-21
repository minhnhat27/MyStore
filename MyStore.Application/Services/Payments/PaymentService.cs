using AutoMapper;
using Microsoft.Extensions.Configuration;
using MyStore.Application.DTOs;
using MyStore.Application.ICaching;
using MyStore.Application.ILibrary;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.ModelView;
using MyStore.Application.Services.Orders;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using Net.payOS;

namespace MyStore.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IVNPayLibrary _vnPayLibrary;
        private readonly PayOS _payOS;

        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly ICache _cache;
        public PaymentService(IPaymentMethodRepository paymentMethodRepository, IVNPayLibrary vnPayLibrary,
            IMapper mapper, IConfiguration configuration, PayOS payOS, 
            IOrderRepository orderRepository, IOrderService orderService, ICache cache)
        {
            _paymentMethodRepository = paymentMethodRepository;
            _mapper = mapper;
            _configuration = configuration;
            _vnPayLibrary = vnPayLibrary;
            _payOS = payOS;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _cache = cache;
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
                var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId)
                    ?? throw new ArgumentException($"Order {orderId}" + ErrorMessage.NOT_FOUND);

                if (order.Total == vnp_Amount)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        order.PaymentTranId = request.vnp_TransactionNo;
                        order.AmountPaid = vnp_Amount;
                        order.OrderStatus = DeliveryStatusEnum.Confirmed;

                        await _orderService.SendEmailConfirmOrder(order, order.OrderDetails);
                        await _orderRepository.UpdateAsync(order);
                        _cache.Remove("Order " + orderId);
                    }
                    else
                    {
                        //if (vnp_ResponseCode == "24" || vnp_ResponseCode == "10")
                        //{
                        //    await _orderService.CancelOrder(orderId);
                        //    _cache.Remove("Order " + orderId);
                        //}
                        throw new Exception(ErrorMessage.PAYMENT_FAILED);
                    }
                }
                else throw new ArgumentException("Số tiền " + ErrorMessage.INVALID);
            }
        }
        public async Task PayOSCallback(PayOSRequest request)
        {
            if (request.Code == "00")
            {
                var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == request.OrderCode)
                    ?? throw new InvalidOperationException(ErrorMessage.NOT_FOUND + " đơn hàng");

                var paymentInfo = await _payOS.getPaymentLinkInformation(request.OrderCode);
                if (paymentInfo.status == "PAID")
                {
                    if (paymentInfo.amountPaid > 0)
                    {
                        order.AmountPaid = paymentInfo.amountPaid;
                        if (paymentInfo.amountPaid >= order.Total)
                        {
                            order.PaymentTranId = request.Id;
                            order.OrderStatus = DeliveryStatusEnum.Confirmed;
                            _cache.Remove("Order " + request.OrderCode);
                            await _orderRepository.UpdateAsync(order);
                            await _orderService.SendEmailConfirmOrder(order, order.OrderDetails);
                        }
                        else
                        {
                            await _orderRepository.UpdateAsync(order);
                            throw new ArgumentException("Số tiền " + ErrorMessage.INVALID);
                        }
                    }
                    else throw new Exception(ErrorMessage.PAYMENT_FAILED);
                }
                else
                {
                    await _payOS.cancelPaymentLink(order.Id);
                    _cache.Remove("Order " + request.OrderCode);
                    await _orderService.CancelOrder(order.Id);
                    throw new Exception(ErrorMessage.PAYMENT_FAILED);
                }
            }
            else
            {
                _cache.Remove("Order " + request.OrderCode);
                await _orderService.CancelOrder(request.OrderCode);
                throw new Exception(ErrorMessage.PAYMENT_FAILED);
            }
        }

    }
}
