using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.Management.Repository;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories;
using RentalManagementPlatformMVC.Services;
using RentalManagementPlatformMVC.Areas.UserManagement.UserServices;

namespace RentalManagementPlatformMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


			// Identity 用的 Context（連線字串用 DefaultConnection）
			builder.Services.AddScoped<ICouponQueryService, CouponQueryService>();

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


			// 業務資料表用的 Context（連線字串同樣指向同一顆 DB）
			//service註冊
			builder.Services.AddScoped<ICouponQueryService, CouponQueryService>();
			builder.Services.AddScoped<CouponCommandService>();

			//repository註冊
			builder.Services.AddScoped<ICouponReadRepository, CouponReadRepository>();
			builder.Services.AddScoped<ICouponWriteRepository, CouponWriteRepository>();

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();


			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			builder.Services.AddScoped<IUserRepository, UserRepository>();

			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

			builder.Services.AddScoped<IUserService, UserService>();

			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
			builder.Services.AddScoped<IPaymentService, PaymentService>();


			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddControllersWithViews();
			//var app = builder.Build();


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
