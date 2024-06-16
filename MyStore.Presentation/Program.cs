using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.ICaching;
using MyStore.Application.IRepository;
using MyStore.Application.ISendMail;
using MyStore.Application.Services.Accounts;
using MyStore.Application.Services.Orders;
using MyStore.Application.Services.Products;
using MyStore.Application.Services.Users;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.Caching;
using MyStore.Infrastructure.DbContext;
using MyStore.Infrastructure.Email;
using MyStore.Infrastructure.Repositories;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(opt => opt.AddPolicy("MyCors", opt =>
{
    opt.WithOrigins("http://localhost:3001").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    opt.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));
builder.Services.AddDbContext<MyDbContext>(opt =>
{
    //opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"),
    //    m => m.MigrationsAssembly("MyStore.Infrastructure"));
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SecretKey"] ?? ""))
    };
});
builder.Services.Configure<SenderSettings>(builder.Configuration.GetSection("SenderSettings"));

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ISendMailService, SendMailService>();
builder.Services.AddSingleton<ICodeCache, CodeCache>();
builder.Services.AddSingleton<ICache, Cache>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())!.FullName, "MyStore.Infrastructure", "Images")
    ),
    RequestPath = "/images"
});

app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
