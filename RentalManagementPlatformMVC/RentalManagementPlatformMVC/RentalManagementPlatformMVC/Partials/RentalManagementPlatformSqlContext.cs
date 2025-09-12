using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformMVC.Models;

public partial class RentalManagementPlatformSqlContext : DbContext
{
	public RentalManagementPlatformSqlContext() { }

	//EF Core建立連線
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		//如果外面已經建立完成了，不須重新設定，直接return
		if (optionsBuilder.IsConfigured)
			return;

		//IConfiguration設定來源
		//ConfigurationBuilder可以逐步組合多個設定來源，最後建立出一個 IConfiguration 物件
		IConfiguration config = new ConfigurationBuilder()
			// 設定檔案的基底路徑
			//AppDomain.CurrentDomain.BaseDirectory為目前應用程式的根目錄
			.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
			// 加入 JSON 檔
			.AddJsonFile("appsettings.json")
			// 生成 IConfiguration設定
			.Build();
		optionsBuilder.UseSqlServer(config.GetConnectionString("RentalManagementPlatformSql"));
	}

	partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Booking>(entity =>
		{
			// User一對多關聯
			entity.HasOne(e => e.Guest)
				  .WithMany(e => e.Bookings)
				  .HasForeignKey(e => e.GuestId)
				  .HasConstraintName("FK_BOOKING_USER")
				  .OnDelete(DeleteBehavior.Restrict);

			// Room一對多關聯
			entity.HasOne(e => e.Room)
				  .WithMany(e => e.Bookings)
				  .HasForeignKey(e => e.RoomId)
				  .HasConstraintName("FK_BOOKING_ROOMLIST")
				  .OnDelete(DeleteBehavior.Restrict);

			// Coupon一對多關聯
			entity.HasOne(e => e.Coupon)
				  .WithMany(e => e.Bookings)
				  .HasForeignKey(e => e.CouponId)
				  .HasConstraintName("FK_BOOKING_COUPON")
				  .IsRequired(false)
				  .OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<HostPayout>(entity =>
		{
			// User 一對多關聯
			entity.HasOne(e => e.Host)
				  .WithMany(e => e.HostPayouts)
				  .HasForeignKey(e => e.HostId)
				  .HasConstraintName("FK_HOSTPAYOUT_USER")
				  .OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<Payment>(entity =>
		{
			// Booking 一對多關聯
			entity.HasOne(e => e.Booking)
				  .WithMany(e => e.Payments)
				  .HasForeignKey(e => e.BookingId)
				  .HasConstraintName("FK_PAYMENT_BOOKING")
				  .OnDelete(DeleteBehavior.Restrict);

			// PaymentTransaction 一對多關聯
			entity.HasMany(e => e.PaymentTransactions)
				  .WithOne(e => e.Payment)
				  .HasForeignKey(e => e.PaymentId)
				  .HasConstraintName("FK_PAYMENTTRANSACTION_PAYMENT")
				  .OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<RoomList>(entity =>
		{
			// User一對多關聯
			entity.HasOne(e => e.Host)
				  .WithMany(e => e.RoomLists)
				  .HasForeignKey(e => e.HostId)
				  .HasConstraintName("FK_ROOMLIST_USER")
				  .OnDelete(DeleteBehavior.Restrict);
		});
	}
}
