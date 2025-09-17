-- FAQ_CATEGORIES
INSERT INTO FAQ_CATEGORIES (slug, name, parent_id,created_at, updated_at) VALUES
(N'orders-checkin', N'訂單／入住（旅客）', NULL, SYSDATETIME(), SYSDATETIME()),
(N'hosts', N'房東端', NULL, SYSDATETIME(), SYSDATETIME()),
(N'account-security', N'帳號安全', NULL, SYSDATETIME(), SYSDATETIME()),
(N'support-disputes', N'客服與糾紛', NULL, SYSDATETIME(), SYSDATETIME()),
(N'faq-feedback', N'FAQ＋反饋流程（通用）', NULL, SYSDATETIME(), SYSDATETIME()),

-- 子分類 (訂單/入住)
(N'search-booking', N'搜尋與預訂', 1, SYSDATETIME(), SYSDATETIME()),
(N'payment-receipts', N'付款與收據', 1, SYSDATETIME(), SYSDATETIME()),
(N'cancellation-refund', N'取消與退款', 1, SYSDATETIME(), SYSDATETIME()),
(N'checkin-checkout', N'入住與退房', 1, SYSDATETIME(), SYSDATETIME()),

-- 子分類 (房東端)
(N'listing-setup', N'刊登設定', 2, SYSDATETIME(), SYSDATETIME()),
(N'calendar-pricing', N'日曆與定價', 2, SYSDATETIME(), SYSDATETIME()),

-- 子分類 (帳號安全)
(N'login-verification', N'登入與驗證', 3, SYSDATETIME(), SYSDATETIME()),
(N'payment-fraud', N'付款安全與詐騙', 3, SYSDATETIME(), SYSDATETIME()),

-- 子分類 (客服與糾紛)
(N'contact-support', N'聯繫客服與處理時效', 4, SYSDATETIME(), SYSDATETIME()),
(N'refund-disputes', N'退款爭議', 4, SYSDATETIME(), SYSDATETIME()),

-- 子分類 (FAQ + 反饋流程)
(N'how-to-use-faq', N'如何使用FAQ', 5, SYSDATETIME(), SYSDATETIME()),
(N'article-feedback', N'文章有幫助？', 5, SYSDATETIME(), SYSDATETIME());

