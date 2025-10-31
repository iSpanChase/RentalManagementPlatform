using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using RentalManagementPlatformAPI.Repository;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Data;
using RentalManagementPlatformWebAPI.DTOs; // For MinioSettings
using RentalManagementPlatformWebAPI.Hubs;
using RentalManagementPlatformWebAPI.Mappings;
using RentalManagementPlatformWebAPI.Middlewares;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
//using RentalManagementPlatformWebAPI.Repositories.Interface;
using RentalManagementPlatformWebAPI.Repositories.Bookings;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Repositories.Payments;
using RentalManagementPlatformWebAPI.Repositories.Property;
using RentalManagementPlatformWebAPI.Repositories.Property.Interfaces;
using RentalManagementPlatformWebAPI.Repository.Interfaces;
using RentalManagementPlatformWebAPI.Services;
using Meilisearch;
using Minio;
using RentalManagementPlatformWebAPI.DTOs; // For MinioSettings
using StackExchange.Redis;
using RentalManagementPlatformWebAPI.Services.Bookings;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Payments;
using RentalManagementPlatformWebAPI.Services.Property;
using RentalManagementPlatformWebAPI.Services.Property.Interfaces;
using System.Reflection;
using System.Text.Json;
using RentalManagementPlatformWebAPI.Services.Interface;

