using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Mappings;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories.Bookings;
using RentalManagementPlatformMVC.Repositories.Payments;
using RentalManagementPlatformMVC.Repositories.SubscriptionPlans;
using RentalManagementPlatformMVC.Services.Bookings;
using RentalManagementPlatformMVC.Services.Payments;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;
using AutoMapper;

namespace RentalManagementPlatformMVC
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
			{
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql"));
			});

			builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();
			builder.Services.AddControllersWithViews();

			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
			builder.Services.AddScoped<IPaymentService, PaymentService>();
			builder.Services.AddScoped<IHostPayoutRepository, HostPayoutRepository>();
			builder.Services.AddScoped<IHostPayoutService, HostPayoutService>();
			builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
			builder.Services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
			builder.Services.AddScoped<IHostSubscriptionRepository, HostSubscriptionRepository>();
			builder.Services.AddScoped<IHostSubscriptionService, HostSubscriptionService>();
			builder.Services.AddAutoMapper(cfg => { }, typeof(Program).Assembly);

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

			if (!app.Environment.IsDevelopment())
			{
				app.UseHttpsRedirection();
			}
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.MapControllerRoute(
				name: "areas",
				pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
			);

			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");
			app.MapRazorPages();

			app.Run();
		}
	}
}