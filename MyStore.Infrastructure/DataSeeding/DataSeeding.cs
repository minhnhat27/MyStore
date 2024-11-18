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
                try
                {
                    if (context.Database.GetPendingMigrations().Any())
                    {
                        context.Database.Migrate();
                    }
                    await InitializeProductAttributes(context);
                    await InitializeRoles(scope.ServiceProvider, context);
                }
                catch
                {
                    throw;
                }
            }
        }
        private static async Task InitializeRoles(IServiceProvider serviceProvider, MyDbContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            string[] roles = Enum.GetNames(typeof(RolesEnum));
            foreach (string role in roles)
            {
                if (!context.Roles.Any(r => r.Name == role))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }
            await InitializeUsers(serviceProvider, context, roles);
        }
        private static async Task InitializeUsers(IServiceProvider serviceProvider, MyDbContext context, string[] roles)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            var email = "minhnhat012340@gmail.com";
            var user = new User
            {
                Fullname = "Minh Nhật",
                Email = email,
                NormalizedEmail = email.ToUpper(),
                UserName = email,
                NormalizedUserName = email.ToUpper(),
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
                    await userManager.AddToRoleAsync(user, RolesEnum.Admin.ToString());
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

            if (!context.Brands.Any())
            {
                await context.Brands.AddAsync(new Brand
                {
                    Name = NoBrandEnum.NO_BRAND.ToString(),
                    ImageUrl = ""
                });
            }

            if (!context.PaymentMethods.Any() || !context.Sizes.Any() || !context.Brands.Any())
            {
                await context.SaveChangesAsync();
            }
        }
    }
}
