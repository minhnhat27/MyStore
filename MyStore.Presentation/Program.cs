using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.IRepository;
using MyStore.Application.IRepository.Caching;
using MyStore.Application.IRepository.SendMail;
using MyStore.Application.Services.Accounts;
using MyStore.Application.Services.Products;
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
builder.Services.AddDbContext<ApplicationContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), 
        m => m.MigrationsAssembly("MyStore.Infrastructure"));
});
builder.Services.AddIdentity<User, IdentityRole>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.Password.RequiredLength = 6;
}).AddEntityFrameworkStores<ApplicationContext>().AddDefaultTokenProviders();

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
builder.Services.AddSingleton<ISendMailService, SendMailService>();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICodeCaching, CodeCaching>();

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

builder.Services.AddTransient<IUserRepository, UserRespository>();
builder.Services.AddTransient<IAccountService, AccountService>();

builder.Services.AddTransient<IImageRepository, ImageRepository>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IProductService, ProductService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MyCors");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
