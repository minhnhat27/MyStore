using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.ICaching;
using MyStore.Application.ILibrary;
using MyStore.Application.IRepositories;
using MyStore.Application.IRepositories.Orders;
using MyStore.Application.IRepositories.Products;
using MyStore.Application.IRepositories.Users;
using MyStore.Application.ISendMail;
using MyStore.Application.IStorage;
using MyStore.Application.Services;
using MyStore.Application.Services.Brands;
using MyStore.Application.Services.Carts;
using MyStore.Application.Services.Categories;
using MyStore.Application.Services.Materials;
using MyStore.Application.Services.Orders;
using MyStore.Application.Services.Payments;
using MyStore.Application.Services.Products;
using MyStore.Application.Services.Sizes;
using MyStore.Application.Services.Statistics;
using MyStore.Application.Services.Users;
using MyStore.Application.Services.Vouchers;
using MyStore.Domain.Constants;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.AuthenticationService;
using MyStore.Infrastructure.Caching;
using MyStore.Infrastructure.DataSeeding;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.Email;
using MyStore.Infrastructure.Library;
using MyStore.Infrastructure.Mapping;
using MyStore.Infrastructure.Repositories;
using MyStore.Infrastructure.Repositories.Orders;
using MyStore.Infrastructure.Repositories.Products;
using MyStore.Infrastructure.Repositories.Users;
using MyStore.Infrastructure.Storage;
using MyStore.Presentation.Hubs;
using MyStore.Presentation.Hubs.Message;
using Net.payOS;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt => opt.AddPolicy("MyCors", opt =>
{
    opt.WithOrigins("http://localhost:3000", "https://localhost:3000", "http://localhost:3001")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
}));
builder.Services.AddDbContext<MyDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSqlConnection"),
        m => m.MigrationsAssembly("MyStore.Infrastructure"));
});

builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
    opt.User.RequireUniqueEmail = true;
}).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? "")),
        ClockSkew = TimeSpan.Zero
    };
    opt.Events = new JwtBearerEvents()
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAutoMapper(typeof(Mapping));

builder.Services.Configure<SenderSettings>(builder.Configuration.GetSection("SenderSettings"));

builder.Services.AddSignalR();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISendMailService, SendMailService>();
builder.Services.AddSingleton<ICache, Cache>();
builder.Services.AddSingleton<IMessageManager, MessageManager>();

PayOS payOS = new(builder.Configuration["PayOS:clientId"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL),
                        builder.Configuration["PayOS:apiKey"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL),
                        builder.Configuration["PayOS:checksumKey"] ?? throw new Exception(ErrorMessage.ARGUMENT_NULL));
builder.Services.AddSingleton(payOS);

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

builder.Services.AddScoped<IFileStorage, FileStorage>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<ISizeRepository, SizeRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICartItemRepository, CartItemRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IPaymentMethodRepository, PaymentMethodRepository>();
builder.Services.AddScoped<IProductMaterialRepository, ProductMaterialRepository>();
builder.Services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
builder.Services.AddScoped<IProductSizeRepository, ProductSizeRepository>();
builder.Services.AddScoped<IProductColorRepository, ProductColorRepository>();
builder.Services.AddScoped<IProductFavoriteRepository, ProductFavoriteRepository>();
builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
builder.Services.AddScoped<IVoucherRepository, VoucherRepository>();
builder.Services.AddScoped<IUserVoucherRepository, UserVoucherRepository>();
builder.Services.AddScoped<IDeliveryAddressRepository, DeliveryAddressRepository>();

builder.Services.AddScoped<IBrandService, BrandService>();
builder.Services.AddScoped<ISizeService, SizeService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMaterialService, MaterialService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<IVNPayLibrary, VNPayLibrary>();

var app = builder.Build();

DataSeeding.Initialize(app.Services).Wait();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseDefaultFiles();

app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<ChatBox>("/chat");

app.MapControllers();

app.Run();
