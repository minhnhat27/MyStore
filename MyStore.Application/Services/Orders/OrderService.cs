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
using MyStore.Application.ISendMail;
using MyStore.Application.IStorage;
using MyStore.Application.ModelView;
using MyStore.Application.Request;
using MyStore.Application.Response;
using MyStore.Application.Services.FlashSales;
using MyStore.Application.Services.Payments;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Domain.Enumerations;
using Net.payOS;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

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
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IProductReviewRepository _productReviewRepository;

        private readonly IFileStorage _fileStorage;
        private readonly IConfiguration _configuration;

        private readonly IMapper _mapper;
        private readonly ICache _cache;
        private readonly ITransactionRepository _transaction;
        private readonly IVNPayLibrary _vnPayLibrary;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly IFlashSaleService _flashSaleService;
        private readonly IFlashSaleRepository _flashSaleRepository;
        private readonly ISendMailService _sendMailService;
        private readonly IUserRepository _userRepository;

        private readonly IPaymentProcessor _paymentProcessor;

        private readonly string reviewImagesPath = "assets/images/reviews";

        public OrderService(IOrderRepository orderRepository,
            ICartItemRepository cartItemRepository,
            IOrderDetailRepository orderDetailRepository,
            IProductSizeRepository productSizeRepository,
            IProductRepository productRepository,
            IPaymentMethodRepository paymentMethodRepository,
            ITransactionRepository transaction,
            IPaymentProcessor paymentProcessor,
            IProductReviewRepository productReviewRepository,
            IFlashSaleService flashSaleService,
            ISendMailService sendMailService,
            IFlashSaleRepository flashSaleRepository, IUserRepository userRepository,
            ICache cache, IVNPayLibrary vnPayLibrary, IFileStorage fileStorage,
            IConfiguration configuration, IServiceScopeFactory serviceScopeFactory,
            IUserVoucherRepository userVoucherRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _cartItemRepository = cartItemRepository;
            _orderDetailRepository = orderDetailRepository;
            _productRepository = productRepository;
            _productSizeRepository = productSizeRepository;
            _userVoucherRepository = userVoucherRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _productReviewRepository = productReviewRepository;
            _fileStorage = fileStorage;

            _vnPayLibrary = vnPayLibrary;

            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _flashSaleService = flashSaleService;
            _flashSaleRepository = flashSaleRepository;
            _sendMailService = sendMailService;
            _userRepository = userRepository;

            _mapper = mapper;
            _cache = cache;
            _transaction = transaction;
            _paymentProcessor = paymentProcessor;
        }
        struct OrderCache
        {
            public string Url { get; set; }
            public long OrderId { get; set; }
            public string? vnp_IpAddr { get; set; }
            public string? vnp_CreateDate { get; set; }
            public string? vnp_OrderInfo { get; set; }
            public DateTime Deadline { get; set; }
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
                bool isLong = long.TryParse(keySearch, out long idSearch);
                keySearch = keySearch.ToLower();

                Expression<Func<Order, bool>> expression =
                    e => e.Id.Equals(idSearch)
                        || e.PaymentMethodName.ToLower().Contains(keySearch);

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

        public async Task<PagedResponse<OrderDTO>> GetAll(int page, int pageSize, string? keySearch, DeliveryStatusEnum statusEnum)
        {
            int totalOrder;
            IEnumerable<Order> orders;

            Expression<Func<Order, DateTime?>> sortExpression = e => e.UpdatedAt;

            if (statusEnum == DeliveryStatusEnum.Processing)
            {
                sortExpression = e => e.CreatedAt;
            }


            if (string.IsNullOrEmpty(keySearch))
            {
                totalOrder = await _orderRepository.CountAsync(e => e.OrderStatus == statusEnum);
                orders = await _orderRepository
                    .GetPagedOrderByDescendingAsync(page, pageSize, e => e.OrderStatus == statusEnum, sortExpression);
            }
            else
            {
                bool isLong = long.TryParse(keySearch, out long idSearch);
                keySearch = keySearch.ToLower();

                Expression<Func<Order, bool>> expression =
                    e => (e.Id.Equals(idSearch) && e.OrderStatus == statusEnum)
                        || e.PaymentMethodName.ToLower().Contains(keySearch);

                totalOrder = await _orderRepository.CountAsync(expression);
                orders = await _orderRepository.GetPagedOrderByDescendingAsync(page, pageSize, expression, sortExpression);
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

        public async Task<OrderDetailsResponse> GetOrderDetail(long orderId)
        {
            var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            return _mapper.Map<OrderDetailsResponse>(order);
        }

        public async Task<OrderDetailsResponse> GetOrderDetail(long orderId, string userId)
        {
            var order = await _orderRepository
                .SingleOrDefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId)
                    ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            return _mapper.Map<OrderDetailsResponse>(order);
        }

        public async Task<PagedResponse<OrderResponse>> GetOrdersByUserId(string userId, PageRequest page)
        {
            var orders = await _orderRepository
                .GetPagedOrderByDescendingAsyncInclude(page.Page, page.PageSize, e => e.UserId == userId, x => x.CreatedAt);
            
            var total = await _orderRepository.CountAsync(e => e.UserId == userId);

            var items = _mapper.Map<IEnumerable<OrderResponse>>(orders).Select(x =>
            {
                x.PaymentDeadline = _cache.Get<OrderCache?>("Order " + x.Id)?.Deadline;
                return x;
            });

            return new PagedResponse<OrderResponse>
            {
                Items = items,
                TotalItems = total,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task<PagedResponse<OrderResponse>> GetOrdersByUserId(string userId, PageRequest page, DeliveryStatusEnum statusEnum)
        {
            Expression<Func<Order, DateTime?>> sortExpression = e => e.UpdatedAt;

            if (statusEnum == DeliveryStatusEnum.Processing)
            {
                sortExpression = e => e.CreatedAt;
            }

            var orders = await _orderRepository
                .GetPagedOrderByDescendingAsyncInclude(page.Page, page.PageSize, 
                    e => e.UserId == userId && e.OrderStatus == statusEnum, sortExpression);

            var total = await _orderRepository.CountAsync(e => e.UserId == userId && e.OrderStatus == statusEnum);

            var items = _mapper.Map<IEnumerable<OrderResponse>>(orders).Select(x =>
            {
                x.PaymentDeadline = _cache.Get<OrderCache?>("Order " + x.Id)?.Deadline;
                return x;
            });

            return new PagedResponse<OrderResponse>
            {
                Items = items,
                TotalItems = total,
                Page = page.Page,
                PageSize = page.PageSize
            };
        }

        public async Task SendEmailConfirmOrder(Order order, IEnumerable<OrderDetail> orderDetails)
        {
            var path = _sendMailService.OrderConfirmEmailPath;
            var productPath = _sendMailService.ProductListEmailPath;
            if (File.Exists(path) && File.Exists(productPath))
            {
                var user = await _userRepository.SingleAsync(e => e.Id == order.UserId);
                var storeName = _configuration["Store:Name"];
                var subject = $"[{storeName}] Xác nhận đặt hàng";

                string body = File.ReadAllText(path);
                body = body.Replace("{NAME}", user.Fullname);
                body = body.Replace("{SHOP_NAME}", storeName);
                if(order.AmountPaid == order.Total)
                {
                    body = body.Replace("{ORDER_STATUS}", "đã được thanh toán");
                }
                else
                {
                    body = body.Replace("{ORDER_STATUS}", "đang được xử lý");
                }
                body = body.Replace("{ORDERID}", "#" + order.Id); 

                var culture = new CultureInfo("vi-VN");
                body = body.Replace("{TOTAL}", order.Total.ToString("C0", culture));
                body = body.Replace("{RECEIVER}", order.Receiver);
                body = body.Replace("{DELIVERY_ADDRESS}", order.DeliveryAddress);

                var product_List = File.ReadAllText(productPath);
                string lstProductBody = "";
                foreach (var item in orderDetails)
                {
                    string kq = product_List.Replace("{PRODUCT_NAME}", item.ProductName);
                    kq = kq.Replace("{VARIANT}", item.Variant);
                    kq = kq.Replace("{QUANTITY}", item.Quantity.ToString());
                    lstProductBody += kq;
                }
                body = body.Replace("{PRODUCT_LIST}", lstProductBody);
                _ = Task.Run(() => _sendMailService.SendMailToOne(user.Email!, subject, body));
            }
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
                    DistrictID = request.DistrictID,
                    WardID = request.WardID,
                };

                var method = await _paymentMethodRepository
                    .SingleOrDefaultAsync(x => x.Id == request.PaymentMethodId && x.IsActive) 
                    ?? throw new ArgumentException(ErrorMessage.NOT_FOUND + " phương thức thanh toán");

                order.PaymentMethodId = request.PaymentMethodId;
                order.PaymentMethodName = method.Name;
                await _orderRepository.AddAsync(order);

                double total = 0;
                double voucherDiscount = 0;

                var cartItems = await _cartItemRepository
                    .GetAsync(e => e.UserId == userId && request.CartIds.Contains(e.Id));

                var lstpSizeUpdate = new List<ProductSize>();
                var lstProductUpdate = new List<Product>();
                var lstDetails = new List<OrderDetail>();
                var lstFlashSaleUpdate = new List<FlashSale>();

                var listFlashSaleDiscount = await _flashSaleService.GetFlashSaleProductsWithDiscountThisTime();

                foreach (var cartItem in cartItems)
                {
                    if (!cartItem.Product.Enable)
                    {
                        throw new Exception($"{cartItem.Product.Name} không tồn tại hoặc đã bị ẩn.");
                    }

                    var size = await _productSizeRepository
                        .SingleAsyncInclude(e => e.ProductColorId == cartItem.ColorId && e.SizeId == cartItem.SizeId);

                    if (size.InStock < cartItem.Quantity)
                    {
                        throw new Exception($"{cartItem.Product.Name} " + ErrorMessage.SOLDOUT);
                    }
                    var discount = cartItem.Product.DiscountPercent;

                    var flashSaleDiscount = listFlashSaleDiscount.SingleOrDefault(e => e.ProductId == cartItem.ProductId);
                    if(flashSaleDiscount != null)
                    {
                        discount = flashSaleDiscount.DiscountPercent;
                    }

                    double price = cartItem.Product.Price - cartItem.Product.Price * (discount / 100.0);
                    //price *= cartItem.Quantity;
                    total += price * cartItem.Quantity;

                    cartItem.Product.Sold += cartItem.Quantity;
                    lstProductUpdate.Add(cartItem.Product);

                    size.InStock -= cartItem.Quantity;
                    lstpSizeUpdate.Add(size);

                    if(flashSaleDiscount != null)
                    {
                        var flashSale = await _flashSaleRepository
                            .SingleAsync(e => e.Id == flashSaleDiscount.FlashSaleId);
                        flashSale.TotalSold += cartItem.Quantity;
                        flashSale.TotalRevenue += price * cartItem.Quantity;
                        lstFlashSaleUpdate.Add(flashSale);
                    }

                    lstDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        SizeId = size.SizeId,
                        Quantity = cartItem.Quantity,
                        Variant = size.ProductColor.ColorName + ", Size " + size.Size.Name,
                        ColorId = size.ProductColorId,
                        ProductName = cartItem.Product.Name,
                        OriginPrice = cartItem.Product.Price,
                        Price = price,
                        ImageUrl = size.ProductColor.ImageUrl,
                    });
                }

                UserVoucher? voucher = null;
                if (request.Code != null)
                {
                    voucher = await _userVoucherRepository
                        .SingleOrDefaultAsyncInclude(x => x.UserId == userId && !x.Used && x.VoucherCode == request.Code);
                                        
                    if (voucher == null
                        || voucher.Voucher.EndDate < now
                        || voucher.Voucher.MinOrder > total)
                    {
                        throw new ArgumentException(ErrorMessage.INVALID_VOUCHER);
                    }

                    voucherDiscount = voucher.Voucher.DiscountPercent.HasValue
                        ? total * (voucher.Voucher.DiscountPercent.Value / 100.0)
                        : voucher.Voucher.DiscountAmount ?? 0;

                    if (voucher.Voucher.MaxDiscount.HasValue && voucherDiscount > voucher.Voucher.MaxDiscount)
                    {
                        voucherDiscount = voucher.Voucher.MaxDiscount.Value;
                    }
                }
                order.VoucherDiscount = voucherDiscount;

                double shipCost = CalcShip(total);
                order.ShippingCost = shipCost;

                total = total - voucherDiscount + shipCost;

                if (total != request.Total)
                {
                    throw new Exception(ErrorMessage.BAD_REQUEST);
                }

                await _productSizeRepository.UpdateAsync(lstpSizeUpdate);
                await _productRepository.UpdateAsync(lstProductUpdate);
                await _orderDetailRepository.AddAsync(lstDetails);
                await _cartItemRepository.DeleteRangeAsync(cartItems);

                if (lstFlashSaleUpdate.Count != 0)
                {
                    await _flashSaleRepository.UpdateAsync(lstFlashSaleUpdate);
                }

                if (voucher != null)
                {
                    voucher.Used = true;
                    await _userVoucherRepository.UpdateAsync(voucher);
                }

                string? paymentUrl = null;
                if (method.Name == PaymentMethodEnum.VNPay.ToString())
                {
                    var orderDesc = "Thanh toan don hang: " + order.Id;
                    var userIP = request.UserIP ?? "127.0.0.1";
                    var deadline = order.OrderDate.AddMinutes(15);

                    paymentUrl = _paymentProcessor.GetVNPayURL(order, deadline, orderDesc, userIP);
                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id,
                        Url = paymentUrl,
                        vnp_CreateDate = order.OrderDate.ToString("yyyyMMddHHmmss"),
                        vnp_IpAddr = userIP,
                        vnp_OrderInfo = orderDesc,
                        Deadline = deadline
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = deadline
                    };
                    cacheOptions.RegisterPostEvictionCallback(OnVNPayDeadline, this);
                    _cache.Set("Order " + order.Id, orderCache, cacheOptions);

                }
                else if(method.Name == PaymentMethodEnum.PayOS.ToString())
                {
                    paymentUrl = await _paymentProcessor.GetPayOSURL(order, lstDetails);

                    var deadline = order.OrderDate.AddMinutes(15);
                    var orderCache = new OrderCache()
                    {
                        OrderId = order.Id,
                        Url = paymentUrl,
                        Deadline = deadline
                    };

                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpiration = deadline
                    };
                    cacheOptions.RegisterPostEvictionCallback(OnPayOSDeadline, this);
                    _cache.Set("Order " + order.Id, orderCache, cacheOptions);
                }
                else
                {
                    await SendEmailConfirmOrder(order, lstDetails);
                }
                await transaction.CommitAsync();
                return paymentUrl;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
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
                    if(checkSignature)
                    {
                        var order = await orderRepository.FindAsync(data.OrderId);
                        if (order != null)
                        {
                            long vnp_Amount = Convert.ToInt64(queryDrResponse.vnp_Amount) / 100;

                            if (queryDrResponse.vnp_TransactionStatus == "00" 
                                && queryDrResponse.vnp_ResponseCode == "00" 
                                && vnp_Amount == order.Total)
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
                var orderService = _scope.ServiceProvider.GetRequiredService<IOrderService>();
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
                            await orderRepository.UpdateAsync(order);
                        }
                        else
                        {
                            await orderService.CancelOrder(order.Id);
                        }
                    }
                }
                else if(paymentInfo.status != "CANCELLED")
                {
                    await payOS.cancelPaymentLink(data.OrderId);
                }
            }
        }

        public async Task<string> Repayment(string userId, long orderId)
        {
            var orderInfo = _cache.Get<OrderCache?>("Order " + orderId)
                ?? throw new InvalidDataException(ErrorMessage.PAYMENT_DUE);
            var order = await _orderRepository
                .SingleOrDefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            var paymentUrl = await _paymentProcessor.GetPayOSURL(order, order.OrderDetails);
            if (order.PaymentMethodName == PaymentMethodEnum.VNPay.ToString())
            {
                paymentUrl = _paymentProcessor.GetVNPayURL(order, orderInfo.Deadline, orderInfo.vnp_OrderInfo, orderInfo.vnp_IpAddr);
            }
            return paymentUrl;
        }

        public async Task<OrderDTO> UpdateOrder(long id, string userId, UpdateOrderRequest request)
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

        public async Task DeleteOrder(long id)
        {
            var order = await _orderRepository.SingleOrDefaultAsync(e => e.Id == id);
            if (order != null)
            {
                await _orderRepository.DeleteAsync(order);
            }
            else throw new ArgumentException($"Id {id} " + ErrorMessage.NOT_FOUND);
        }

        public async Task CancelOrder(long orderId, string userId)
        {
            var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
            
            if (order.OrderStatus.Equals(DeliveryStatusEnum.Processing) || order.OrderStatus.Equals(DeliveryStatusEnum.Confirmed))
            {
                order.OrderStatus = DeliveryStatusEnum.Canceled;
                List<Product> lstProductUpdate = new();
                List<ProductSize> lstSizeUpdate = new();

                foreach (var orderDetails in order.OrderDetails)
                {
                    var product = await _productRepository.FindAsync(orderDetails.ProductId);
                    if (product != null)
                    {
                        product.Sold -= orderDetails.Quantity;
                        lstProductUpdate.Add(product);
                    }
                    var updateInStock = await _productSizeRepository.FindAsync(orderDetails.ColorId, orderDetails.SizeId);
                    if(updateInStock != null)
                    {
                        updateInStock.InStock += orderDetails.Quantity;
                        lstSizeUpdate.Add(updateInStock);
                    }
                }
                if(lstProductUpdate.Any())
                {
                    await _productRepository.UpdateAsync(lstProductUpdate);
                }
                if (lstSizeUpdate.Any())
                {
                    await _productSizeRepository.UpdateAsync(lstSizeUpdate);
                }

                _cache.Remove("Order " + orderId);
                await _orderRepository.UpdateAsync(order);
            }
            else throw new InvalidDataException(ErrorMessage.CANNOT_CANCEL);
        }

        public async Task CancelOrder(long orderId)
        {
            var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            if (order.OrderStatus.Equals(DeliveryStatusEnum.Processing) || order.OrderStatus.Equals(DeliveryStatusEnum.Confirmed))
            {
                order.OrderStatus = DeliveryStatusEnum.Canceled;
                List<Product> lstProductUpdate = new();
                List<ProductSize> lstSizeUpdate = new();

                foreach (var orderDetails in order.OrderDetails)
                {
                    var product = await _productRepository.FindAsync(orderDetails.ProductId);
                    if (product != null)
                    {
                        product.Sold -= orderDetails.Quantity;
                        lstProductUpdate.Add(product);
                    }
                    var updateInStock = await _productSizeRepository.FindAsync(orderDetails.ColorId, orderDetails.SizeId);
                    if (updateInStock != null)
                    {
                        updateInStock.InStock += orderDetails.Quantity;
                        lstSizeUpdate.Add(updateInStock);
                    }
                }
                if (lstProductUpdate.Any())
                {
                    await _productRepository.UpdateAsync(lstProductUpdate);
                }
                if (lstSizeUpdate.Any())
                {
                    await _productSizeRepository.UpdateAsync(lstSizeUpdate);
                }

                _cache.Remove("Order " + orderId);
                await _orderRepository.UpdateAsync(order);
            }
            else throw new InvalidDataException(ErrorMessage.CANNOT_CANCEL);
        }

        public async Task Review(long orderId, string userId, IEnumerable<ReviewRequest> reviews)
        {
            using var transaction = await _transaction.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository
                    .SingleOrDefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);
                if (order.OrderStatus != DeliveryStatusEnum.Received)
                {
                    throw new InvalidDataException("Chưa thể đánh giá đơn hàng này.");
                }
                if(DateTime.Now > order.ReviewDeadline)
                {
                    throw new InvalidOperationException("Đã hết hạn đánh giá");
                }

                List<ProductReview> pReviews = new();
                List<Product> products = new();

                foreach (var rv in reviews)
                {
                    var productPath = Path.Combine(reviewImagesPath, rv.ProductId.ToString());
                    List<string>? pathNames = null;

                    if (rv.Images != null)
                    {
                        pathNames = new();
                        var imgNames = rv.Images.Select(image =>
                        {
                            var name = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                            pathNames.Add(Path.Combine(productPath, name));
                            return name;
                        }).ToList();

                        await _fileStorage.SaveAsync(productPath, rv.Images, imgNames);
                    }

                    var product = await _productRepository.FindAsync(rv.ProductId);
                    if (product != null)
                    {
                        var currentStar = product.Rating * product.RatingCount;
                        product.Rating = (currentStar + rv.Star) / (product.RatingCount + 1);
                        product.RatingCount += 1;

                        products.Add(product);
                        pReviews.Add(new ProductReview
                        {
                            ProductId = rv.ProductId,
                            UserId = userId,
                            Star = rv.Star,
                            Description = rv.Description,
                            ImagesUrls = pathNames,
                            Variant = rv.Variant
                        });
                    }
                }

                await _productReviewRepository.AddAsync(pReviews);
                await _productRepository.UpdateAsync(products);
                order.Reviewed = true;
                await _orderRepository.UpdateAsync(order);
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task NextOrderStatus(long orderId, DeliveryStatusEnum currentStatus)
        {
            var order = await _orderRepository.FindAsync(orderId) 
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            if(order.OrderStatus != currentStatus)
            {
                throw new InvalidOperationException("Trạng thái đơn hàng không hợp lệ.");
            }
            
            if (!order.OrderStatus.Equals(DeliveryStatusEnum.Received) 
                || !order.OrderStatus.Equals(DeliveryStatusEnum.Canceled))
            {
                order.OrderStatus += 1;
                if(order.OrderStatus == DeliveryStatusEnum.Received)
                {
                    order.ReceivedDate = DateTime.Now;
                    order.ReviewDeadline = DateTime.Now.AddDays(15);
                }
                await _orderRepository.UpdateAsync(order);
            }
            else throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
        }

        public async Task ConfirmDelivery(long orderId, string userId)
        {
            var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId && e.UserId == userId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            if (order.OrderStatus != DeliveryStatusEnum.BeingDelivered)
            {
                throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }

            order.OrderStatus = DeliveryStatusEnum.Received;
            order.ReceivedDate = DateTime.Now;
            order.ReviewDeadline = DateTime.Now.Date.AddDays(15);
            await _orderRepository.UpdateAsync(order);
        }

        public async Task OrderToShipping(long orderId, OrderToShippingRequest request)
        {
            var order = await _orderRepository.SingleOrDefaultAsyncInclude(e => e.Id == orderId)
                ?? throw new InvalidOperationException(ErrorMessage.ORDER_NOT_FOUND);

            if(order.OrderStatus != DeliveryStatusEnum.Confirmed)
            {
                throw new InvalidDataException(ErrorMessage.BAD_REQUEST);
            }

            var token = _configuration["GHN:Token"];
            var shopId = _configuration["GHN:ShopId"];
            var url = _configuration["GHN:Url"];
            //var shopName = _configuration["Store:Name"];
            //var from_phone = _configuration["Store:PhoneNumber"];

            //var from_address = _configuration["Store:Address"];
            //var from_ward_name = _configuration["Store:WardName"];
            //var from_district_name = _configuration["Store:DistrictName"];
            //var from_provice_name = _configuration["Store:ProvinceName"];

            var receiver = order.Receiver.Split(", ").Select(e => e?.Trim()).ToArray();
            var to_name = receiver[0];
            var to_phone = receiver[1];


            if (token == null || shopId == null || url == null 
                || to_name == null || to_phone == null)
            {
                throw new ArgumentNullException(ErrorMessage.ARGUMENT_NULL);
            }

            var to_address = order.DeliveryAddress;
            var to_ward_code = order.WardID;
            var to_district_id = order.DistrictID;

            var items = order.OrderDetails.Select(e => new
            {
                name = e.ProductName,
                quantity = e.Quantity,
                price = (int)Math.Floor(e.Price)
            }).ToArray();

            var cod_amount = order.AmountPaid < order.Total ? order.Total : 0;

            var data = new
            {
                cod_amount,
                to_name,
                to_phone,
                to_address,
                to_ward_code,
                to_district_id,
                service_type_id = 2,
                payment_type_id = 1,
                weight = request.Weight,
                length = request.Length,
                width = request.Width,
                height = request.Height,
                required_note = request.RequiredNote.ToString(),
                items,
            };

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("ShopId", shopId);
            httpClient.DefaultRequestHeaders.Add("Token", token);

            var res = await httpClient.PostAsJsonAsync(url + "/create", data);
            var dataResponse = await res.Content.ReadFromJsonAsync<GHNResponse>();
            if (!res.IsSuccessStatusCode)
            {
                throw new InvalidDataException(dataResponse?.Message ?? ErrorMessage.BAD_REQUEST);
            }

            order.ShippingCode = dataResponse?.Data?.Order_code;
            order.Expected_delivery_time = dataResponse?.Data?.Expected_delivery_time;

            order.OrderStatus = DeliveryStatusEnum.AwaitingPickup;
            await _orderRepository.UpdateAsync(order);
        }
    }
}
