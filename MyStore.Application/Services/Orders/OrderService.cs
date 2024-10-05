using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyStore.Application.DTOs;
using MyStore.Application.ICaching;
using MyStore.Application.ILibrary;
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
using Net.payOS;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Net.Http.Json;

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
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IConfiguration _configuration;

        private readonly IMapper _mapper;
        private readonly ICache _cache;
        private readonly ITransactionRepository _transaction;
        private readonly IVNPayLibrary _vnPayLibrary;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly PayOS _payOS;

        public OrderService(IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductSizeRepository productSizeRepository,
            IProductRepository productRepository,
            IPaymentMethodRepository paymentMethodRepository,
            ITransactionRepository transaction,
            IPaymentService paymentService,
            IPaymentMethodRepository methodRepository,
            ICache cache, IVNPayLibrary vnPayLibrary,
            IConfiguration configuration, IServiceScopeFactory serviceScopeFactory,
            PayOS payOS, IUserVoucherRepository userVoucherRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _userVoucherRepository = userVoucherRepository;
            _paymentService = paymentService;
            _paymentMethodRepository = paymentMethodRepository;
            _vnPayLibrary = vnPayLibrary;
            _payOS = payOS;

            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            _mapper = mapper;
            _cache = cache;
            _transaction = transaction;
        }
        struct OrderCache
        {
            public string Url { get; set; }
            public long OrderId { get; set; }
            public string? vnp_IpAddr { get; set; }
            public string? vnp_CreateDate { get; set; }
            public string? vnp_OrderInfo { get; set; }
        }

        public async Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch)
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

                var method = await _paymentMethodRepository
                    .SingleOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.IsActive) 
                    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND + " phương thức thanh toán");

                order.PaymentMethodId = request.PaymentMethodId;
                order.PaymentMethodName = method.Name;
                await _orderRepository.AddAsync(order);

                double total = 0;
                double voucherDiscount = 0;

                var cartItems = await _cartItemRepository.GetAsync(e => e.UserId == userId && request.CartIds.Contains(e.Id));

                var lstpSizeUpdate = new ConcurrentBag<ProductSize>();
                var lstProductUpdate = new ConcurrentBag<Product>(); //truy cap dong thoi (da luong)
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
                        SizeId = size.SizeId,
                        Quantity = cartItem.Quantity,
                        ColorName = size.ProductColor.ColorName,
                        ColorId = size.ProductColorId,
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

                if (method.Name == PaymentMethodEnum.VNPay.ToString())
                {
                    var orderInfo = new VNPayOrderInfo
                    {
                        OrderId = order.Id,
                        Amount = total,
                        CreatedDate = order.OrderDate,
                        Status = order.OrderStatus?.ToString() ?? "0",
                        OrderDesc = "Thanh toan don hang: " + order.Id,
                    };

                    var userIP = "127.0.0.1";
                    var paymentUrl = _paymentService.GetVNPayURL(orderInfo, userIP);

                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id,
                        Url = paymentUrl,
                        vnp_CreateDate = order.OrderDate.ToString("yyyyMMddHHmmss"),
                        vnp_IpAddr = userIP,
                        vnp_OrderInfo = orderInfo.OrderDesc,
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
                    };
                    cacheOptions.RegisterPostEvictionCallback(OnVNPayDeadline, this);
                    _cache.Set("Order " + order.Id, orderCache, cacheOptions);

                    return paymentUrl;
                }
                else if(method.Name == PaymentMethodEnum.PayOS.ToString())
                {
                    var orders = new PayOSOrderInfo
                    {
                        OrderId = order.Id,
                        Amount = total,
                        Products = details.Select(e => new ProductInfo
                        {
                            Name = e.ProductName,
                            Price = e.Price,
                            Quantity = e.Quantity
                        })
                    };

                    var paymentUrl = await _paymentService.GetPayOSURL(orders);

                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id,
                        Url = paymentUrl,
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(15)
                    };
                    cacheOptions.RegisterPostEvictionCallback(OnPayOSDeadline, this);
                    _cache.Set("Order " + order.Id, orderCache, cacheOptions);

                    return paymentUrl;
                }
                else return null;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex.Message);
            }
        }

        private async void OnVNPayDeadline(object key, object? value, EvictionReason reason, object? state)
        {
            if(value != null)
            {
                using var _scope = _serviceScopeFactory.CreateScope();
                var orderRepository = _scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var vnPayLibrary = _scope.ServiceProvider.GetRequiredService<IVNPayLibrary>();
                var configuration = _scope.ServiceProvider.GetRequiredService<IConfiguration>();


                var data = (OrderCache) value;
                var vnp_QueryDrUrl = configuration["VNPay:vnp_QueryDrUrl"] ?? throw new Exception(ErrorMessage.ERROR);
                var vnp_HashSecret = configuration["VNPay:vnp_HashSecret"] ?? throw new Exception(ErrorMessage.ERROR); 
                var vnp_TmnCode = configuration["VNPay:vnp_TmnCode"] ?? throw new Exception(ErrorMessage.ERROR);

                var queryDr = new VNPayQueryDr
                {
                    vnp_Command = "querydr",
                    vnp_RequestId = data.OrderId.ToString(),
                    vnp_Version = _vnPayLibrary.VERSION,
                    vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss"),
                    vnp_TransactionDate = data.vnp_CreateDate,
                    vnp_IpAddr = data.vnp_IpAddr,
                    vnp_OrderInfo = data.vnp_OrderInfo,
                    vnp_TmnCode = vnp_TmnCode,
                    vnp_TxnRef = data.OrderId.ToString()
                };
                var checksum = vnPayLibrary.CreateSecureHashQueryDr(queryDr, vnp_HashSecret);

                var queryDrWithHash = new
                {
                    queryDr.vnp_Command,
                    queryDr.vnp_RequestId,
                    queryDr.vnp_Version,
                    queryDr.vnp_CreateDate,
                    queryDr.vnp_TransactionDate,
                    queryDr.vnp_IpAddr,
                    queryDr.vnp_OrderInfo,
                    queryDr.vnp_TmnCode,
                    queryDr.vnp_TxnRef,
                    vnp_SecureHash = checksum
                };

                using var httpClient = new HttpClient();

                var res = await httpClient.PostAsJsonAsync(vnp_QueryDrUrl, queryDrWithHash);
                VNPayQueryDrResponse? queryDrResponse = await res.Content.ReadFromJsonAsync<VNPayQueryDrResponse?>();

                if (queryDrResponse != null)
                {
                    bool checkSignature = vnPayLibrary
                        .ValidateQueryDrSignature(queryDrResponse, queryDrResponse.vnp_SecureHash, vnp_HashSecret);
                    if(checkSignature && queryDrResponse.vnp_ResponseCode == "00")
                    {
                        var order = await orderRepository.FindAsync(data.OrderId);
                        if (order != null)
                        {
                            long vnp_Amount = Convert.ToInt64(queryDrResponse.vnp_Amount) / 100;

                            if (queryDrResponse.vnp_TransactionStatus == "00" && vnp_Amount == order.Total)
                            {
                                order.PaymentTranId = queryDrResponse.vnp_TransactionNo;
                                order.AmountPaid = vnp_Amount;
                                order.OrderStatus = DeliveryStatusEnum.Confirmed;
                            }
                            else
                            {
                                order.OrderStatus = DeliveryStatusEnum.Canceled;
                            }
                            await orderRepository.UpdateAsync(order);
                        }
                    }
                }
            }
        }
        private async void OnPayOSDeadline(object key, object? value, EvictionReason reason, object? state)
        {
            if (value != null)
            {
                using var _scope = _serviceScopeFactory.CreateScope();
                var orderRepository = _scope.ServiceProvider.GetRequiredService<IOrderRepository>();
                var payOS = _scope.ServiceProvider.GetRequiredService<PayOS>();

                var data = (OrderCache) value;

                var paymentInfo = await payOS.getPaymentLinkInformation(data.OrderId);
                if(paymentInfo.status == "PAID")
                {
                    var order = await orderRepository.FindAsync(data.OrderId);
                    if(order != null)
                    {
                        if (paymentInfo.amount == order.Total)
                        {
                            order.PaymentTranId = paymentInfo.id;
                            order.AmountPaid = paymentInfo.amountPaid;
                            order.OrderStatus = DeliveryStatusEnum.Confirmed;
                        }
                        else
                        {
                            order.OrderStatus = DeliveryStatusEnum.Canceled;
                        }
                        await orderRepository.UpdateAsync(order);
                    }
                }
                else if(paymentInfo.status != "CANCELLED")
                {
                    await payOS.cancelPaymentLink(data.OrderId);
                }
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

            var items = _mapper.Map<IEnumerable<OrderDTO>>(orders).Select(x =>
            {
                x.PayBackUrl = _cache.Get<OrderCache?>("Order " + x.Id)?.Url;
                return x;
            });

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

                    _cache.Remove("Order " + orderId);
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
