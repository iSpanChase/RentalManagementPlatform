using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services;
using Meilisearch;
using Minio;
using RentalManagementPlatformWebAPI.DTOs; // For MinioSettings
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

            // Meilisearch Configuration
            builder.Services.AddSingleton<MeilisearchClient>(provider =>
            {
                var meilisearchHost = builder.Configuration["Meilisearch:url"] ?? throw new InvalidOperationException("Missing Meilisearch:Host");
                var meilisearchApiKey = builder.Configuration["Meilisearch:ApiKey"] ?? throw new InvalidOperationException("Missing Meilisearch:ApiKey");
                return new MeilisearchClient(meilisearchHost, meilisearchApiKey);
            });

            // Minio Configuration
            builder.Services.Configure<MinioSettings>(builder.Configuration.GetSection("MinioSettings"));
            builder.Services.AddSingleton<IMinioService, MinioService>();
            builder.Services.AddScoped<IFileUrlResolver, FileUrlResolver>();
            builder.Services.AddScoped<IImageUrlResolver, ImageUrlResolver>(); // Register the new ImageUrlResolver

			// CORS�]�̹�ګe�ݺ���վ�^
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

			// Controllers + JSON�]�קK���V�ɯ�ɭP�`���Ѧҧ� Swagger �����^
			builder.Services.AddControllers()
				.AddJsonOptions(opt =>
				{
					opt.JsonSerializerOptions.ReferenceHandler =
						System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
				});

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
            builder.Services.AddScoped<MeilisearchService>();
            builder.Services.AddSingleton<IMinioService, MinioService>();

			// Swagger�]�� Schema Id / JWT / DateOnly/TimeOnly �����^
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "RentalManagementPlatformWebAPI", Version = "v1" });

				// �קK���P�R�W�Ŷ��P�W���O�y�� Schema �Ĭ�
				c.CustomSchemaIds(t => t.FullName);

				// JWT �w���w�q
				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "��J: Bearer {your token}"
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

				// �Y�M�רϥ� DateOnly/TimeOnly
				c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
				c.MapType<TimeOnly>(() => new OpenApiSchema { Type = "string", Format = "time" });

				// �i��G�۰ʸ��J XML ���ѡ]�s�b�~���^
				var xml = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xml);
				if (File.Exists(xmlPath))
				{
					c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
				}
			});

			var app = builder.Build();

			// �b�}�o������ܧ���ҥ~���A��K�ݨ� /swagger/v1/swagger.json �����|
			if (app.Environment.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// ��ĳ���������ҳ��} Swagger�]���צn�A��^�u�b Dev �}�^
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

			app.MapControllers();

			app.Run();
		}
	}
}