namespace RentalManagementPlatformWebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			//SignalR()
            builder.Services.AddSignalR();
            // In-Memory 暫存
            builder.Services.AddSingleton<InMemoryStore>();

            // 加入 CORS 服務
            builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowVue", policy =>
				{
					policy.WithOrigins("http://localhost:5173",
						"https://my-project-frontend.ngrok.app"); // <--- 將 ngrok URL 加入！
					policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")  // Vue 前端的網址
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

			// 業務資料庫連線註冊
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			// Redis 註冊
			// 註冊 IConnectionMultiplexer 作為單例，提供給整個應用程式共用一個 Redis 連線。
			// 這是 StackExchange.Redis 推薦的做法，用於高效地管理 Redis 連線。
			builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
			builder.Services.AddStackExchangeRedisCache(options =>
			{
				options.Configuration = builder.Configuration.GetConnectionString("Redis");
				options.InstanceName = "RentalPlatform_"; // 可選：為 Key 加上前綴，避免多個應用共用 Redis 時衝突
			});

			// Controllers + 解決JSON循環參照問題
			// JSON 設定
			builder.Services.AddControllers()
				.AddJsonOptions(options =>
				{
					// 1. 避免循環參考造成序列化錯誤
					options.JsonSerializerOptions.ReferenceHandler =
						System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;

					// 2. 忽略大小寫
					options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;

					// 3. JSON 統一用 camelCase
					options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
				});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			// Authorization ]     U 򥻵    GAdminOnly ^
			//  ʺA v    ĳ אּ ۭq IAuthorizationPolicyProvider F     n b Ұʮɳs DB C
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
			});

            // DI Message
            builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
            builder.Services.AddScoped<IMessageService, MessageService>();

            // DI�GDomain Services
            builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>,
									   Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IRoleService, RoleService>();
			builder.Services.AddScoped<IPermissionService, PermissionService>();
			builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
			builder.Services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

			// DI：Repositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IRoleRepository, RoleRepository>();
			builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
			builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

			builder.Services.AddScoped<IRoomRepository, RoomRepository>();
			builder.Services.AddScoped<IRoomListReadRepository, RoomListReadRepository>();
			builder.Services.AddScoped<IRoomListWriteRepository, RoomListWriteRepository>();
			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<ICouponRepository, CouponRepository>();
			builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
			builder.Services.AddScoped<IReviewRepository, ReviewRepository>();

			// DI：Application Services
			builder.Services.AddScoped<IRoomListQueryService, RoomListQueryService>();
			builder.Services.AddScoped<IRoomListCommandService, RoomListCommandService>();
			builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentsService, PaymentsService>();
			builder.Services.AddScoped<IReviewService, ReviewService>();

            // 註冊背景工作服務 (Hosted Service)。
            // MeilisearchIndexWorker 會在應用程式啟動時自動執行，並在背景監聽 Redis Stream 處理索引更新。
            builder.Services.AddHostedService<MeilisearchIndexWorker>();

			// External integrations
			builder.Services.AddSingleton(new MeilisearchClient(builder.Configuration["Meilisearch:Url"], builder.Configuration["Meilisearch:ApiKey"]));
			builder.Services.AddScoped<MeilisearchService>();

			// 配置 MinioSettings：將 appsettings.json 中的 "MinioSettings" 區塊綁定到 MinioSettings DTO。
			builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));
			builder.Services.AddSingleton<IMinioService, MinioService>();
			builder.Services.AddScoped<IFileUrlResolver, FileUrlResolver>();

			builder.Services.AddScoped<IRoomListReadRepository, RoomListReadRepository>();
			builder.Services.AddScoped<IRoomListWriteRepository, RoomListWriteRepository>();
			builder.Services.AddScoped<IRoomListQueryService, RoomListQueryService>();
			builder.Services.AddScoped<IRoomListCommandService, RoomListCommandService>();
			builder.Services.AddScoped<IFileUrlResolver, FileUrlResolver>();
			// Meilisearch Client and Service registration
			builder.Services.AddSingleton(new MeilisearchClient(builder.Configuration["Meilisearch:Url"], builder.Configuration["Meilisearch:ApiKey"]));
			builder.Services.AddScoped<MeilisearchService>();

			// MinIO Client and Service registration
			builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));
			builder.Services.AddSingleton<IMinioService, MinioService>();
			builder.Services.AddScoped<IFileUrlResolver, FileUrlResolver>();
			//builder.Services.AddScoped<IImageUrlResolver, ImageUrlResolver>(); // Register the new ImageUrlResolver

			// Swagger ]   Schema Id / JWT / DateOnly/TimeOnly      ^
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				// 避免因重複名稱而產生相同名稱 Schema 的問題
				c.CustomSchemaIds(t => t.FullName);

				// JWT 驗證定義
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "輸入: Bearer {your token}"
				});
				c.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "Bearer"
							}
						},
						Array.Empty<string>()
					}
				});

				// 特殊型別的 DateOnly/TimeOnly
				c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
				c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

				// 可選：自動載入 XML 註解（不存在不報錯）
				var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
				if (File.Exists(xmlPath))
				{
					c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
				}
			});

			builder.Services.AddScoped<ICouponApiService, CouponApiService>();
			builder.Services.AddScoped<CouponValidationService>();          // CouponAPI   U  Ƽh Repository
			builder.Services.AddScoped<ICouponReadRepository, CouponReadRepository>();
			builder.Services.AddScoped<ICouponDistrictReadRepository, CouponDistrictReadRepository>();
			builder.Services.AddScoped<IBookingReadRepository, BookingReadRepository>(); builder.Services.AddScoped<IUserReadRepository, UserReadRepository>();
			builder.Services.AddScoped<ICouponApiRepository, CouponApiRepository>();
			builder.Services.AddScoped<ICouponDbRepository, CouponDbRepository>();

			builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
			builder.Services.AddScoped<IPropertyService, PropertyService>();

			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<IRoomRepository, RoomRepository>();
			builder.Services.AddScoped<ICouponRepository, CouponRepository>();
			builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IRoleRepository, RoleRepository>();
			builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
			builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
			builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentsService, PaymentsService>();

			// DI：Domain Services
			builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>,
									   Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IRoleService, RoleService>();
			builder.Services.AddScoped<IPermissionService, PermissionService>();
			builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
			builder.Services.AddScoped<ECPayService>();
			builder.Services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

			builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
			builder.Services.AddProblemDetails(); // 問題詳情中介軟體

			builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
            builder.Services.AddSignalR();
            var app = builder.Build();

			app.UseExceptionHandler(); // 全域異常處理中介軟體

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseHttpsRedirection();

			app.UseCors("AllowVue");

			app.UseAuthentication();
			app.UseAuthorization();

			app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");

			//Message
            app.MapHub<ChatHub>("/hubs/chat");

            app.Run();
		}
	}
}
