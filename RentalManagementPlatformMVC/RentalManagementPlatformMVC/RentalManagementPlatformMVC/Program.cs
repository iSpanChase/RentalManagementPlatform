using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			//         var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
			//         builder.Services.AddDbContext<ApplicationDbContext>(options =>
			//             options.UseSqlServer(connectionString));

			//builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
			//{
			//	options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql"));
			//});

			// Identity 用的 Context（連線字串用 DefaultConnection）
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			// 業務資料表用的 Context（連線字串同樣指向同一顆 DB）
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSQL")));

			builder.Services.AddScoped<RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories.IUnitOfWork, 
                                       RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories.UnitOfWork>();

			builder.Services.AddScoped<RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories.IUserRepository,
                                       RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories.UserRepository>();

			builder.Services.AddScoped<RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories.IUserService,
									   RentalManagementPlatformMVC.Areas.UserManagement.UserServices.UserService>();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

			app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapRazorPages();

            app.Run();
        }
    }
}
