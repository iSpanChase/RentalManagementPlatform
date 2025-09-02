-- 切換使用資料庫
USE RentalManagementPlatformSQL;

CREATE TABLE [ADDRESS] (
  [address_id] int PRIMARY KEY NOT NULL,
  [district_id] int,
  [Latitude] decimal(9,6) NOT NULL,
  [Longitude] decimal(9,6) NOT NULL,
  [street] nvarchar(512),
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [BOOKING] (
  [booking_id] int PRIMARY KEY NOT NULL,
  [coupon_id] int,
  [guest_id] int,
  [room_id] int,
  [order_number] nvarchar(512),
  [check_in] DATETIME2,
  [check_out] DATETIME2,
  [total_price] decimal,
  [commission_rate_snapshot] decimal,
  [points_earned] int,
  [points_redeemed] int,
  [status] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [USER] (
  [user_id] int IDENTITY(1,1) PRIMARY KEY NOT NULL,
  [username] nvarchar(512) NOT NULL,
  [email] nvarchar(512) NOT NULL,
  [name] nvarchar(512) NOT NULL,
  [auto_subscribe] BIT,
  [password_hash] nvarchar(512) NOT NULL,
  [gender] nvarchar(512) NOT NULL,
  [birth_date] DATETIME2 NOT NULL,
  [phone] nvarchar(512),
  [address] nvarchar(512) NOT NULL,
  [profile_imageurl] nvarchar(512),
  [point] int,
  [isverified] bit NOT NULL,
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [BOOKING_GUEST] (
  [booking_guest_id] int PRIMARY KEY NOT NULL,
  [booking_id] int,
  [guest_name] nvarchar(512),
  [guest_id_number] nvarchar(512)
)
GO

CREATE TABLE [CITY] (
  [city_id] int PRIMARY KEY NOT NULL,
  [city_name] nvarchar(512)
)
GO

CREATE TABLE [COUPON] (
  [coupon_id] int PRIMARY KEY NOT NULL,
  [coupon_name] nvarchar(512),
  [discount_code] nvarchar(512),
  [description] nvarchar(512),
  [min_rental_period] int,
  [max_rental_period] int,
  [start_rental_period] DATETIME2,
  [end_rental_period] DATETIME2,
  [discount_method] nvarchar(512),
  [discount_quota] decimal,
  [end_at] DATETIME2,
  [low_spend] decimal,
  [start_at] DATETIME2
)
GO

CREATE TABLE [COUPON_GUEST] (
  [coupon_guest_id] int PRIMARY KEY NOT NULL,
  [coupon_id] int,
  [guest_id] int,
  [create_at] DATETIME2,
  [remove_at] DATETIME2
)
GO

CREATE TABLE [COUPON_DISTRICT] (
  [coupon_district_id] int PRIMARY KEY NOT NULL,
  [coupon_id] int,
  [district_id] int
)
GO

CREATE TABLE [DISTRICT] (
  [district_id] int PRIMARY KEY NOT NULL,
  [city_id] int,
  [district_name] nvarchar(512)
)
GO

CREATE TABLE [PAYMENT] (
  [payment_id] int PRIMARY KEY NOT NULL,
  [booking_id] int,
  [amount] decimal,
  [method] nvarchar(512),
  [paid_at] DATETIME2,
  [payment_ref] nvarchar(512),
  [order_number_snapshot] nvarchar(512),
  [status] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [PAYMENT_TRANSACTION] (
  [transaction_id] int PRIMARY KEY NOT NULL,
  [payment_id] int,
  [provider_txn_id] nvarchar(512),
  [response_code] nvarchar(512),
  [provider] nvarchar(512),
  [response_message] nvarchar(512),
  [txn_ref] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [REVIEW] (
  [review_id] int PRIMARY KEY NOT NULL,
  [booking_id] int,
  [host_id] int,
  [reviewer_id] int,
  [room_id] int,
  [comment] nvarchar(512),
  [rating] int,
  [created_at] DATETIME2
)
GO

CREATE TABLE [ROOM_LIST] (
  [room_id] int PRIMARY KEY NOT NULL,
  [address_id] int,
  [host_id] int,
  [title] nvarchar(512),
  [description] nvarchar(512),
  [max_guests] int,
  [price_per_night] decimal,
  [status] nvarchar(512),
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [ROOM_PHOTO] (
  [photo_id] int PRIMARY KEY NOT NULL,
  [room_id] int,
  [sort_order] int,
  [bucket] NVARCHAR(128) NOT NULL DEFAULT N'room-photos',
  [object_key] NVARCHAR(512) NOT NULL,
  [content_type] NVARCHAR(64) NOT NULL DEFAULT N'image/jpeg'
)
GO

CREATE TABLE [ROLES] (
  [role_id] INT IDENTITY(1,1) PRIMARY KEY,
  [role_code] NVARCHAR(50) UNIQUE NOT NULL,
  [role_name] NVARCHAR(100) NOT NULL,
  [description] NVARCHAR(500),
  [created_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
  [updated_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME())
)
GO

CREATE TABLE [PERMISSIONS] (
  [permission_id] INT IDENTITY(1,1) PRIMARY KEY,
  [perm_code] NVARCHAR(100) UNIQUE NOT NULL,
  [perm_name] NVARCHAR(200) NOT NULL,
  [module] NVARCHAR(50) NOT NULL,
  [action] NVARCHAR(50) NOT NULL,
  [description] NVARCHAR(500),
  [created_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
  [updated_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME())
)
GO

CREATE TABLE [ROLE_PERMISSIONS] (
  [role_permission_id] INT PRIMARY KEY NOT NULL IDENTITY(1,1),
  [role_id] INT NOT NULL,
  [permission_id] INT NOT NULL,
  [created_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME()),
)
GO

CREATE TABLE [USER_ROLES] (
  [user_role_id] INT PRIMARY KEY NOT NULL IDENTITY(1,1),
  [user_id] INT NOT NULL,
  [role_id] INT NOT NULL,
  [created_at] DATETIME2 NOT NULL DEFAULT (SYSDATETIME())
)
GO

CREATE TABLE [HOST_PAYOUT] (
  [payout_id] int PRIMARY KEY,
  [host_id] int,
  [cycle_start] DATETIME2,
  [cycle_end] DATETIME2,
  [amount_gross] decimal,
  [platform_fee] decimal,
  [amount_net] decimal,
  [paid_at] datetime2,
  [status] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [HOST_PAYOUT_ITEM] (
  [payout_item_id] int PRIMARY KEY,
  [payout_id] int,
  [booking_id] int,
  [order_number_snapshot] nvarchar(512),
  [amount_gross] decimal,
  [commission_pct] decimal,
  [platform_fee] decimal,
  [amount_net] decimal
)
GO

CREATE TABLE [SUBSCRIPTION_PLAN] (
  [plan_id] int PRIMARY KEY,
  [plan_name] nvarchar(512),
  [monthly_fee] decimal,
  [commission_rate] decimal,
  [perk_priority] BIT,
  [perk_analytics] BIT,
  [is_active] BIT,
  [created_at] DATETIME2
)
GO

CREATE TABLE [HOST_SUBSCRIPTION] (
  [host_sub_id] int PRIMARY KEY,
  [host_id] int,
  [plan_id] int,
  [start_date] DATETIME2,
  [next_billing_date] DATETIME2,
  [cancel_at_period_end] BIT,
  [status] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [SUBSCRIPTION_BILLING_LOG] (
  [bill_id] int PRIMARY KEY,
  [host_sub_id] int,
  [bill_period_start] DATETIME2,
  [bill_period_end] DATETIME2,
  [amount] decimal,
  [paid_status] nvarchar(512),
  [paid_at] DATETIME2,
  [created_at] DATETIME2,
  [note] nvarchar(512)
)
GO

CREATE TABLE [POINT_RULE] (
  [rule_id] int PRIMARY KEY,
  [earn_rate_per_ntd] decimal,
  [max_points_per_order] int,
  [expiry_months] int,
  [redeem_rate_ntd_per_pt] decimal,
  [active_from] DATETIME2,
  [active_to] DATETIME2,
  [is_active] BIT,
  [created_at] DATETIME2
)
GO

CREATE TABLE [POINT_LEDGER] (
  [ledger_id] int PRIMARY KEY,
  [guest_id] int,
  [booking_id] int,
  [order_number_snapshot] nvarchar(512),
  [type] nvarchar(512),
  [points] int,
  [occurred_at] DATETIME2,
  [expires_at] DATETIME2,
  [note] nvarchar(512)
)
GO

CREATE TABLE [FAQ_CATEGORIES] (
  [faq_categories_id] int PRIMARY KEY IDENTITY(1,1),
  [slug] nvarchar(255) UNIQUE,
  [parent_id] int,
  [name] nvarchar(255),
  [description] nvarchar(255),
  [sort_order] int,
  [is_active] BIT,
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [FAQ_ARTICLES] (
  [faq_articles_id] int PRIMARY KEY IDENTITY(1,1),
  [slug] nvarchar(255) UNIQUE,
  [category_id] int,
  [author_id] int,
  [title] nvarchar(255),
  [summary] nvarchar(255),
  [content] nvarchar(512),
  [is_pinned] BIT,
  [published_at] DATETIME2,
  [view_count] int,
  [helpful_yes] int,
  [helpful_no] int,
  [status] nvarchar(255),
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [FAQ_FEEDBACK] (
  [faq_feedback_id] int PRIMARY KEY IDENTITY(1,1),
  [article_id] int,
  [user_id] int,
  [sentiment] nvarchar(255),
  [reason] nvarchar(255),
  [contact_email] nvarchar(255),
  [escalated_to_ticket] BIT,
  [created_at] DATETIME2
)
GO

CREATE TABLE [SUPPORT_TICKETS] (
  [support_tickets_id] int PRIMARY KEY IDENTITY(1,1),
  [related_feedback_id] int,
  [created_by_user_id] int,
  [assigned_staff_id] int,
  [subject] nvarchar(255),
  [content] nvarchar(512),
  [contact_email] nvarchar(255),
  [priority] nvarchar(255),
  [source] nvarchar(255),
  [status] nvarchar(255),
  [created_at] DATETIME2,
  [updated_at] DATETIME2
)
GO

CREATE TABLE [MESSAGE] (
  [message_id] int PRIMARY KEY IDENTITY(1,1),
  [ticket_id] int,
  [booking_id] int,
  [receiver_id] int,
  [room_id] int,
  [sender_id] int,
  [faq_id] int,
  [content] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [CATEGORIES] (
  [categories_id] int PRIMARY KEY IDENTITY(1, 1),
  [name] nvarchar(50) UNIQUE NOT NULL,
  [is_active] BIT DEFAULT (0)
)
GO

CREATE TABLE [POSTS_CATEGORIES] (
  [post_categories_id] int PRIMARY KEY IDENTITY(1, 1),
  [posts_id] int NOT NULL,
  [categories_id] int NOT NULL
)
GO

CREATE TABLE [POSTS] (
  [posts_id] int PRIMARY KEY IDENTITY(1, 1),
  [user_id] int NOT NULL,
  [region_id] int NOT NULL,
  [title] nvarchar(120) NOT NULL,
  [contact_name] nvarchar(100) NOT NULL,
  [content] nvarchar(512) NOT NULL,
  [address] nvarchar(255),
  [contact_phone] nvarchar(30),
  [contact_email] nvarchar(255),
  [contact_note] nvarchar(255),
  [publish_at] DATETIME2,
  [expire_at] DATETIME2,
  [views] int DEFAULT (0),
  [status] nvarchar(512) NOT NULL DEFAULT 'draft',
  [created_at] DATETIME2 NOT NULL,
  [updated_at] DATETIME2 NOT NULL,
  [deleted_at] DATETIME2,
  [proposed_price] decimal
)
GO

CREATE TABLE [USER_FAVORITE_REPORT] (
  [favorite_id] int PRIMARY KEY NOT NULL,
  [user_id] int,
  [report_type] nvarchar(50),
  [report_params] nvarchar(512),
  [created_at] DATETIME2
)
GO

CREATE TABLE [ANOMALY_RULE] (
  [rule_id] int PRIMARY KEY NOT NULL,
  [rule_name] nvarchar(100),
  [target_type] nvarchar(50),
  [condition_expression] nvarchar(512),
  [threshold_value] decimal(10,2),
  [is_active] BIT,
  [created_at] DATETIME2
)
GO

CREATE TABLE [ANOMALY_DETECTION_LOG] (
  [log_id] int PRIMARY KEY NOT NULL,
  [rule_id] int,
  [target_id] int,
  [detected_value] decimal(18,2),
  [expected_value] decimal(18,2),
  [created_at] DATETIME2
)
GO

CREATE TABLE [MONGODB] (
  [mongodb_id] nvarchar(24) PRIMARY KEY,
  [ListingId] int NOT NULL,
  [description] nvarchar(max),
  [ImageUrl] nvarchar(500)
)
GO

CREATE INDEX [POSTS_index_0] ON [POSTS] ("status", "region_id", "created_at")
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '地址 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'address_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '行政區 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'district_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CHECK (Latitude BETWEEN -90 AND 90)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'Latitude';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = 'CHECK (Longitude BETWEEN -180 AND 180)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'Longitude';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '路名與門牌，例如：復興南路一段 390 號 10 樓',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'street';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立紀錄時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最後更新時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ADDRESS',
@level2type = N'Column', @level2name = 'updated_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訂單 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'booking_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '優惠券 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'coupon_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '旅客 ID (FK, role=guest)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'guest_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'room_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '入住時間，例如：2025-02-10 15:00',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'check_in';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '退房時間，例如：2025-02-12 11:00',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'check_out';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '實付總額，含清潔費、服務費、折扣',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'total_price';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訂單狀態：pending / confirmed / cancelled',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '使用者 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'user_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '使用者登入帳號',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'username';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '電子郵件，例如：ming@example.com',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'email';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '姓名，例如：王小明',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '自動訂閱，true/false',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'auto_subscribe';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '雜湊密碼，禁止明碼存放',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'password_hash';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '性別',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'gender';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '生日',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'birth_date';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '手機號碼，例如：09xx-xxx-xxx',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'phone';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '聯絡地址',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'address';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '頭像路徑檔',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'profile_imageurl';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '點數（行銷/客服補償用）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'point';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '是否通過Email驗證',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'isverified';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間，建議存 UTC（台灣時區 +8）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最後更新時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER',
@level2type = N'Column', @level2name = 'updated_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '入住人資料 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING_GUEST',
@level2type = N'Column', @level2name = 'booking_guest_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訂單 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING_GUEST',
@level2type = N'Column', @level2name = 'booking_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '入住人姓名，例如：陳OO',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING_GUEST',
@level2type = N'Column', @level2name = 'guest_name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '身分證字號/居留證，例如：A123456789（可加密/遮罩）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'BOOKING_GUEST',
@level2type = N'Column', @level2name = 'guest_id_number';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '城市 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'CITY',
@level2type = N'Column', @level2name = 'city_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '城市名稱，例如：台北市、台中市',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'CITY',
@level2type = N'Column', @level2name = 'city_name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '優惠券 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'coupon_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '券名，例如：開站早鳥 9 折',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'coupon_name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '券說明，例如：限週末訂單使用',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'description';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最短租期',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'min_rental_period';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最長租期',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'max_rental_period';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '租期區間開始',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'start_rental_period';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '租期區間結束',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'end_rental_period';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '折扣型態：percentage / amount',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'discount_method';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '折扣數值，例如：0.9=9折；200=折200元',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'discount_quota';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '截止時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'end_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最低消費門檻，例如：滿2000才可用',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'low_spend';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '生效時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON',
@level2type = N'Column', @level2name = 'start_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '發放紀錄 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_GUEST',
@level2type = N'Column', @level2name = 'coupon_guest_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '優惠券 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_GUEST',
@level2type = N'Column', @level2name = 'coupon_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '旅客 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_GUEST',
@level2type = N'Column', @level2name = 'guest_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '發放地點 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_DISTRICT',
@level2type = N'Column', @level2name = 'coupon_district_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '優惠券 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_DISTRICT',
@level2type = N'Column', @level2name = 'coupon_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '地區 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'COUPON_DISTRICT',
@level2type = N'Column', @level2name = 'district_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '行政區 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'DISTRICT',
@level2type = N'Column', @level2name = 'district_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '城市 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'DISTRICT',
@level2type = N'Column', @level2name = 'city_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '行政區名稱，例如：大安區、信義區',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'DISTRICT',
@level2type = N'Column', @level2name = 'district_name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '付款紀錄 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'payment_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訂單 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'booking_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '交易金額，單位：TWD',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'amount';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '付款方式，例如：信用卡、ATM、行動支付',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'method';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '完成時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'paid_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '付款狀態：pending / paid / refunded',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT',
@level2type = N'Column', @level2name = 'status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '金流交易流水號',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'transaction_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '付款紀錄 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'payment_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '金流商交易編號',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'provider_txn_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '金流回傳代碼，例如：0000=成功',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'response_code';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '金流商，例如：綠界、藍新',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'provider';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '金流回傳訊息',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'response_message';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'PAYMENT_TRANSACTION',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '評論 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'review_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訂單 ID (FK, 防刷評)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'booking_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房東 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'host_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '評論者 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'reviewer_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'room_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '文字評論，可審核敏感字',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'comment';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '評分，1–5',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'rating';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'REVIEW',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'room_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '地址 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'address_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房東 ID (FK, role=host)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'host_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源名稱，例如：捷運站 3 分鐘溫馨套房',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'title';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源描述，可含設備、附近生活機能',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'description';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最多入住人數，例如：4',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'max_guests';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '每晚價格，單位：TWD，例如：1280.00',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'price_per_night';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '審核/上架狀態：pending / approved / rejected / active / inactive',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'status';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '最後更新時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_LIST',
@level2type = N'Column', @level2name = 'updated_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '照片 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_PHOTO',
@level2type = N'Column', @level2name = 'photo_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_PHOTO',
@level2type = N'Column', @level2name = 'room_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '照片網址，可存雲端路徑',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_PHOTO'
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '排序，數字小者優先',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ROOM_PHOTO',
@level2type = N'Column', @level2name = 'sort_order';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訊息 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'message_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '關聯訂單 (FK，可為空)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'booking_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '收件者 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'receiver_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '房源 ID (FK，可為空)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'room_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '寄件者 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'sender_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '訊息內容，建議做敏感字/個資遮罩',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'content';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'MESSAGE',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = '公告分類（如：高空清洗、管線維修…）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'CATEGORIES';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = '公告分類（如：高空清洗、管線維修…）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'POSTS_CATEGORIES';
GO

EXEC sp_addextendedproperty
@name = N'Table_Description',
@value = '公告主表（刊登/審核/公開/下架的核心）',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'POSTS';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建議金額，例：清潔加價 300 元',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'POSTS',
@level2type = N'Column', @level2name = 'proposed_price';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '收藏報表 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER_FAVORITE_REPORT',
@level2type = N'Column', @level2name = 'favorite_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '使用者 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER_FAVORITE_REPORT',
@level2type = N'Column', @level2name = 'user_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '報表類型，例如：OrderRevenue, RoomStats',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER_FAVORITE_REPORT',
@level2type = N'Column', @level2name = 'report_type';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '報表參數，JSON 格式存查詢條件',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER_FAVORITE_REPORT',
@level2type = N'Column', @level2name = 'report_params';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立紀錄時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'USER_FAVORITE_REPORT',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '規則 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'rule_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '規則名稱，例如：租金高於區域平均 50%',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'rule_name';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '檢測目標類型，例如：Room / Host / Booking',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'target_type';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '檢測條件，以 JSON 定義規則邏輯',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'condition_expression';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '閾值，例：0.5 表示 50%',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'threshold_value';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '是否啟用，1=啟用, 0=停用',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'is_active';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '建立紀錄時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_RULE',
@level2type = N'Column', @level2name = 'created_at';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '檢測紀錄 ID',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'log_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '套用的規則 ID (FK)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'rule_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '檢測目標 ID，依 target_type 可對應 RoomId / user_id',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'target_id';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '實際檢測數值，例如：30000 (實際租金)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'detected_value';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '參考值，例如：20000 (區域平均租金)',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'expected_value';
GO

EXEC sp_addextendedproperty
@name = N'Column_Description',
@value = '檢測產生時間',
@level0type = N'Schema', @level0name = 'dbo',
@level1type = N'Table',  @level1name = 'ANOMALY_DETECTION_LOG',
@level2type = N'Column', @level2name = 'created_at';
GO