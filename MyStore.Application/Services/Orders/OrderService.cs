using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MyStore.Application.DTO;
using MyStore.Application.ICaching;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.IRepositories.Users;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Application.Services.Payments;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using System.Linq.Expressions;

namespace MyStore.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IProductRepository _productRepository;
        private readonly IProductSizeRepository _productSizeRepository;

        private readonly IUserVoucherRepository _userVoucherRepository;
        private readonly IPaymentService _paymentService;

        private readonly IMapper _mapper;
        private readonly ICache _cache;
        private readonly ITransactionRepository _transaction;
        public OrderService(IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductSizeRepository productSizeRepository,
            IProductRepository productRepository,
            IPaymentMethodRepository paymentMethodRepository,
            ITransactionRepository transaction,
            IPaymentService paymentService,
            ICache cache,
            IUserVoucherRepository userVoucherRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _userVoucherRepository = userVoucherRepository;
            _paymentService = paymentService;

            _mapper = mapper;
            _cache = cache;
            _transaction = transaction;
        }
        struct OrderCache
        {
            public string Url { get; set; }
            public string OrderId { get; set; }
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
                orders = await _orderRepository.GetPagedOrderByDescendingAsync(page, pageSize, null, e => e.CreatedAt);
            }
            else
            {
                Expression<Func<Order, bool>> expression =
                    e => e.Id.ToString().Contains(keySearch)
                        || (e.OrderStatus != null && e.OrderStatus.Value.ToString().Contains(keySearch));
                //|| (e.PaymentMethodName != null && e.PaymentMethodName.Contains(keySearch)

                totalOrder = await _orderRepository.CountAsync(expression);
                orders = await _orderRepository.GetPagedOrderByDescendingAsync(page, pageSize, expression, e => e.CreatedAt);
            }
            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders).Select(x =>
            {
                x.PayBackUrl = _cache.Get<OrderCache?>("Order " + x.Id)?.Url;
                return x;
            });

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

        private double CalcShip(double price) => price >= 400000 ? 0 : price >= 200000 ? 10000 : 30000;

        public async Task<string?> CreateOrder(string userId, OrderRequest request)
        {
            using var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var order = new Order
                {
                    DeliveryAddress = request.DeliveryAddress,
                    OrderDate = now,
                    UserId = userId,
                    Receiver = request.Receiver,
                    Total = request.Total,
                };

                var method = await _paymentService.IsActivePaymentMethod(request.PaymentMethodId) 
                    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND);

                order.PaymentMethodId = request.PaymentMethodId;
                await _orderRepository.AddAsync(order);

                double total = 0;
                double voucherDiscount = 0;

                var cartItems = await _cartItemRepository.GetAsync(e => e.UserId == userId && request.CartIds.Contains(e.Id));

                var lstpSizeUpdate = new List<ProductSize>();
                var lstProductUpdate = new List<Product>();
                var details = await Task.WhenAll(cartItems.Select(async cartItem =>
                {
                    var size = await _productSizeRepository
                        .SingleAsyncInclude(e => e.ProductColorId == cartItem.ColorId && e.SizeId == cartItem.SizeId);
                    
                    if (size.InStock < cartItem.Quantity)
                    {
                        throw new Exception(ErrorMessage.SOLDOUT);
                    }

                    double price = cartItem.Product.Price - cartItem.Product.Price * (cartItem.Product.DiscountPercent / 100.0);
                    price *= cartItem.Quantity;
                    total += price;

                    cartItem.Product.Sold += cartItem.Quantity;
                    lstProductUpdate.Add(cartItem.Product);

                    size.InStock -= cartItem.Quantity;
                    lstpSizeUpdate.Add(size);

                    return new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        SizeName = size.Size.Name,
                        //SizeId = size.SizeId,
                        Quantity = cartItem.Quantity,
                        ColorName = size.ProductColor.ColorName,
                        //ColorId = size.ProductColorId,
                        ProductName = cartItem.Product.Name,
                        OriginPrice = cartItem.Product.Price,
                        Price = price,
                    };
                }));

                UserVoucher? voucher = null;
                if (request.Code != null)
                {
                    voucher = await _userVoucherRepository.SingleOrDefaultAsyncInclude(x => x.UserId == userId && x.VoucherCode == request.Code);
                    if (voucher == null
                        || voucher.Voucher.EndDate < now
                        || voucher.Voucher.MinOrder > total)
                    {
                        throw new ArgumentException(ErrorMessage.INVALID_VOUCHER);
                    }

                    //if (voucher.Voucher.DiscountPercent.HasValue)
                    //{
                    //    voucherDiscount = total * (voucher.Voucher.DiscountPercent.Value / 100.0);
                    //}
                    //else if (voucher.Voucher.DiscountAmount.HasValue)
                    //{
                    //    voucherDiscount = voucher.Voucher.DiscountAmount.Value;
                    //}

                    voucherDiscount = voucher.Voucher.DiscountPercent.HasValue
                        ? total * (voucher.Voucher.DiscountPercent.Value / 100.0)
                        : voucher.Voucher.DiscountAmount ?? 0;

                    if (voucher.Voucher.MaxDiscount.HasValue && voucherDiscount > voucher.Voucher.MaxDiscount)
                    {
                        voucherDiscount = voucher.Voucher.MaxDiscount.Value;
                    }
                }

                double shipCost = CalcShip(total);
                order.ShippingCost = shipCost;

                total = total - voucherDiscount + shipCost;

                if (total != request.Total)
                {
                    throw new Exception(ErrorMessage.BAD_REQUEST);
                }

                await _productSizeRepository.UpdateAsync(lstpSizeUpdate);
                await _productRepository.UpdateAsync(lstProductUpdate);
                await _orderDetailRepository.AddAsync(details);
                await _cartItemRepository.DeleteRangeAsync(cartItems);
                if (voucher != null)
                {
                    voucher.Used = true;
                    await _userVoucherRepository.UpdateAsync(voucher);
                }

                await transaction.CommitAsync();

                if (method != PaymentMethodEnum.COD.ToString())
                {
                    var orderInfo = new OrderInfo
                    {
                        OrderId = order.Id,
                        Amount = total,
                        CreatedDate = order.OrderDate,
                        Status = order.OrderStatus?.ToString() ?? "0",
                        OrderDesc = string.Join("\n ", cartItems.Select(e => $"{e.Quantity} x {e.Product.Name}"))
                    };

                    var paymentUrl = _paymentService.GetVNPayURL(orderInfo, request.UserIP ?? "127.0.0.1");

                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id.ToString(),
                        Url = paymentUrl,
                    };
                    _cache.Set("Order " + order.Id.ToString(), orderCache, TimeSpan.FromMinutes(15));

                    return paymentUrl;
                }
                return null;
                //return _mapper.Map<OrderDTO>(order);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        public async Task<OrderDTO> UpdateOrder(int id, string userId, UpdateOrderRequest request)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id && e.UserId == userId);
            if(order != null && order.OrderStatus.Equals(DeliveryStatusEnum.Processing))
            {
                if(request.DeliveryAddress != null)
                {
                    order.DeliveryAddress = request.DeliveryAddress;
                }
                if(request.ReceiverInfo != null)
                {
                    order.DeliveryAddress = request.ReceiverInfo;
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

        public async Task<PagedResponse<OrderDTO>> GetOrdersByUserId(string userId, PageRequest page)
        {
            var orders = await _orderRepository.GetPagedOrderByDescendingAsync(page.Page, page.PageSize, e => e.UserId == userId, x => x.CreatedAt);
            var total = await _orderRepository.CountAsync(e => e.UserId == userId);
            
            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders);

            return new PagedResponse<OrderDTO>
            {
                Items = items,
                TotalItems = total,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task CancelOrder(int orderId, string userId)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == orderId && e.UserId == userId);
            if (order != null)
            {
                if (order.OrderStatus.Equals(DeliveryStatusEnum.Processing)
                    || order.OrderStatus.Equals(DeliveryStatusEnum.Confirmed))
                {
                    order.OrderStatus = DeliveryStatusEnum.Canceled;
                    //var updateProduct = order.OrderDetails.Select(dt =>
                    //{
                    //    if(dt.Product != null)
                    //    {
                    //        dt.Product.Sold += dt.Quantity;
                    //    }
                    //    return dt.Product;
                    //});

                    await _orderRepository.UpdateAsync(order);
                }
                else throw new Exception(ErrorMessage.CANNOT_CANCEL);
            }
            else throw new ArgumentException($"Id {orderId} " + ErrorMessage.NOT_FOUND);
        }

        public async Task<OrderDetailsResponse> GetOrderDetail(int id)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id);
            if (order != null)
            {
                var orderDetail = await _orderDetailRepository.GetAsync(e => e.OrderId == id);
                var products = _mapper.Map<IEnumerable<ProductsOrderDetail>>(orderDetail);

                var res = _mapper.Map<OrderDetailsResponse>(order);
                res.Products = products;

                return res;
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }
    }
}
