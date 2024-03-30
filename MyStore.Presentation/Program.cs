using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyStore.Application.Services;
using MyStore.Domain.Entities;
using MyStore.Infrastructure.DbContext;
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
}).AddEntityFrameworkStores<ApplicationContext>();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:SerectKey"] ?? ""))
    };
});

builder.Services.AddScoped<IUserRepository, UserRespository>();
builder.Services.AddScoped<IAccountService, AccountService>();


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
