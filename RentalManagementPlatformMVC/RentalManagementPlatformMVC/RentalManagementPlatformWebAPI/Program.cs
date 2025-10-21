
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Services;
using System;
using System.Text;

namespace RentalManagementPlatformWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

			// Identity 用的 Context（連線字串用 DefaultConnection）
			// builder.Services.AddScoped<ICouponQueryService, CouponQueryService>();

			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			// CORS（把前端網址加進來）
			builder.Services.AddCors(opt =>
			{
				opt.AddPolicy("spa", p => p
					.WithOrigins("http://localhost:5173") // 你的 Vue 域名/埠
					.AllowAnyHeader()
					.AllowAnyMethod()
					.AllowCredentials()
				);
			});

			// JWT Auth
			var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Missing Jwt:Key"); ;
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

			//builder.Services.AddAuthorization(options =>
			//{
			//	// 基於 Permission 的 policy（啟動後可從 DB 動態載入；此處先寫死最常用）
			//	foreach (var code in new[] { "Roles.View", "Roles.Assign", "Permissions.View", "Permissions.Assign" })
			//		options.AddPolicy(code, p => p.RequireClaim("perm", code));

			//	options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
			//});

			// 先建立暫時的 ServiceProvider 讀一次 DB 的所有 PermCode
			using (var tmp = builder.Services.BuildServiceProvider())
			using (var scope = tmp.CreateScope())
			{
				var db = scope.ServiceProvider.GetRequiredService<RentalManagementPlatformSqlContext>();
				var codes = db.Permissions.Select(p => p.PermCode).Distinct().ToList();

				builder.Services.AddAuthorization(options =>
				{
					foreach (var code in codes)
						options.AddPolicy(code, p => p.RequireClaim("perm", code));
					options.AddPolicy("AdminOnly", p => p.RequireRole("ADMIN"));
				});
			}

			// DI
			// 密碼雜湊（使用 ASP.NET Core 內建的 PasswordHasher<User>）
			builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<RentalManagementPlatformWebAPI.Models.User>,
									   Microsoft.AspNetCore.Identity.PasswordHasher<RentalManagementPlatformWebAPI.Models.User>>();

			// Domain Services
			builder.Services.AddScoped<IAuthService, AuthService>();
			builder.Services.AddScoped<IUserService, UserService>();
			builder.Services.AddScoped<IRoleService, RoleService>();
			builder.Services.AddScoped<IPermissionService, PermissionService>();
			builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
			builder.Services.AddSingleton<IGoogleTokenVerifier, GoogleTokenVerifier>();

			// Repositories
			builder.Services.AddScoped<IUserRepository, UserRepository>();
			builder.Services.AddScoped<IRoleRepository, RoleRepository>();
			builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
			builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

			// Add services to the container.

			builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new() { Title = "RentalManagementPlatformWebAPI", Version = "v1" });
				c.AddSecurityDefinition("Bearer", new()
				{
					Name = "Authorization",
					Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					In = Microsoft.OpenApi.Models.ParameterLocation.Header,
					Description = "在此輸入: Bearer {your token}"
				});
				c.AddSecurityRequirement(new()
				{
					{
						new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
						Array.Empty<string>()
					}
				});
			});

			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
			app.UseCors("spa");
			app.UseAuthentication();
			app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
