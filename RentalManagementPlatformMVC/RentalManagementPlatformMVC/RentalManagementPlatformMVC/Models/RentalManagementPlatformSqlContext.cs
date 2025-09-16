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

    public virtual DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

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
            entity.HasKey(e => e.AddressId).HasName("PK__ADDRESS__CAA247C86F8A2165");

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

            entity.HasOne(d => d.District).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK_ADDRESS_district_id_DISTRICT_district_id");
        });

        modelBuilder.Entity<AnomalyDetectionLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__ANOMALY___9E2397E0D099628B");

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

            entity.HasOne(d => d.Rule).WithMany(p => p.AnomalyDetectionLogs)
                .HasForeignKey(d => d.RuleId)
                .HasConstraintName("FK_ANOMALY_DETECTION_LOG_rule_id_ANOMALY_RULE_rule_id");
        });

        modelBuilder.Entity<AnomalyRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__ANOMALY___E92A929642C8774F");

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
            entity.HasKey(e => e.BookingId).HasName("PK__BOOKING__5DE3A5B1F79B0AB0");

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

            entity.HasOne(d => d.Coupon).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK_BOOKING_coupon_id_COUPON_coupon_id");

            entity.HasOne(d => d.Guest).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("FK_BOOKING_guest_id_USER_user_id");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_BOOKING_room_id_ROOM_LIST_room_id");
        });

        modelBuilder.Entity<BookingGuest>(entity =>
        {
            entity.HasKey(e => e.BookingGuestId).HasName("PK__BOOKING___A6D88E883406CE09");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingGuests)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_BOOKING_GUEST_booking_id_BOOKING_booking_id");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoriesId).HasName("PK__CATEGORI__92BEE78AD4015464");

            entity.ToTable("CATEGORIES");

            entity.HasIndex(e => e.Name, "UQ__CATEGORI__72E12F1B3FCE9575").IsUnique();

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
            entity.HasKey(e => e.CityId).HasName("PK__CITY__031491A8DEA847CF");

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
            entity.HasKey(e => e.CouponId).HasName("PK__COUPON__58CF6389C3D23323");

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
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.LowSpend)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("low_spend");
            entity.Property(e => e.MaxRentalPeriod).HasColumnName("max_rental_period");
            entity.Property(e => e.MinRentalPeriod).HasColumnName("min_rental_period");
            entity.Property(e => e.StartRentalPeriod).HasColumnName("start_rental_period");
        });

        modelBuilder.Entity<CouponDistrict>(entity =>
        {
            entity.HasKey(e => e.CouponDistrictId).HasName("PK__COUPON_D__1F5D108E811BAF7C");

            entity.ToTable("COUPON_DISTRICT");

            entity.Property(e => e.CouponDistrictId)
                .ValueGeneratedNever()
                .HasColumnName("coupon_district_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponDistricts)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK_COUPON_DISTRICT_coupon_id_COUPON_coupon_id");

            entity.HasOne(d => d.District).WithMany(p => p.CouponDistricts)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("FK_COUPON_DISTRICT_district_id_DISTRICT_district_id");
        });

        modelBuilder.Entity<CouponGuest>(entity =>
        {
            entity.HasKey(e => e.CouponGuestId).HasName("PK__COUPON_G__538EB55D98178C2B");

            entity.ToTable("COUPON_GUEST");

            entity.Property(e => e.CouponGuestId)
                .ValueGeneratedNever()
                .HasColumnName("coupon_guest_id");
            entity.Property(e => e.CouponId).HasColumnName("coupon_id");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.GuestId).HasColumnName("guest_id");
            entity.Property(e => e.RemoveAt).HasColumnName("remove_at");

            entity.HasOne(d => d.Coupon).WithMany(p => p.CouponGuests)
                .HasForeignKey(d => d.CouponId)
                .HasConstraintName("FK_COUPON_GUEST_coupon_id_COUPON_coupon_id");

            entity.HasOne(d => d.Guest).WithMany(p => p.CouponGuests)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("FK_COUPON_GUEST_guest_id_USER_user_id");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.DistrictId).HasName("PK__DISTRICT__2521322BED2DA422");

            entity.ToTable("DISTRICT");

            entity.Property(e => e.DistrictId)
                .ValueGeneratedNever()
                .HasColumnName("district_id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.DistrictName)
                .HasMaxLength(512)
                .HasColumnName("district_name");

            entity.HasOne(d => d.City).WithMany(p => p.Districts)
                .HasForeignKey(d => d.CityId)
                .HasConstraintName("FK_DISTRICT_city_id_CITY_city_id");
        });

        modelBuilder.Entity<FaqArticle>(entity =>
        {
            entity.HasKey(e => e.FaqArticlesId).HasName("PK__FAQ_ARTI__B0FC36A6D2AD4930");

            entity.ToTable("FAQ_ARTICLES");

            entity.HasIndex(e => e.Slug, "UQ__FAQ_ARTI__32DD1E4C3C50D08A").IsUnique();

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

            entity.HasOne(d => d.Author).WithMany(p => p.FaqArticles)
                .HasForeignKey(d => d.AuthorId)
                .HasConstraintName("FK_FAQ_ARTICLES_author_id_USER_user_id");

            entity.HasOne(d => d.Category).WithMany(p => p.FaqArticles)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_FAQ_ARTICLES_category_id_FAQ_CATEGORIES_faq_categories_id");
        });

        modelBuilder.Entity<FaqCategory>(entity =>
        {
            entity.HasKey(e => e.FaqCategoriesId).HasName("PK__FAQ_CATE__09E5F979F69B8446");

            entity.ToTable("FAQ_CATEGORIES");

            entity.HasIndex(e => e.Slug, "UQ__FAQ_CATE__32DD1E4C721346C6").IsUnique();

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

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent)
                .HasForeignKey(d => d.ParentId)
                .HasConstraintName("FK_FAQ_CATEGORIES_parent_id_FAQ_CATEGORIES_faq_categories_id");
        });

        modelBuilder.Entity<FaqFeedback>(entity =>
        {
            entity.HasKey(e => e.FaqFeedbackId).HasName("PK__FAQ_FEED__4E34B383EBD1A8FA");

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

            entity.HasOne(d => d.Article).WithMany(p => p.FaqFeedbacks)
                .HasForeignKey(d => d.ArticleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FAQ_FEEDBACK_article_id_FAQ_ARTICLES_faq_articles_id");
        });

        modelBuilder.Entity<HostPayout>(entity =>
        {
            entity.HasKey(e => e.PayoutId).HasName("PK__HOST_PAY__3B0771ECB7C7206B");

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

            entity.HasOne(d => d.Host).WithMany(p => p.HostPayouts)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("FK_HOST_PAYOUT_host_id_USER_user_id");
        });

        modelBuilder.Entity<HostPayoutItem>(entity =>
        {
            entity.HasKey(e => e.PayoutItemId).HasName("PK__HOST_PAY__7BD82C6E704D6CF1");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.HostPayoutItems)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_HOST_PAYOUT_ITEM_booking_id_BOOKING_booking_id");

            entity.HasOne(d => d.Payout).WithMany(p => p.HostPayoutItems)
                .HasForeignKey(d => d.PayoutId)
                .HasConstraintName("FK_HOST_PAYOUT_ITEM_payout_id_HOST_PAYOUT_payout_id");
        });

        modelBuilder.Entity<HostSubscription>(entity =>
        {
            entity.HasKey(e => e.HostSubId).HasName("PK__HOST_SUB__70E025A832292C5F");

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

            entity.HasOne(d => d.Host).WithMany(p => p.HostSubscriptions)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("FK_HOST_SUBSCRIPTION_host_id_USER_user_id");

            entity.HasOne(d => d.Plan).WithMany(p => p.HostSubscriptions)
                .HasForeignKey(d => d.PlanId)
                .HasConstraintName("FK_HOST_SUBSCRIPTION_plan_id_SUBSCRIPTION_PLAN_plan_id");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__MESSAGE__0BBF6EE681CF34FC");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.Messages)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_MESSAGE_booking_id_BOOKING_booking_id");

            entity.HasOne(d => d.Faq).WithMany(p => p.Messages)
                .HasForeignKey(d => d.FaqId)
                .HasConstraintName("FK_MESSAGE_faq_id_FAQ_ARTICLES_faq_articles_id");

            entity.HasOne(d => d.Receiver).WithMany(p => p.MessageReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .HasConstraintName("FK_MESSAGE_receiver_id_USER_user_id");

            entity.HasOne(d => d.Room).WithMany(p => p.Messages)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_MESSAGE_room_id_ROOM_LIST_room_id");

            entity.HasOne(d => d.Sender).WithMany(p => p.MessageSenders)
                .HasForeignKey(d => d.SenderId)
                .HasConstraintName("FK_MESSAGE_sender_id_USER_user_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.Messages)
                .HasForeignKey(d => d.TicketId)
                .HasConstraintName("FK_MESSAGE_ticket_id_SUPPORT_TICKETS_support_tickets_id");
        });

        modelBuilder.Entity<Mongodb>(entity =>
        {
            entity.HasKey(e => e.MongodbId).HasName("PK__MONGODB__EC04A0A479C500ED");

            entity.ToTable("MONGODB");

            entity.Property(e => e.MongodbId)
                .HasMaxLength(24)
                .HasColumnName("mongodb_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Listing).WithMany(p => p.Mongodbs)
                .HasForeignKey(d => d.ListingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MONGODB_ListingId_ROOM_LIST_room_id");
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__PASSWORD__CB3C9E172B51613B");

            entity.ToTable("PASSWORD_RESET_TOKENS");

            entity.HasIndex(e => e.UserId, "IX_RESET_user");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.TokenHash)
                .HasMaxLength(128)
                .HasColumnName("token_hash");
            entity.Property(e => e.UsedAt).HasColumnName("used_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordResetTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PASSWORD_RESET_TOKENS_user_id_USER_user_id");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__PAYMENT__ED1FC9EACA80488E");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_PAYMENT_booking_id_BOOKING_booking_id");
        });

        modelBuilder.Entity<PaymentTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__PAYMENT___85C600AFE6316F8E");

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

            entity.HasOne(d => d.Payment).WithMany(p => p.PaymentTransactions)
                .HasForeignKey(d => d.PaymentId)
                .HasConstraintName("FK_PAYMENT_TRANSACTION_payment_id_PAYMENT_payment_id");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__PERMISSI__E5331AFAF943CA4D");

            entity.ToTable("PERMISSIONS");

            entity.HasIndex(e => e.PermCode, "UQ__PERMISSI__B74793E249F5B176").IsUnique();

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
            entity.HasKey(e => e.LedgerId).HasName("PK__POINT_LE__97EDEDA11A7393F6");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_POINT_LEDGER_booking_id_BOOKING_booking_id");

            entity.HasOne(d => d.Guest).WithMany(p => p.PointLedgers)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("FK_POINT_LEDGER_guest_id_USER_user_id");
        });

        modelBuilder.Entity<PointRule>(entity =>
        {
            entity.HasKey(e => e.RuleId).HasName("PK__POINT_RU__E92A92966ED42D4E");

            entity.ToTable("POINT_RULE");

            entity.Property(e => e.RuleId).HasColumnName("rule_id");
            entity.Property(e => e.ActiveFrom).HasColumnName("active_from");
            entity.Property(e => e.ActiveTo).HasColumnName("active_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.EarnRatePerNtd)
                .HasColumnType("decimal(18, 4)")
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
            entity.HasKey(e => e.PostsId).HasName("PK__POSTS__7F3EFABEE51CEC05");

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

            entity.HasOne(d => d.Region).WithMany(p => p.Posts)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTS_region_id_DISTRICT_district_id");

            entity.HasOne(d => d.User).WithMany(p => p.Posts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTS_user_id_USER_user_id");
        });

        modelBuilder.Entity<PostsCategory>(entity =>
        {
            entity.HasKey(e => e.PostCategoriesId).HasName("PK__POSTS_CA__CA316AA503DC46A8");

            entity.ToTable("POSTS_CATEGORIES");

            entity.Property(e => e.PostCategoriesId).HasColumnName("post_categories_id");
            entity.Property(e => e.CategoriesId).HasColumnName("categories_id");
            entity.Property(e => e.PostsId).HasColumnName("posts_id");

            entity.HasOne(d => d.Categories).WithMany(p => p.PostsCategories)
                .HasForeignKey(d => d.CategoriesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTS_CATEGORIES_categories_id_CATEGORIES_categories_id");

            entity.HasOne(d => d.Posts).WithMany(p => p.PostsCategories)
                .HasForeignKey(d => d.PostsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_POSTS_CATEGORIES_posts_id_POSTS_posts_id");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__REVIEW__60883D90FFAE34B8");

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

            entity.HasOne(d => d.Booking).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK_REVIEW_booking_id_BOOKING_booking_id");

            entity.HasOne(d => d.Host).WithMany(p => p.ReviewHosts)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("FK_REVIEW_host_id_USER_user_id");

            entity.HasOne(d => d.Reviewer).WithMany(p => p.ReviewReviewers)
                .HasForeignKey(d => d.ReviewerId)
                .HasConstraintName("FK_REVIEW_reviewer_id_USER_user_id");

            entity.HasOne(d => d.Room).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_REVIEW_room_id_ROOM_LIST_room_id");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__ROLES__760965CC53119F94");

            entity.ToTable("ROLES");

            entity.HasIndex(e => e.RoleCode, "UQ__ROLES__BAE63075897D03F2").IsUnique();

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
            entity.HasKey(e => e.RolePermissionId).HasName("PK__ROLE_PER__B1E85A10C4282F72");

            entity.ToTable("ROLE_PERMISSIONS");

            entity.Property(e => e.RolePermissionId).HasColumnName("role_permission_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.PermissionId).HasColumnName("permission_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ROLE_PERMISSIONS_permission_id_PERMISSIONS_permission_id");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ROLE_PERMISSIONS_role_id_ROLES_role_id");
        });

        modelBuilder.Entity<RoomList>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__ROOM_LIS__19675A8A6929C837");

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
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
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

            entity.HasOne(d => d.Address).WithMany(p => p.RoomLists)
                .HasForeignKey(d => d.AddressId)
                .HasConstraintName("FK_ROOM_LIST_address_id_ADDRESS_address_id");

            entity.HasOne(d => d.Host).WithMany(p => p.RoomLists)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("FK_ROOM_LIST_host_id_USER_user_id");
        });

        modelBuilder.Entity<RoomPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__ROOM_PHO__CB48C83DB1EFBC34");

            entity.ToTable("ROOM_PHOTO");

            entity.HasIndex(e => new { e.RoomId, e.PhotoType, e.SortOrder, e.PhotoId }, "IX_ROOM_PHOTO_Room_Type_Sort");

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
            entity.Property(e => e.PhotoType)
                .HasMaxLength(20)
                .HasColumnName("photo_type");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.SortOrder).HasColumnName("sort_order");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomPhotos)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_ROOM_PHOTO_room_id_ROOM_LIST_room_id");
        });

        modelBuilder.Entity<SubscriptionBillingLog>(entity =>
        {
            entity.HasKey(e => e.BillId).HasName("PK__SUBSCRIP__D706DDB3D64624B9");

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

            entity.HasOne(d => d.HostSub).WithMany(p => p.SubscriptionBillingLogs)
                .HasForeignKey(d => d.HostSubId)
                .HasConstraintName("FK_SUBSCRIPTION_BILLING_LOG_host_sub_id_HOST_SUBSCRIPTION_host_sub_id");
        });

        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.HasKey(e => e.PlanId).HasName("PK__SUBSCRIP__BE9F8F1DE6670852");

            entity.ToTable("SUBSCRIPTION_PLAN");

            entity.Property(e => e.PlanId).HasColumnName("plan_id");
            entity.Property(e => e.CommissionRate)
                .HasColumnType("decimal(18, 4)")
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
            entity.HasKey(e => e.SupportTicketsId).HasName("PK__SUPPORT___494F3323155A4E86");

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

            entity.HasOne(d => d.AssignedStaff).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.AssignedStaffId)
                .HasConstraintName("FK_SUPPORT_TICKETS_assigned_staff_id_USER_user_id");

            entity.HasOne(d => d.RelatedFeedback).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.RelatedFeedbackId)
                .HasConstraintName("FK_SUPPORT_TICKETS_related_feedback_id_FAQ_ARTICLES_faq_articles_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__USER__B9BE370FD682ED20");

            entity.ToTable("USER");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(512)
                .HasColumnName("address");
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
            entity.HasKey(e => e.FavoriteId).HasName("PK__USER_FAV__46ACF4CB02214EC0");

            entity.ToTable("USER_FAVORITE_REPORT");

            entity.Property(e => e.FavoriteId).HasColumnName("favorite_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ReportParams).HasColumnName("report_params");
            entity.Property(e => e.ReportType)
                .HasMaxLength(50)
                .HasColumnName("report_type");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserFavoriteReports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_USER_FAVORITE_REPORT_user_id_USER_user_id");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PK__USER_ROL__B8D9ABA2CAB900DF");

            entity.ToTable("USER_ROLES");

            entity.Property(e => e.UserRoleId).HasColumnName("user_role_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("created_at");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLES_role_id_ROLES_role_id");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_USER_ROLES_user_id_USER_user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
