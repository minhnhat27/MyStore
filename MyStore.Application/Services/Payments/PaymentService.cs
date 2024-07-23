using MyStore.Application.IRepository.Orders;

namespace MyStore.Application.Services.Payments
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        public PaymentService(IPaymentMethodRepository paymentMethodRepository)
        {
            _paymentMethodRepository = paymentMethodRepository;
        }
        public async Task<IEnumerable<string>> GetPaymentMethodsIsActive()
        {
            var payment = await _paymentMethodRepository.GetPaymentMethodsIsActiveAsync();
            return payment.Select(e => e.Name);
        }

        public async Task<IEnumerable<string>> GetPaymentMethods()
        {
            var payment = await _paymentMethodRepository.GetAllAsync();
            return payment.Select(e => e.Name);
        }
    }
}
