using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Infrastructure.DbContext;
using MyStore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using MyStore.Domain.Enumerations;
using System.Data;

namespace MyStore.Infrastructure.DataSeeding
{
    public class DataSeeding
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            if(context != null)
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                    await InitializeProductAttributes(context);
                    await InitializeRoles(scope.ServiceProvider, context);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        private static async Task InitializeRoles(IServiceProvider serviceProvider, MyDbContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Admin", "User", "Owner", "Employee" };

            foreach (string role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
            await InitializeUsers(serviceProvider, context, roles);
        }
        private static async Task InitializeUsers(IServiceProvider serviceProvider, MyDbContext context, string[] roles)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var user = new User
            {
                Fullname = "Minh Nhật",
                Email = "minhnhat012340@gmail.com",
                NormalizedEmail = "minhnhat012340@gmail.com",
                UserName = "minhnhat012340@gmail.com",
                NormalizedUserName = "minhnhat012340@gmail.com",
                PhoneNumber = "0358103707",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
            };

            if (!context.Users.Any(u => u.UserName == user.UserName))
            {
                var result = await userManager.CreateAsync(user, "Minhnhat2702");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }
        private static async Task InitializeProductAttributes(MyDbContext context)
        {
            if (!context.PaymentMethods.Any())
            {
                var lstPaymentMethod = Enum.GetNames(typeof(PaymentMethodEnum))
                    .Select(name => new PaymentMethod { Name = name, IsActive = false });

                await context.PaymentMethods.AddRangeAsync(lstPaymentMethod);
            };

            if (!context.Sizes.Any())
            {
                var lstSize = Enum.GetNames(typeof(SizeEnum))
                    .Select(name => new Size { Name = name });

                await context.Sizes.AddRangeAsync(lstSize);
            };

            if(!context.PaymentMethods.Any() || !context.Sizes.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
