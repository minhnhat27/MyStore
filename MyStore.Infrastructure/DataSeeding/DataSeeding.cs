using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyStore.Infrastructure.DbContext;
using MyStore.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using MyStore.Domain.Enumerations;

namespace MyStore.Infrastructure.DataSeeding
{
    public class DataSeeding
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                if (context != null && context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();

                    using (var transaction = await context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            await InitializeRoles(scope.ServiceProvider, context);
                            await InitializeProductAttributes(context);
                            await transaction.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await transaction.RollbackAsync();
                            throw;
                        }
                    }
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
                var result = await userManager.CreateAsync(user, "Minhnhat2702@");
                if (result.Succeeded)
                {
                    await userManager.AddToRolesAsync(user, roles);
                }
            }
        }

        private static async Task InitializeProductAttributes(MyDbContext context)
        {
            var lstOrderStatus = Enum.GetNames(typeof(DeliveryStatus))
                .Select(e => new OrderStatus
                {
                    Name = e
                });
            await context.OrderStatus.AddRangeAsync(lstOrderStatus);

            var lstPaymentMethod = Enum.GetNames(typeof(PaymentMethodEnum))
                .Select(e => new PaymentMethod
                {
                    Name = e,
                    isActive = false,
                });
            await context.PaymentMethods.AddRangeAsync(lstPaymentMethod);

            var lstSize = Enum.GetNames(typeof(SizeEnum))
                .Select(e => new Size
                {
                    Id = e,
                });
            await context.Sizes.AddRangeAsync(lstSize);
            await context.SaveChangesAsync();
        }
        
    }
}
