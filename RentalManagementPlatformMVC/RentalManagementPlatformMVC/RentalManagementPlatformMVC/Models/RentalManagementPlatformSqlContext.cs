using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformMVC.Models;

public partial class RentalManagementPlatformSqlContext : DbContext
{
    public RentalManagementPlatformSqlContext(DbContextOptions<RentalManagementPlatformSqlContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<AnomalyDetectionLog> AnomalyDetectionLogs { get; set; }

    public virtual DbSet<AnomalyRule> AnomalyRules { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingGuest> BookingGuests { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<CouponDistrict> CouponDistricts { get; set; }

    public virtual DbSet<CouponGuest> CouponGuests { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<FaqArticle> FaqArticles { get; set; }

    public virtual DbSet<FaqCategory> FaqCategories { get; set; }

    public virtual DbSet<FaqFeedback> FaqFeedbacks { get; set; }

    public virtual DbSet<HostPayout> HostPayouts { get; set; }

    public virtual DbSet<HostPayoutItem> HostPayoutItems { get; set; }

    public virtual DbSet<HostSubscription> HostSubscriptions { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Mongodb> Mongodbs { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentTransaction> PaymentTransactions { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PointLedger> PointLedgers { get; set; }

    public virtual DbSet<PointRule> PointRules { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<PostsCategory> PostsCategories { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<RoomList> RoomLists { get; set; }

    public virtual DbSet<RoomPhoto> RoomPhotos { get; set; }

    public virtual DbSet<SubscriptionBillingLog> SubscriptionBillingLogs { get; set; }

    public virtual DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavoriteReport> UserFavoriteReports { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK__ADDRESS__CAA247C847D37F08");

            entity.ToTable("ADDRESS");

            entity.Property(e => e.AddressId)
                .ValueGeneratedNever()
                .HasColumnName("address_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.Latitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Street)
                .HasMaxLength(512)
                .HasColumnName("street");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<AnomalyDetectionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ANOMALY___9E2397E0E3C384AA");

            entity.ToTable("ANOMALY_DETECTION_LOG");

            entity.Property(e => e.LogId)
                .ValueGeneratedNever()
                .HasColumnName("log_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DetectedValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("detected_value");
            entity.Property(e => e.ExpectedValue)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("expected_value");
            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.TargetId).HasColumnName("target_id");
        });

        modelBuilder.Entity<AnomalyRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__ANOMALY___E92A92969EEA1F33");

            entity.ToTable("ANOMALY_RULE");

            entity.Property(e => e.RuleId)
                .ValueGeneratedNever()
                .HasColumnName("rule_id");
            entity.Property(e => e.ConditionExpression)
                .HasMaxLength(512)
                .HasColumnName("condition_expression");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.RuleName)
                .HasMaxLength(100)
                .HasColumnName("rule_name");
            entity.Property(e => e.TargetType)
                .HasMaxLength(50)
                .HasColumnName("target_type");
            entity.Property(e => e.ThresholdValue)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("threshold_value");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__BOOKING__5DE3A5B1B95DB1AF");

            entity.ToTable("BOOKING");

            entity.Property(e => e.BookingId)
                .ValueGeneratedNever()
                .HasColumnName("booking_id");
            entity.Property(e => e.CheckIn).HasColumnName("check_in");
            entity.Property(e => e.CheckOut).HasColumnName("check_out");
            entity.Property(e => e.CommissionRateSnapshot)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("commission_rate_snapshot");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(512)
                .HasColumnName("order_number");
            entity.Property(e => e.PointsEarned).HasColumnName("points_earned");
            entity.Property(e => e.PointsRedeemed).HasColumnName("points_redeemed");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasColumnName("status");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("total_price");

			// 加關聯設定
			entity.HasOne(e => e.Guest)                      // Booking 有一個 Guest
				  .WithMany(e => e.Bookings)                 // User 有很多 Bookings
				  .HasForeignKey(e => e.GuestId)             // FK 是 Booking.GuestId
				  .HasConstraintName("FK_BOOKING_USER");     // FK 名稱可自訂

			entity.HasOne(e => e.Room)                       // Booking 有一個 Room
				  .WithMany(e => e.Bookings)                 // Room 可以有多個 Booking
				  .HasForeignKey(e => e.RoomId)              // FK 是 Booking.RoomId
				  .HasConstraintName("FK_BOOKING_ROOMLIST"); // FK 名稱可自訂

			entity.HasOne(e => e.Coupon)                     // Booking 有一個 Coupon
		          .WithMany(e => e.Bookings)                 // Coupon 可以被多個 Booking 使用
		          .HasForeignKey(e => e.CouponId)            // FK 是 Booking.CouponId
				  .HasConstraintName("FK_BOOKING_COUPON")    // FK 名稱可自訂
				  .IsRequired(false);                        // 可以沒有 Coupon
		});

        modelBuilder.Entity<BookingGuest>(entity =>
        {
            entity.HasKey(e => e.BookingGuestId).HasName("PK__BOOKING___A6D88E8825FF3AFC");

            entity.ToTable("BOOKING_GUEST");

            entity.Property(e => e.BookingGuestId)
                .ValueGeneratedNever()
                .HasColumnName("booking_guest_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.GuestIdNumber)
                .HasMaxLength(512)
                .HasColumnName("guest_id_number");
            entity.Property(e => e.GuestName)
                .HasMaxLength(512)
                .HasColumnName("guest_name");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoriesId).HasName("PK__CATEGORI__92BEE78A6AD46DB9");

            entity.ToTable("CATEGORIES");

            entity.HasIndex(e => e.Name, "UQ__CATEGORI__72E12F1BA1785D24").IsUnique();

            entity.Property(e => e.CategoriesId).HasColumnName("categories_id");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(false)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.CityId).HasName("PK__CITY__031491A8AEF7F11F");

            entity.ToTable("CITY");

            entity.Property(e => e.CityId)
                .ValueGeneratedNever()
                .HasColumnName("city_id");
            entity.Property(e => e.CityName)
                .HasMaxLength(512)
                .HasColumnName("city_name");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.CouponId).HasName("PK__COUPON__58CF6389B451D7D2");

            entity.ToTable("COUPON");

            entity.Property(e => e.CouponId)
                .ValueGeneratedNever()
                .HasColumnName("coupon_id");
            entity.Property(e => e.CouponName)
                .HasMaxLength(512)
                .HasColumnName("coupon_name");
            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .HasColumnName("description");
            entity.Property(e => e.DiscountCode)
                .HasMaxLength(512)
                .HasColumnName("discount_code");
            entity.Property(e => e.DiscountMethod)
                .HasMaxLength(512)
                .HasColumnName("discount_method");
            entity.Property(e => e.DiscountQuota)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("discount_quota");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.EndRentalPeriod).HasColumnName("end_rental_period");
            entity.Property(e => e.LowSpend)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("low_spend");
            entity.Property(e => e.MaxRentalPeriod).HasColumnName("max_rental_period");
            entity.Property(e => e.MinRentalPeriod).HasColumnName("min_rental_period");
            entity.Property(e => e.StartRentalPeriod).HasColumnName("start_rental_period");
        });

        modelBuilder.Entity<CouponDistrict>(entity =>
        {
            entity.HasKey(e => e.CouponDistrictId).HasName("PK__COUPON_D__1F5D108E58668C71");

            entity.ToTable("COUPON_DISTRICT");

            entity.Property(e => e.CouponDistrictId)
                .ValueGeneratedNever()
                .HasColumnName("coupon_district_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
        });

        modelBuilder.Entity<CouponGuest>(entity =>
        {
            entity.HasKey(e => e.CouponGuestId).HasName("PK__COUPON_G__538EB55DBD8D21D0");

            entity.ToTable("COUPON_GUEST");

            entity.Property(e => e.CouponGuestId)
                .ValueGeneratedNever()
                .HasColumnName("coupon_guest_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.RemoveAt).HasColumnName("remove_at");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__DISTRICT__2521322B1747C515");

            entity.ToTable("DISTRICT");

            entity.Property(e => e.DistrictId)
                .ValueGeneratedNever()
                .HasColumnName("district_id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(512)
                .HasColumnName("district_name");
        });

        modelBuilder.Entity<FaqArticle>(entity =>
        {
            entity.HasKey(e => e.FaqArticlesId).HasName("PK__FAQ_ARTI__B0FC36A6376104A2");

            entity.ToTable("FAQ_ARTICLES");

            entity.HasIndex(e => e.Slug, "UQ__FAQ_ARTI__32DD1E4C8FF6DBC4").IsUnique();

            entity.Property(e => e.FaqArticlesId).HasColumnName("faq_articles_id");
            entity.Property(e => e.AuthorId).HasColumnName("author_id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Content)
                .HasMaxLength(512)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.HelpfulNo).HasColumnName("helpful_no");
            entity.Property(e => e.HelpfulYes).HasColumnName("helpful_yes");
            entity.Property(e => e.IsPinned).HasColumnName("is_pinned");
            entity.Property(e => e.PublishedAt).HasColumnName("published_at");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .HasColumnName("slug");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Summary)
                .HasMaxLength(255)
                .HasColumnName("summary");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.ViewCount).HasColumnName("view_count");
        });

        modelBuilder.Entity<FaqCategory>(entity =>
        {
            entity.HasKey(e => e.FaqCategoriesId).HasName("PK__FAQ_CATE__09E5F979407FFBEC");

            entity.ToTable("FAQ_CATEGORIES");

            entity.HasIndex(e => e.Slug, "UQ__FAQ_CATE__32DD1E4C40D6D32E").IsUnique();

            entity.Property(e => e.FaqCategoriesId).HasColumnName("faq_categories_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");
            entity.Property(e => e.Slug)
                .HasMaxLength(255)
                .HasColumnName("slug");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<FaqFeedback>(entity =>
        {
            entity.HasKey(e => e.FaqFeedbackId).HasName("PK__FAQ_FEED__4E34B38312D5D4B0");

            entity.ToTable("FAQ_FEEDBACK");

            entity.Property(e => e.FaqFeedbackId).HasColumnName("faq_feedback_id");
            entity.Property(e => e.ArticleId).HasColumnName("article_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EscalatedToTicket).HasColumnName("escalated_to_ticket");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.Sentiment)
                .HasMaxLength(255)
                .HasColumnName("sentiment");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<HostPayout>(entity =>
        {
            entity.HasKey(e => e.PayoutId).HasName("PK__HOST_PAY__3B0771ECF43D22EE");

            entity.ToTable("HOST_PAYOUT");

            entity.Property(e => e.PayoutId)
                .ValueGeneratedNever()
                .HasColumnName("payout_id");
            entity.Property(e => e.AmountGross)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount_gross");
            entity.Property(e => e.AmountNet)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount_net");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CycleEnd).HasColumnName("cycle_end");
            entity.Property(e => e.CycleStart).HasColumnName("cycle_start");
            entity.Property(e => e.HostId).HasColumnName("host_id");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PlatformFee)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("platform_fee");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasColumnName("status");
        });

        modelBuilder.Entity<HostPayoutItem>(entity =>
        {
            entity.HasKey(e => e.PayoutItemId).HasName("PK__HOST_PAY__7BD82C6EE5E33B00");

            entity.ToTable("HOST_PAYOUT_ITEM");

            entity.Property(e => e.PayoutItemId)
                .ValueGeneratedNever()
                .HasColumnName("payout_item_id");
            entity.Property(e => e.AmountGross)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount_gross");
            entity.Property(e => e.AmountNet)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount_net");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CommissionPct)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("commission_pct");
            entity.Property(e => e.OrderNumberSnapshot)
                .HasMaxLength(512)
                .HasColumnName("order_number_snapshot");
            entity.Property(e => e.PayoutId).HasColumnName("payout_id");
            entity.Property(e => e.PlatformFee)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("platform_fee");
        });

        modelBuilder.Entity<HostSubscription>(entity =>
        {
            entity.HasKey(e => e.HostSubId).HasName("PK__HOST_SUB__70E025A835209577");

            entity.ToTable("HOST_SUBSCRIPTION");

            entity.Property(e => e.HostSubId)
                .ValueGeneratedNever()
                .HasColumnName("host_sub_id");
            entity.Property(e => e.CancelAtPeriodEnd).HasColumnName("cancel_at_period_end");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.HostId).HasColumnName("host_id");
            entity.Property(e => e.NextBillingDate).HasColumnName("next_billing_date");
            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasColumnName("status");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__MESSAGE__0BBF6EE6B0197031");

            entity.ToTable("MESSAGE");

            entity.Property(e => e.MessageId).HasColumnName("message_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Content)
                .HasMaxLength(512)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.FaqId).HasColumnName("faq_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
        });

        modelBuilder.Entity<Mongodb>(entity =>
        {
            entity.HasKey(e => e.MongodbId).HasName("PK__MONGODB__EC04A0A448955789");

            entity.ToTable("MONGODB");

            entity.Property(e => e.MongodbId)
                .HasMaxLength(24)
                .HasColumnName("mongodb_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PAYMENT__ED1FC9EAF9933CAE");

            entity.ToTable("PAYMENT");

            entity.Property(e => e.PaymentId)
                .ValueGeneratedNever()
                .HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Method)
                .HasMaxLength(512)
                .HasColumnName("method");
            entity.Property(e => e.OrderNumberSnapshot)
                .HasMaxLength(512)
                .HasColumnName("order_number_snapshot");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaymentRef)
                .HasMaxLength(512)
                .HasColumnName("payment_ref");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasColumnName("status");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PAYMENT___85C600AF6418E5DA");

            entity.ToTable("PAYMENT_TRANSACTION");

            entity.Property(e => e.TransactionId)
                .ValueGeneratedNever()
                .HasColumnName("transaction_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Provider)
                .HasMaxLength(512)
                .HasColumnName("provider");
            entity.Property(e => e.ProviderTxnId)
                .HasMaxLength(512)
                .HasColumnName("provider_txn_id");
            entity.Property(e => e.ResponseCode)
                .HasMaxLength(512)
                .HasColumnName("response_code");
            entity.Property(e => e.ResponseMessage)
                .HasMaxLength(512)
                .HasColumnName("response_message");
            entity.Property(e => e.TxnRef)
                .HasMaxLength(512)
                .HasColumnName("txn_ref");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__PERMISSI__E5331AFA9DCF2327");

            entity.ToTable("PERMISSIONS");

            entity.HasIndex(e => e.PermCode, "UQ__PERMISSI__B74793E243F6C5CF").IsUnique();

            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Module)
                .HasMaxLength(50)
                .HasColumnName("module");
            entity.Property(e => e.PermCode)
                .HasMaxLength(100)
                .HasColumnName("perm_code");
            entity.Property(e => e.PermName)
                .HasMaxLength(200)
                .HasColumnName("perm_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<PointLedger>(entity =>
        {
            entity.HasKey(e => e.LedgerId).HasName("PK__POINT_LE__97EDEDA184BF8022");

            entity.ToTable("POINT_LEDGER");

            entity.Property(e => e.LedgerId)
                .ValueGeneratedNever()
                .HasColumnName("ledger_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.Note)
                .HasMaxLength(512)
                .HasColumnName("note");
            entity.Property(e => e.OccurredAt).HasColumnName("occurred_at");
            entity.Property(e => e.OrderNumberSnapshot)
                .HasMaxLength(512)
                .HasColumnName("order_number_snapshot");
            entity.Property(e => e.Points).HasColumnName("points");
            entity.Property(e => e.Type)
                .HasMaxLength(512)
                .HasColumnName("type");
        });

        modelBuilder.Entity<PointRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__POINT_RU__E92A9296F2D7BE3A");

            entity.ToTable("POINT_RULE");

            entity.Property(e => e.RuleId)
                .ValueGeneratedNever()
                .HasColumnName("rule_id");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EarnRatePerNtd)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("earn_rate_per_ntd");
            entity.Property(e => e.ExpiryMonths).HasColumnName("expiry_months");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.MaxPointsPerOrder).HasColumnName("max_points_per_order");
            entity.Property(e => e.RedeemRateNtdPerPt)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("redeem_rate_ntd_per_pt");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.PostsId).HasName("PK__POSTS__7F3EFABEE23FEB79");

            entity.ToTable("POSTS");

            entity.HasIndex(e => new { e.Status, e.RegionId, e.CreatedAt }, "POSTS_index_0");

            entity.Property(e => e.PostsId).HasColumnName("posts_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.ContactName)
                .HasMaxLength(100)
                .HasColumnName("contact_name");
            entity.Property(e => e.ContactNote)
                .HasMaxLength(255)
                .HasColumnName("contact_note");
            entity.Property(e => e.ContactPhone)
                .HasMaxLength(30)
                .HasColumnName("contact_phone");
            entity.Property(e => e.Content)
                .HasMaxLength(512)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.ExpireAt).HasColumnName("expire_at");
            entity.Property(e => e.ProposedPrice)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("proposed_price");
            entity.Property(e => e.PublishAt).HasColumnName("publish_at");
            entity.Property(e => e.RegionId).HasColumnName("region_id");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasDefaultValue("draft")
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(120)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Views)
                .HasDefaultValue(0)
                .HasColumnName("views");
        });

        modelBuilder.Entity<PostsCategory>(entity =>
        {
            entity.HasKey(e => e.PostCategoriesId).HasName("PK__POSTS_CA__CA316AA505ECF9E6");

            entity.ToTable("POSTS_CATEGORIES");

            entity.Property(e => e.PostCategoriesId).HasColumnName("post_categories_id");
            entity.Property(e => e.CategoriesId).HasColumnName("categories_id");
            entity.Property(e => e.PostsId).HasColumnName("posts_id");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__REVIEW__60883D9091A0A191");

            entity.ToTable("REVIEW");

            entity.Property(e => e.ReviewId)
                .ValueGeneratedNever()
                .HasColumnName("review_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Comment)
                .HasMaxLength(512)
                .HasColumnName("comment");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.HostId).HasColumnName("host_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.ReviewerId).HasColumnName("reviewer_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLES__760965CC8E2AB6A0");

            entity.ToTable("ROLES");

            entity.HasIndex(e => e.RoleCode, "UQ__ROLES__BAE630752D55EEEA").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .HasColumnName("role_code");
            entity.Property(e => e.RoleName)
                .HasMaxLength(100)
                .HasColumnName("role_name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.RolePermissionId).HasName("PK__ROLE_PER__B1E85A1004CB77E8");

            entity.ToTable("ROLE_PERMISSIONS");

            entity.Property(e => e.RolePermissionId).HasColumnName("role_permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
        });

        modelBuilder.Entity<RoomList>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__ROOM_LIS__19675A8A3CF82736");

            entity.ToTable("ROOM_LIST");

            entity.Property(e => e.RoomId)
                .ValueGeneratedNever()
                .HasColumnName("room_id");
            entity.Property(e => e.AddressId).HasColumnName("address_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(512)
                .HasColumnName("description");
            entity.Property(e => e.HostId).HasColumnName("host_id");
            entity.Property(e => e.MaxGuests).HasColumnName("max_guests");
            entity.Property(e => e.PricePerNight)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("price_per_night");
            entity.Property(e => e.Status)
                .HasMaxLength(512)
                .HasColumnName("status");
            entity.Property(e => e.Title)
                .HasMaxLength(512)
                .HasColumnName("title");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

			// 加關聯設定
			entity.HasOne(e => e.Host)                      // RoomList 有一個 Host
				  .WithMany(e => e.RoomLists)                 // Host 有很多 RoomList
				  .HasForeignKey(e => e.HostId)             // FK 是 Booking.GuestId
				  .HasConstraintName("FK_ROOMLIST_USER");     // FK 名稱可自訂
		});

        modelBuilder.Entity<RoomPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__ROOM_PHO__CB48C83D2A308619");

            entity.ToTable("ROOM_PHOTO");

            entity.Property(e => e.PhotoId)
                .ValueGeneratedNever()
                .HasColumnName("photo_id");
            entity.Property(e => e.Bucket)
                .HasMaxLength(128)
                .HasDefaultValue("room-photos")
                .HasColumnName("bucket");
            entity.Property(e => e.ContentType)
                .HasMaxLength(64)
                .HasDefaultValue("image/jpeg")
                .HasColumnName("content_type");
            entity.Property(e => e.ObjectKey)
                .HasMaxLength(512)
                .HasColumnName("object_key");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");
        });

        modelBuilder.Entity<SubscriptionBillingLog>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__SUBSCRIP__D706DDB332BB5CAF");

            entity.ToTable("SUBSCRIPTION_BILLING_LOG");

            entity.Property(e => e.BillId)
                .ValueGeneratedNever()
                .HasColumnName("bill_id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.HostSubId).HasColumnName("host_sub_id");
            entity.Property(e => e.Note)
                .HasMaxLength(512)
                .HasColumnName("note");
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.PaidStatus)
                .HasMaxLength(512)
                .HasColumnName("paid_status");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__SUBSCRIP__BE9F8F1DF4FE8258");

            entity.ToTable("SUBSCRIPTION_PLAN");

            entity.Property(e => e.PlanId)
                .ValueGeneratedNever()
                .HasColumnName("plan_id");
            entity.Property(e => e.CommissionRate)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("commission_rate");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.MonthlyFee)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("monthly_fee");
            entity.Property(e => e.PerkAnalytics).HasColumnName("perk_analytics");
            entity.Property(e => e.PerkPriority).HasColumnName("perk_priority");
            entity.Property(e => e.PlanName)
                .HasMaxLength(512)
                .HasColumnName("plan_name");
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.SupportTicketsId).HasName("PK__SUPPORT___494F33232DD042B3");

