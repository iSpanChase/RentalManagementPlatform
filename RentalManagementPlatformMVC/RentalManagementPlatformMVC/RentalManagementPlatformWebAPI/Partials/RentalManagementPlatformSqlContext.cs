using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformWebAPI.Models;

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

		modelBuilder.Entity<HostSubscription>(entity =>
		{
			// SubscriptionPlan 一對多關聯
			entity.HasOne(e => e.Plan)
				  .WithMany(e => e.HostSubscriptions)
				  .HasForeignKey(e => e.PlanId)
				  .HasConstraintName("FK_HOSTSUBSCRIPTION_SUBSCRIPTIONPLAN")
				  .OnDelete(DeleteBehavior.Restrict);

			// User 一對多關聯 (單一 Host)
			entity.HasOne(e => e.Host)
				  .WithMany(e => e.HostSubscriptions)
				  .HasForeignKey(e => e.HostId)
				  .HasConstraintName("FK_HOSTSUBSCRIPTION_USER")
				  .OnDelete(DeleteBehavior.Restrict);

			// SubscriptionBillingLog 一對多關聯
			entity.HasMany(e => e.SubscriptionBillingLogs)
				  .WithOne(e => e.HostSubscription)
				  .HasForeignKey(e => e.HostSubId)
				  .HasConstraintName("FK_SUBSCRIPTIONBILLINGLOG_HOSTSUBSCRIPTION")
				  .OnDelete(DeleteBehavior.Restrict);
		});

		modelBuilder.Entity<PointLedger>(entity =>
		{
			// User 一對多關聯
			entity.HasOne(e => e.Guest)
				  .WithMany(e => e.PointLedgers)
				  .HasForeignKey(e => e.GuestId)
				  .HasConstraintName("FK_POINTLEDGER_USER")
				  .OnDelete(DeleteBehavior.Restrict);
		});

		// FAQ 相關實體配置
		modelBuilder.Entity<FaqCategory>(entity =>
		{
			// 主鍵名稱沿用你的 Scaffold 欄位（FaqCategoriesId）
			entity.HasKey(e => e.FaqCategoriesId);

			entity.HasOne(e => e.Parent)            // 我有一個父
				  .WithMany(p => p.Children)        // 父有很多子
				  .HasForeignKey(e => e.ParentId)   // FK 欄位
				  .OnDelete(DeleteBehavior.NoAction) // 刪父不連動刪子，避免一串刪光
				  .HasConstraintName("FK_FaqCategory_Parent"); // 可選的 FK 名稱
		});

		// FaqArticle <-> FaqCategory
		modelBuilder.Entity<FaqArticle>(entity =>
		{
			// 明確指定欄位名稱
			entity.Property(e => e.CategoryId).HasColumnName("category_id");

			// 一(分類)對多(文章)
			entity.HasOne(a => a.Category)
				  .WithMany(c => c.FaqArticles)
				  .HasForeignKey(a => a.CategoryId)
				  .OnDelete(DeleteBehavior.NoAction)
				  .HasConstraintName("FK_FAQ_ARTICLES_FAQ_CATEGORIES");
		});

		modelBuilder.Entity<FaqFeedback>(entity =>
		{
			entity.HasOne(f => f.Article)
				  .WithMany(a => a.FaqFeedbacks)
				  .HasForeignKey(f => f.ArticleId)
				  .OnDelete(DeleteBehavior.Cascade)           // 刪文章時連動刪回饋（你DB就是這樣）
				  .HasConstraintName("FK_FaqFeedback_Article");
		});

		modelBuilder.Entity<Role>(e =>
		{
			e.HasIndex(x => x.RoleCode).IsUnique();
			e.Property(x => x.RoleCode).HasMaxLength(64).IsRequired();
			e.Property(x => x.RoleName).HasMaxLength(128).IsRequired();

			e.HasMany(x => x.RolePermissions)
			 .WithOne(x => x.Role)
			 .HasForeignKey(x => x.RoleId)
			 .OnDelete(DeleteBehavior.Cascade);

			e.HasMany(x => x.UserRoles)
			 .WithOne(x => x.Role)
			 .HasForeignKey(x => x.RoleId)
			 .OnDelete(DeleteBehavior.Restrict); // 視需求：避免刪除角色時把使用者關聯全刪
		});

		modelBuilder.Entity<Permission>(e =>
		{
			e.HasIndex(x => x.PermCode).IsUnique();
			e.Property(x => x.PermCode).HasMaxLength(128).IsRequired();
			e.Property(x => x.PermName).HasMaxLength(128).IsRequired();

			e.HasMany(x => x.RolePermissions)
			 .WithOne(x => x.Permission)
			 .HasForeignKey(x => x.PermissionId)
			 .OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<UserRole>(e =>
		{
			e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
			e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
			e.HasOne(x => x.User)
			 .WithMany(u => u.UserRoles)
			 .HasForeignKey(x => x.UserId)
			 .OnDelete(DeleteBehavior.Cascade);
		});

		modelBuilder.Entity<RolePermission>(e =>
		{
			e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
			e.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
		});
	}
}