using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RentalManagementPlatformWebAPI.Auth;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Services;
using System.Reflection;
using System.Text;

namespace RentalManagementPlatformWebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// DbContext
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			// CORS（依實際前端網域調整）
			builder.Services.AddCors(opt =>
			{
				opt.AddPolicy("spa", p => p
					.WithOrigins("http://localhost:5173")
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials()
				);
			});

			// JWT Authentication
			var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key");
			builder.Services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(o =>
				{
					o.TokenValidationParameters = new()
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidIssuer = builder.Configuration["Jwt:Issuer"],
						ValidAudience = builder.Configuration["Jwt:Audience"],
						ClockSkew = TimeSpan.Zero
					};
				});

			// Controllers + JSON（避免雙向導航導致循環參考把 Swagger 炸掉）
			builder.Services.AddControllers()
				.AddJsonOptions(opt =>
				{
					opt.JsonSerializerOptions.ReferenceHandler =
						System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
				});

			// Authorization（先註冊基本策略：AdminOnly）
			// 動態權限建議改為自訂 IAuthorizationPolicyProvider；先不要在啟動時連 DB。
			builder.Services.AddAuthorization(options =>
			{
				options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
			});

			// DI：Domain Services
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

			// DI：Email Sender（SmtpEmailSender）
			builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Email:Smtp"));
			builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
			//builder.Services.AddScoped<IEmailSender, EmailSender>();
			builder.Services.Configure<EmailVerificationOptions>(
				builder.Configuration.GetSection("EmailVerification"));
			builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

			// DI：Auth Claims Transformation & Policy Provider
			builder.Services.AddMemoryCache();
			builder.Services.AddScoped<IClaimsTransformation, PermissionClaimsTransformation>();
			builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

			// Swagger（補 Schema Id / JWT / DateOnly/TimeOnly 對應）
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "RentalManagementPlatformWebAPI", Version = "v1" });

				// 避免不同命名空間同名類別造成 Schema 衝突
				c.CustomSchemaIds(t => t.FullName);

				// JWT 安全定義
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

				// 若專案使用 DateOnly/TimeOnly
				c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
				c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

				// 可選：自動載入 XML 註解（存在才載）
				var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
				if (File.Exists(xmlPath))
				{
					c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
				}
			});

			var app = builder.Build();

			// 在開發環境顯示完整例外頁，方便看到 /swagger/v1/swagger.json 的堆疊
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// 建議先不分環境都開 Swagger（等修好再改回只在 Dev 開）
			app.UseSwagger();
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "RentalManagementPlatformWebAPI v1");
				c.RoutePrefix = "swagger";
			});

			app.UseHttpsRedirection();
			app.UseCors("spa");

			app.UseAuthentication();
			app.UseAuthorization();
			app.UseStaticFiles();

			app.MapControllers();

			app.Run();
		}
	}
}