            entity.ToTable("SUPPORT_TICKETS");

            entity.Property(e => e.SupportTicketsId).HasColumnName("support_tickets_id");
            entity.Property(e => e.AssignedStaffId).HasColumnName("assigned_staff_id");
            entity.Property(e => e.ContactEmail)
                .HasMaxLength(255)
                .HasColumnName("contact_email");
            entity.Property(e => e.Content)
                .HasMaxLength(512)
                .HasColumnName("content");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.Priority)
                .HasMaxLength(255)
                .HasColumnName("priority");
            entity.Property(e => e.RelatedFeedbackId).HasColumnName("related_feedback_id");
            entity.Property(e => e.Source)
                .HasMaxLength(255)
                .HasColumnName("source");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.Subject)
                .HasMaxLength(255)
                .HasColumnName("subject");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USER__B9BE370F0DBDA1DF");

            entity.ToTable("USER");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(512)
                .HasColumnName("address");
            entity.Property(e => e.AutoSubscribe).HasColumnName("auto_subscribe");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(512)
                .HasColumnName("email");
            entity.Property(e => e.Gender)
                .HasMaxLength(512)
                .HasColumnName("gender");
            entity.Property(e => e.Isverified).HasColumnName("isverified");
            entity.Property(e => e.Name)
                .HasMaxLength(512)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(512)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(512)
                .HasColumnName("phone");
            entity.Property(e => e.Point).HasColumnName("point");
            entity.Property(e => e.ProfileImageurl)
                .HasMaxLength(512)
                .HasColumnName("profile_imageurl");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.Username)
                .HasMaxLength(512)
                .HasColumnName("username");
        });

        modelBuilder.Entity<UserFavoriteReport>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__USER_FAV__46ACF4CB21E56733");

            entity.ToTable("USER_FAVORITE_REPORT");

            entity.Property(e => e.FavoriteId)
                .ValueGeneratedNever()
                .HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ReportParams)
                .HasMaxLength(512)
                .HasColumnName("report_params");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .HasColumnName("report_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__USER_ROL__B8D9ABA2C4744ADA");

            entity.ToTable("USER_ROLES");

            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
