using AutoMapper;
using MyStore.Application.DTO;
using MyStore.Application.ICaching;
using MyStore.Application.IRepository;
using MyStore.Application.IRepository.Orders;
using MyStore.Application.IRepository.Products;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;

namespace MyStore.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductSizeRepository _productSizeRepository;
        private readonly ICache _orderCache;
        private readonly IMapper _mapper;
        private readonly ITransactionRepository _transaction;
        public OrderService(IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductSizeRepository productSizeRepository,
            IProductRepository productRepository,
            ICache cache, IMapper mapper, ITransactionRepository transaction)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _orderCache = cache;
            _mapper = mapper;
            _transaction = transaction;
        }

        public async Task<IEnumerable<OrderDTO>> GetOrders()
        {
            var orders = await _orderRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }
        
        public async Task<PagedResponse<OrderDTO>> GetOrders(int page, int pageSize, string? keySearch)
        {
            int totalOrder;
            IEnumerable<Order> orders;
            if (string.IsNullOrEmpty(keySearch))
            {
                totalOrder = await _orderRepository.CountAsync();
                orders = await _orderRepository.GetPagedAsync(page, pageSize);
            }
            else
            {
                totalOrder = await _orderRepository.CountAsync(keySearch);
                orders = await _orderRepository.GetPagedAsync(page, pageSize, keySearch);
            }
            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);

            return new PagedResponse<OrderDTO>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalOrder
            };
        }

        public async Task<OrderDTO> GetOrder(int id)
        {
            var order = await _orderRepository.FindAsync(id);
            if (order != null)
            {
                return _mapper.Map<OrderDTO>(order);
            }
            else throw new ArgumentException($"Id {id}" + ErrorMessage.NOT_FOUND);
        }

        public async Task<OrderDTO> CreateOrder(string userId, OrderRequest request)
        {
            try
            {
                using(var transaction = await _transaction.BeginTransactionAsync())
                {
                    var order = _mapper.Map<Order>(request);                                                   
                    order.OrderDate = DateTime.Now;
                    order.UserId = userId;
                    double total = 0;   

                    var orderDetailsTasks = request.ProductsAndQuantities.Select(async e =>
                    {
                        var product = await _productRepository.FindAsync(e.ProductId);
                        if(product != null)
                        {
                            var productSize = product.Sizes.SingleOrDefault(s => s.SizeId == e.SizeId);
                            double discountPercent = product.DiscountPercent;
                            double discount = discountPercent / 100.0 * product.Price;
                            var price = product.Price - discount;
                            total += price;

                            return new OrderDetail
                            {
                                OrderId = order.Id,
                                ProductId = e.ProductId,
                                Size = e.SizeId,
                                Quantity = e.Quantity,
                                UnitPrice = price,
                            };
                        }
                        return null;
                    });
                    var orderDetails = (await Task.WhenAll(orderDetailsTasks)).Where(e => e != null);
                    order.Total = total;

                    await _orderRepository.AddAsync(order);
                    await _orderDetailRepository.AddAsync(orderDetails);

                    await _cartItemRepository.DeleteRangeByUserId(userId, request.ProductsAndQuantities.Select(e => e.ProductId));

                    await _transaction.CommitTransactionAsync();
                    
                    return _mapper.Map<OrderDTO>(order);
                }
            }
            catch (Exception ex)
            {
                await _transaction.RollbackTransactionAsync();
                throw new Exception(ex.InnerException?.Message ?? ex.Message);
            }
        }

        public async Task<OrderDTO> UpdateOrder(int id, string userId, UpdateOrderRequest request)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if(order != null && order.OrderStatusName.Equals(DeliveryStatus.Proccessing.ToString()))
            {
                if(request.ShippingAddress != null)
                {
                    order.ShippingAddress = request.ShippingAddress;
                }
                if(request.ReceiverInfo != null)
                {
                    order.ReceiverInfo = request.ReceiverInfo;
                }
                await _orderRepository.UpdateAsync(order);
                return _mapper.Map<OrderDTO>(order);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task DeleteOrder(int id, string userId)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if (order != null)
            {
                await _orderRepository.DeleteAsync(order);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<IEnumerable<OrderDTO>> GetOrdersByUserId(string userId)
        {
            var orders = await _orderRepository.GetAsync(e => e.UserId == userId);
            return _mapper.Map<IEnumerable<OrderDTO>>(orders);
        }

        public async Task CancelOrder(int id)
        {
            var order = await _orderRepository.FindAsync(id);
            if (order != null)
            {
                if (order.OrderStatusName == DeliveryStatus.Proccessing.ToString()
                    || order.OrderStatusName == DeliveryStatus.Confirmed.ToString())
                {
                    order.OrderStatusName = DeliveryStatus.Canceled.ToString();
                    await _orderRepository.UpdateAsync(order);
                }
                else throw new Exception(ErrorMessage.CANNOT_CANCEL);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<OrderDetailResponse> GetOrderDetail(int id)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id);
            if (order != null)
            {
                var orderDetail = await _orderDetailRepository.GetAsync(e => e.OrderId == id);
                var products = _mapper.Map<IEnumerable<ProductsOrderDetail>>(orderDetail);

                var res = _mapper.Map<OrderDetailResponse>(order);
                res.Products = products;

                return res;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
