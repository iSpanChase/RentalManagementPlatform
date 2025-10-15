using Meilisearch;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Auth.Data;
using RentalManagementPlatformMVC.Areas.Auth.Repositories;
using RentalManagementPlatformMVC.Areas.Auth.Services;
using RentalManagementPlatformMVC.Areas.Management.Repository;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Areas.Permissions.Models;
using RentalManagementPlatformMVC.Areas.Permissions.PermissionsRepositories;
using RentalManagementPlatformMVC.Areas.Permissions.Services;
using RentalManagementPlatformMVC.Areas.Roles.RolesRepositories;
using RentalManagementPlatformMVC.Areas.Roles.RolesServices;
using RentalManagementPlatformMVC.Areas.Room_List.Services;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.UserServices;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Data;
using RentalManagementPlatformMVC.Mappings;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories;
using RentalManagementPlatformMVC.Repositories.Interfaces;
using RentalManagementPlatformMVC.Services;
using RentalManagementPlatformMVC.Services.Interfaces;
using System;
using RentalManagementPlatformMVC.Repositories.PointRules;
using RentalManagementPlatformMVC.Repositories.SubscriptionPlans;
using RentalManagementPlatformMVC.Repositories.Payments;
using RentalManagementPlatformMVC.Repositories.Bookings;
using RentalManagementPlatformMVC.Services.PointRules;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;
using RentalManagementPlatformMVC.Services.Payments;
using RentalManagementPlatformMVC.Services.Bookings;
using RentalManagementPlatformMVC.DTOs;
using Minio;
using RentalManagementPlatformMVC.Areas.ReportForm.Anomaly;

namespace RentalManagementPlatformMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


			// Identity 用的 Context（連線字串用 DefaultConnection）
			// builder.Services.AddScoped<ICouponQueryService, CouponQueryService>();

			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddSingleton<AnomalyNotifier>();
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

            // MinIO Client and Service registration
            builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));
            builder.Services.AddSingleton<IMinioService, MinioService>();
            builder.Services.AddScoped<IFileUrlResolver, FileUrlResolver>();
            builder.Services.AddScoped<IImageUrlResolver, ImageUrlResolver>(); // Register the new ImageUrlResolver


			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			//註冊Context類別，並給予對應資料庫的連線方式
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
			builder.Services.AddScoped<IUserService, UserService>();

			// 泛型 Repo 註冊
			builder.Services.AddScoped<IRepository<UserRole>, EfRepository<UserRole>>();
			builder.Services.AddScoped<IRepository<Role>, EfRepository<Role>>();

			builder.Services.AddScoped<IRolesRepository, RolesRepository>();   
			builder.Services.AddScoped<IRolesService, RolesService>();         

			builder.Services.AddScoped<IAuthService, AuthService>();

			builder.Services.AddScoped<IPermissionsService, PermissionsService>();
			builder.Services.AddScoped<IPermissionsRepository, PermissionsRepository>();

			builder.Services.AddHttpContextAccessor();

			builder.Services.AddAuthorization(options =>
			{
				// 將 AppPermissions.AllCodes() 中的每一個 code 都註冊成 Policy
				foreach (var code in AppPermissions.AllCodes())
				{
					options.AddPolicy(code, p => p.Requirements.Add(new PermissionRequirement(code)));
				}
			});

			// 註冊授權處理器
			builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

			builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
			builder.Services.AddScoped<IEmailSender, MailKitEmailSender>();
			builder.Services.AddSession();

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
			builder.Services.AddScoped<IPointRuleRepository, PointRuleRepository>();
			builder.Services.AddScoped<IPointRuleService, PointRuleService>();
			builder.Services.AddScoped<IPointLedgerRepository, PointLedgerRepository>();
			builder.Services.AddScoped<IPointLedgerService, PointLedgerService>();

            builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);


			builder.Services.AddScoped<IRoomListReadRepository, RoomListReadRepository>();
			builder.Services.AddScoped<IRoomListWriteRepository, RoomListWriteRepository>();

			builder.Services.AddScoped<IRoomListQueryService, RoomListQueryService>();
			builder.Services.AddScoped<IRoomListCommandService, RoomListCommandService>();

            builder.Services.AddScoped<IAnomalyEvaluator, AnomalyEvaluator>();
            builder.Services.AddHostedService<AnomalyBackgroundService>();

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			builder.Services.AddControllersWithViews();
			//var app = builder.Build();


            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

			builder.Services.AddControllersWithViews();

			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(o =>
	{
		o.LoginPath = "/Auth/Auth/Login";
		o.LogoutPath = "/Auth/Auth/Logout";
		o.AccessDeniedPath = "/Auth/Auth/AccessDenied";
		o.SlidingExpiration = true;
		o.ExpireTimeSpan = TimeSpan.FromHours(8);
		// o.Cookie.HttpOnly = true; o.Cookie.SecurePolicy = CookieSecurePolicy.Always; // �G�p https �ɫ�ĳ�}
	});


			builder.Services.AddAuthorization(); // �u�����v����������A��

			var app = builder.Build();

			using (var scope = app.Services.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<RentalManagementPlatformSqlContext>();
				PermissionSeeder.SeedAsync(db).GetAwaiter().GetResult();
			}

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
			app.UseSession();
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
