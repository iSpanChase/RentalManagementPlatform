using Meilisearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs; // For MinioSettings
using RentalManagementPlatformWebAPI.Mappings;
using RentalManagementPlatformWebAPI.Middlewares;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Repositories.Bookings;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Repositories.Payments;
using RentalManagementPlatformWebAPI.Services;
using RentalManagementPlatformWebAPI.Services.Bookings;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Payments;
using System.Reflection;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// 加入 CORS 服務
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowVue", policy =>
				{
					policy.WithOrigins("http://localhost:5173",
									   "https://my-project-frontend.ngrok.app")  // <--- 將 ngrok URL 加入！
						  .AllowAnyHeader()
						 .AllowAnyMethod();
				});
			});

			// 業務資料庫連線註冊
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

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
			// Authorization�]�����U�򥻵����GAdminOnly�^
			// �ʺA�v����ĳ�אּ�ۭq IAuthorizationPolicyProvider�F�����n�b�Ұʮɳs DB�C
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
			});

			// DI�GDomain Services
			builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>,
									   Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IRoleService, RoleService>();
			builder.Services.AddScoped<IPermissionService, PermissionService>();
			builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
			builder.Services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

			// DI�GRepositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IRoleRepository, RoleRepository>();
			builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
			builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
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
            builder.Services.AddScoped<IImageUrlResolver, ImageUrlResolver>(); // Register the new ImageUrlResolver

			// Swagger�]�� Schema Id / JWT / DateOnly/TimeOnly �����^
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

			app.Run();
		}
	}
}