using Meilisearch;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using RentalManagementPlatformAPI.Repository;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Area.ReportForm.Services;
using RentalManagementPlatformWebAPI.Auth;
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
using RentalManagementPlatformWebAPI.Services.Bookings;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using RentalManagementPlatformWebAPI.Services.Payments;
using RentalManagementPlatformWebAPI.Services.Property;
using RentalManagementPlatformWebAPI.Services.Property.Interfaces;
using StackExchange.Redis;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace RentalManagementPlatformWebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var cfg = builder.Configuration;

			//SignalR()
			builder.Services.AddSignalR();
            // In-Memory 暫存
            builder.Services.AddSingleton<InMemoryStore>();
            builder.Services.AddHttpClient();

			// 加入 CORS 服務
			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowVue", policy =>
				{
                    policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173", "https://my-project-frontend.ngrok.app")
						  .AllowAnyHeader()
						  .AllowAnyMethod()
                          .AllowCredentials();
				});
			});

			// 業務資料庫連線註冊
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			builder.Services.AddLogging(logging =>
			{
				logging.ClearProviders();
				logging.AddConsole();
				logging.AddDebug();
			});

			// JWT Authentication（一定要把預設方案設為 JwtBearer）
			builder.Services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(options =>
				{
					options.RequireHttpsMetadata = false; // dev 可關掉，prod 建議開
					options.SaveToken = true;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuer = true,
						ValidIssuer = builder.Configuration["Authentication:Jwt:Issuer"],
						ValidateAudience = true,
						ValidAudience = builder.Configuration["Authentication:Jwt:Audience"],
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(builder.Configuration["Authentication:Jwt:Key"]!)
						),
						ValidateLifetime = true,
						ClockSkew = TimeSpan.Zero
					};
				});

			// 2) 為了跑外部登入流程，需要一個短生命週期的 Cookie Scheme 來存 state/nonce
			builder.Services.AddAuthentication() // ← 不帶 options，避免改動 Default*
			.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, o =>
			{
				o.ExpireTimeSpan = TimeSpan.FromMinutes(10);
				o.SlidingExpiration = false;
				o.Cookie.HttpOnly = true;
				o.Cookie.SameSite = SameSiteMode.None;          // ← 如需更保險
				o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
			});

			// 3) Google（使用內建 Handler）
			builder.Services.AddAuthentication()
				.AddGoogle("Google", opt =>
				{
					opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
					opt.ClientId = cfg["Authentication:Google:ClientId"]!;
					opt.ClientSecret = cfg["Authentication:Google:ClientSecret"]!;
					opt.CallbackPath = cfg["Authentication:Google:CallbackPath"]; // e.g. /api/auth/oauth/google/callback
					opt.Scope.Add("email");
					opt.Scope.Add("profile");
					opt.SaveTokens = true;
					opt.Events = new OAuthEvents
					{
						OnCreatingTicket = async ctx =>
						{
							// 這裡可讀 ctx.Identity/ctx.AccessToken 等
							await Task.CompletedTask;
						}
					};
				});

			// 4) LINE（改用 OIDC userinfo 以取得 email）
			builder.Services.AddAuthentication()
				.AddOAuth("LINE", opt =>
				{
					opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

					opt.ClientId = cfg["Authentication:Line:ChannelId"]!;
					opt.ClientSecret = cfg["Authentication:Line:ChannelSecret"]!;
					opt.CallbackPath = cfg["Authentication:Line:CallbackPath"]; // /api/auth/oauth/line/callback

					opt.AuthorizationEndpoint = "https://access.line.me/oauth2/v2.1/authorize";
					opt.TokenEndpoint = "https://api.line.me/oauth2/v2.1/token";
					// OIDC userinfo：只有這裡才可能帶 email（前提：scope 有 email、使用者 email 已驗證）
					opt.UserInformationEndpoint = "https://api.line.me/oauth2/v2.1/userinfo";

					// 必要 scopes
					opt.Scope.Clear();
					opt.Scope.Add("openid");
					opt.Scope.Add("profile");
					opt.Scope.Add("email");

					opt.SaveTokens = true;

					// 先清空，重新映射 userinfo 欄位
					opt.ClaimActions.Clear();
					opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "sub");  // 唯一識別
					opt.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");          // 表準 email claim
					opt.ClaimActions.MapJsonKey("email", "email");                   // 另外留一份原始 "email"
					opt.ClaimActions.MapJsonKey("email_verified", "email_verified");
					opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
					opt.ClaimActions.MapJsonKey("picture", "picture");

					opt.Events = new OAuthEvents
					{
						// 強制每次詢問同意，避免沿用舊的（沒有勾 email）
						OnRedirectToAuthorizationEndpoint = ctx =>
						{
							var sep = ctx.RedirectUri.Contains('?') ? '&' : '?';
							ctx.Response.Redirect($"{ctx.RedirectUri}{sep}prompt=consent&max_age=0");
							return Task.CompletedTask;
						},

						// 若第三方回傳錯誤，把原因打出來
						OnRemoteFailure = ctx =>
						{
							var logger = ctx.HttpContext.RequestServices
								.GetRequiredService<ILoggerFactory>().CreateLogger("LINE-Debug");
							logger.LogError(ctx.Failure, "LINE remote failure: {Message}", ctx.Failure?.Message);
							return Task.CompletedTask;
						},

						OnCreatingTicket = async ctx =>
						{
							var logger = ctx.HttpContext.RequestServices
								.GetRequiredService<ILoggerFactory>().CreateLogger("LINE-Debug");

							// 1) 以 access_token 要 OIDC userinfo
							using (var req1 = new HttpRequestMessage(HttpMethod.Get, opt.UserInformationEndpoint))
							{
								req1.Headers.Authorization =
									new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);

								using var resp1 = await ctx.Backchannel.SendAsync(req1);
								var body1 = await resp1.Content.ReadAsStringAsync();
								logger.LogInformation("userinfo {status}: {body}", (int)resp1.StatusCode, body1);
								resp1.EnsureSuccessStatusCode();

								using var doc1 = System.Text.Json.JsonDocument.Parse(body1);
								ctx.RunClaimActions(doc1.RootElement); // 把 sub/email/name/picture 寫進 claims（若有）
							}

							// 2) 再補讀 profile（常見只有 displayName、pictureUrl，沒有 email）
							using (var req2 = new HttpRequestMessage(HttpMethod.Get, "https://api.line.me/v2/profile"))
							{
								req2.Headers.Authorization =
									new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AccessToken);

								using var resp2 = await ctx.Backchannel.SendAsync(req2);
								var body2 = await resp2.Content.ReadAsStringAsync();
								logger.LogInformation("profile {status}: {body}", (int)resp2.StatusCode, body2);

								if (resp2.IsSuccessStatusCode)
								{
									using var doc2 = System.Text.Json.JsonDocument.Parse(body2);
									var root = doc2.RootElement;

									if (root.TryGetProperty("displayName", out var n) && !string.IsNullOrWhiteSpace(n.GetString()))
										ctx.Identity!.AddClaim(new Claim(ClaimTypes.Name, n.GetString()!));
									if (root.TryGetProperty("pictureUrl", out var p) && !string.IsNullOrWhiteSpace(p.GetString()))
										ctx.Identity!.AddClaim(new Claim("picture", p.GetString()!));
								}
							}

							// 3) 最終 claims 一覽
							foreach (var c in ctx.Identity!.Claims)
								logger.LogInformation("claim {type}={value}", c.Type, c.Value);
						}
					};
				});


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
			  });            // DI Message
            builder.Services.AddScoped<ISupportTicketService, SupportTicketService>();
            builder.Services.AddScoped<IMessageService, MessageService>();

            // DI�GDomain Services
			//用戶個人推薦房源算法
            builder.Services.AddScoped<RecommendationService>();

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

			builder.Services.Configure<SmtpOptions>(
			builder.Configuration.GetSection("Email:Smtp"));
			// DI：Email Sender（SmtpEmailSender）
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
			builder.Services.AddScoped<IRoomRepository, RoomRepository>();
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
			// Meilisearch Client and Service registration
			builder.Services.AddSingleton(new MeilisearchClient(builder.Configuration["Meilisearch:Url"], builder.Configuration["Meilisearch:ApiKey"]));
			builder.Services.AddScoped<MeilisearchService>();

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

			// DI：Domain Services
			builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>,
									   Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
			builder.Services.AddScoped<ECPayService>();

			builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
			builder.Services.AddProblemDetails(); // 問題詳情中介軟體

			            builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);
			            builder.Services.AddSignalR();
			
						// Configure Forwarded Headers for reverse proxy
						builder.Services.Configure<ForwardedHeadersOptions>(options =>
						{
							options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
							options.KnownNetworks.Clear();
							options.KnownProxies.Clear();
						});
			
			            var app = builder.Build();
			
						// Use Forwarded Headers - must be one of the first middleware
						app.UseForwardedHeaders();
			
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
			app.UseStaticFiles();

			app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");

			//Message
            app.MapHub<ChatHub>("/hubs/chat");

			app.MapGet("/api/auth/oauth/{provider}/challenge", async (HttpContext ctx, string provider) =>
			{
				// provider → scheme
				string scheme = provider?.ToLowerInvariant() switch
				{
					"google" => "Google",
					"line" => "LINE",
					_ => null
				};
				if (scheme is null) { ctx.Response.StatusCode = 400; await ctx.Response.WriteAsync("unknown provider"); return; }

				// returnUrl（防空、防不合法）
				var raw = ctx.Request.Query["returnUrl"].FirstOrDefault();
				var returnUrl = string.IsNullOrWhiteSpace(raw) ? "/auth/callback" : raw.Trim();
				if (!returnUrl.StartsWith("/")) returnUrl = "/" + returnUrl; // 避免 open redirect
				var encoded = System.Net.WebUtility.UrlEncode(returnUrl);

				// ※ 建議導向 finish（由 finish 簽 JWT，再導回前端）
				var props = new AuthenticationProperties
				{
					RedirectUri = $"/api/auth/oauth/finish?provider={provider}&returnUrl={encoded}"
				};
				props.Items["returnUrl"] = returnUrl; // ★ 補這行

				await ctx.ChallengeAsync(scheme, props);
			});

			app.MapGet("/api/auth/oauth/{provider}/callback", async (
				HttpContext http,
				string provider,
				IConfiguration cfg,
				IUserService users,
				IJwtTokenService jwt
			) =>
			{
				var authResult = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
				if (!authResult.Succeeded || authResult.Principal is null)
					return Results.Redirect(cfg["Authentication:FrontendFailUrl"]!);

				var p = authResult.Principal;
				var profile = new ExternalProfileDto
				{
					Provider = provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google" : "LINE",
					ProviderUserId = p.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
					Email = p.FindFirstValue(ClaimTypes.Email),
					DisplayName = p.Identity?.Name,
					PictureUrl = p.FindFirst("picture")?.Value
				};

				var user = await users.FindOrCreateFromExternalAsync(profile);

				// ★ 用你已實作的 IssueTokenAsync 產 JWT
				var token = await jwt.IssueTokenAsync(user);

				await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

				var returnUrl = authResult.Properties?.Items.TryGetValue("returnUrl", out var r) == true ? r : "/";
				var okUrl = $"{cfg["Authentication:FrontendSuccessUrl"]}?token={Uri.EscapeDataString(token)}&returnUrl={Uri.EscapeDataString(returnUrl!)}";
				return Results.Redirect(okUrl);
			});

			app.MapGet("/api/auth/oauth/finish", async (
				HttpContext http,
				string provider,
				IConfiguration cfg,
				IUserService users,
				IJwtTokenService jwt
			) =>
			{
				try
				{
					var authResult = await http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
					if (!authResult.Succeeded || authResult.Principal is null)
					{
						var fail = cfg["Authentication:FrontendFailUrl"] ?? "http://localhost:5173/auth/callback";
						return Results.Redirect(QueryHelpers.AddQueryString(fail, "error", "no_external_principal"));
					}

					var p = authResult.Principal;

					// ← 雙保險：先拿標準 Email，再拿原始 "email"
					var email =
						p.FindFirstValue(ClaimTypes.Email) ??
						p.FindFirst("email")?.Value;

					var profile = new ExternalProfileDto
					{
						Provider = provider.Equals("google", StringComparison.OrdinalIgnoreCase) ? "Google" : "LINE",
						ProviderUserId = p.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
						Email = email,
						DisplayName = p.Identity?.Name,
						PictureUrl = p.FindFirst("picture")?.Value
					};

					var user = await users.FindOrCreateFromExternalAsync(profile);
					var access = await jwt.IssueTokenAsync(user);
					if (string.IsNullOrWhiteSpace(access))
					{
						var fail = cfg["Authentication:FrontendFailUrl"] ?? "http://localhost:5173/auth/callback";
						return Results.Redirect(QueryHelpers.AddQueryString(fail, "error", "no_access_token"));
					}

					await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

					// 僅允許相對路徑，避免 open redirect
					var returnUrl = http.Request.Query["returnUrl"].FirstOrDefault();
					if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith("/")) returnUrl = "/";

					var okBase = cfg["Authentication:FrontendSuccessUrl"] ?? "http://localhost:5173/auth/callback";
					var redirect = QueryHelpers.AddQueryString(okBase, new Dictionary<string, string?>
					{
						["access"] = access,
						["refresh"] = "",
						["redirect"] = returnUrl
					});

					return Results.Redirect(redirect);
				}
				catch (Exception ex)
				{
					var fail = cfg["Authentication:FrontendFailUrl"] ?? "http://localhost:5173/auth/callback";
					var url = QueryHelpers.AddQueryString(fail, "error", ex.Message);
					return Results.Redirect(url);
				}
			});




			app.Run();
		}
	}
}
