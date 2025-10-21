using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Mappings;
using RentalManagementPlatformWebAPI.Middlewares;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Repositories.Interface;
using RentalManagementPlatformWebAPI.Services;
using RentalManagementPlatformWebAPI.Services.Interface;

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
					policy.WithOrigins("http://localhost:5173")  // Vue 前端的網址
						  .AllowAnyHeader()
						  .AllowAnyMethod();
				});
			});

			// 業務資料庫連線註冊
			builder.Services.AddDbContext<RentalManagementPlatformSqlContext>(options =>
				options.UseSqlServer(builder.Configuration.GetConnectionString("RentalManagementPlatformSql")));

			// Controllers + 解決JSON循環參照問題
			builder.Services.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.ReferenceHandler =
						System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
				});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

			builder.Services.AddScoped<IBookingRepository, BookingRepository>();
			builder.Services.AddScoped<IRoomRepository, RoomRepository>();
			builder.Services.AddScoped<ICouponRepository, CouponRepository>();
			builder.Services.AddScoped<IPaymentsRepository, PaymentsRepository>();
            builder.Services.AddScoped<IBookingService, BookingService>();
			builder.Services.AddScoped<IPaymentsService, PaymentsService>();

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

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
