using Microsoft.AspNetCore.Identity;
using Webapi.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Webapi.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create database if it doesn't exist
            context.Database.EnsureCreated();

            // Create default roles
            string[] roleNames = { "Admin", "Manager", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));

                }
            }

            // Check admin user in DB
            var adminUser = await userManager.FindByNameAsync("admin");

            if (adminUser != null)
            {
                // Ensure admin has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // (Optional fallback: create one if missing)
                var newAdmin = new User
                {
                    UserName = "admin",
                    Email = "admin@example.com",
                    FirstName = "Admin",
                    LastName = "User"
                };

                var result = await userManager.CreateAsync(newAdmin, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "Admin");
                }
            }

            // Create default permissions
            if (!context.Permissions.Any())
            {
                var permissions = new List<Permission>
                {
                    new Permission { Name = "Sales_View", Description = "View Sales", Module = "Sales", HierarchyLevel = 1 },
                    new Permission { Name = "Sales_Edit", Description = "Edit Sales", Module = "Sales", HierarchyLevel = 1 },
                    new Permission { Name = "Sales_Delete", Description = "Delete Sales", Module = "Sales", HierarchyLevel = 1 },
                    new Permission { Name = "SalesTransaction_View", Description = "View Sales Transactions", Module = "Sales Transaction", HierarchyLevel = 2 },
                    new Permission { Name = "SalesTransaction_Edit", Description = "Edit Sales Transactions", Module = "Sales Transaction", HierarchyLevel = 2 },
                    new Permission { Name = "SalesTransaction_Delete", Description = "Delete Sales Transactions", Module = "Sales Transaction", HierarchyLevel = 2 },
                    new Permission { Name = "SaleInvoice_View", Description = "View Sale Invoices", Module = "Sale Invoice", HierarchyLevel = 3 },
                    new Permission { Name = "SaleInvoice_Edit", Description = "Edit Sale Invoices", Module = "Sale Invoice", HierarchyLevel = 3 },
                    new Permission { Name = "SaleInvoice_Delete", Description = "Delete Sale Invoices", Module = "Sale Invoice", HierarchyLevel = 3 },
                    new Permission { Name = "SaleInvoice_Print", Description = "Print Sale Invoices", Module = "Sale Invoice", HierarchyLevel = 3 }
                };

                await context.Permissions.AddRangeAsync(permissions);
                await context.SaveChangesAsync();
            }
        }
    }
}