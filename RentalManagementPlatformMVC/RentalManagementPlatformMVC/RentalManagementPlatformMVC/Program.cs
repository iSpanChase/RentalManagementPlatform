using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.Auth.Data;
using RentalManagementPlatformMVC.Areas.Auth.Repositories;
using RentalManagementPlatformMVC.Areas.Auth.Services;
using RentalManagementPlatformMVC.Areas.Management.Repository;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Areas.Roles.RolesRepositories;
using RentalManagementPlatformMVC.Areas.Roles.RolesServices;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.UserServices;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Services.Interfaces;
using RentalManagementPlatformMVC.Services;
using RentalManagementPlatformMVC.Areas.UserManagement.UserServices;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Areas.Room_List.Services;
using Meilisearch;

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
			builder.Services.AddScoped<ICouponGuestQueryService, CouponGuestQueryService>();
			builder.Services.AddScoped<IPostQueryService, PostQueryService>();
			builder.Services.AddScoped<CouponCommandService>();
			builder.Services.AddScoped<CouponGrantService>();
			builder.Services.AddScoped<CouponDropdownService>();
			builder.Services.AddScoped<UserDropdownService>();

			//repository註冊
			builder.Services.AddScoped<ICouponReadRepository, CouponReadRepository>();
			builder.Services.AddScoped<ICouponWriteRepository, CouponWriteRepository>();
			builder.Services.AddScoped<ICouponGuestRepository, CouponGuestRepository>();
			builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
			builder.Services.AddScoped<IPostRepository, PostRepository>();
			builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();

			// Meilisearch Client and Service registration
			builder.Services.AddSingleton(new MeilisearchClient(builder.Configuration["Meilisearch:Url"], builder.Configuration["Meilisearch:ApiKey"]));
			builder.Services.AddScoped<MeilisearchService>();
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();


			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped<IUserService, UserService>();

			builder.Services.AddScoped<IRolesRepository, RolesRepository>();   // �M�� Roles Repo :contentReference[oaicite:24]{index=24} :contentReference[oaicite:25]{index=25}
			builder.Services.AddScoped<IRolesService, RolesService>();         // �s�W�� Service

			builder.Services.AddScoped<IAuthService, AuthService>();

			builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
			builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();

			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
			builder.Services.AddScoped<IPaymentService, PaymentService>();
			builder.Services.AddScoped<IHostPayoutRepository, HostPayoutRepository>();
			builder.Services.AddScoped<IHostPayoutService, HostPayoutService>();


			builder.Services.AddScoped<IRoomListReadRepository, RoomListReadRepository>();
			builder.Services.AddScoped<IRoomListWriteRepository, RoomListWriteRepository>();

			builder.Services.AddScoped<IRoomListQueryService, RoomListQueryService>();
			builder.Services.AddScoped<IRoomListCommandService, RoomListCommandService>();


			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddControllersWithViews();
			//var app = builder.Build();


            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddControllersWithViews();

			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(o =>
	{
		o.LoginPath = "/Auth/Login";
		o.LogoutPath = "/Auth/Logout";
		o.AccessDeniedPath = "/Auth/AccessDenied";
		o.SlidingExpiration = true;
		o.ExpireTimeSpan = TimeSpan.FromHours(8);
		// o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // �G�p https �ɫ�ĳ�}
	});


			builder.Services.AddAuthorization(); // �u�����v����������A��

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

			app.UseAuthentication();
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
