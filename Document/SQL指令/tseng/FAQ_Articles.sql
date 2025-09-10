-- slug 是 給前端url使用的
-- category_id 所屬哪個分類的
-- author_id 作者可空
-- title 問題
-- summary 摘要
-- content 內文
-- is_pinned 是否置頂
-- published_at 發布時間
-- view_count 瀏覽次數
-- helpful_yes 有幫助票數
-- helpful_no 沒幫助票數
-- created_at 建立時間
-- updated_at 更新時間

INSERT INTO faq_articles
(slug, category_id, author_id, title, summary, content, is_pinned, published_at, view_count, helpful_yes, helpful_no, status, created_at, updated_at)
VALUES
-- 訂單／入住（旅客端）
(N'how-to-book', 6, -1,
 N'如何預訂住宿？',
 N'教您如何透過搜尋與日曆完成下單流程。',
 N'在搜尋頁輸入入住與退房日期、入住人數，系統會顯示可用的房源。選擇後即可進行線上付款完成預訂。',
 1, SYSDATETIME(), 120, 85, 5, N'published', SYSDATETIME(), SYSDATETIME()),

(N'cancellation-policy', 8, -1,
 N'我可以取消預訂嗎？',
 N'了解不同取消政策與退款條件。',
 N'是否能全額退款取決於房東的取消政策，您可以在預訂頁面查看並選擇符合需求的政策。',
 0, SYSDATETIME(), 98, 60, 12, N'published', SYSDATETIME(), SYSDATETIME()),

(N'checkin-guide', 9, -1,
 N'入住當天如何取得鑰匙？',
 N'房東會提供自助入住或面交方式。',
 N'多數房東會在入住前提供自助入住方式（如密碼鎖、鑰匙盒），或與您直接約時間交接。請查看「入住指南」。',
 0, SYSDATETIME(), 150, 90, 8, N'archived', SYSDATETIME(), SYSDATETIME()),

-- 房東端
(N'listing-setup-guide', 10, -1,
 N'如何刊登房源？',
 N'房東可以透過平台刊登房源並上傳照片。',
 N'點擊右上角「成為房東」後，依照步驟填寫房源資訊、照片、價格及政策，完成即可上架。',
 1, SYSDATETIME(), 110, 70, 3, N'published', SYSDATETIME(), SYSDATETIME()),

(N'calendar-pricing', 11, -1,
 N'如何設定最低入住天數？',
 N'可在日曆與定價頁面進行設定。',
 N'您可以在「價格與日曆」設定中調整最短與最長入住天數，確保符合個人接待需求。',
 0, SYSDATETIME(), 76, 45, 6, N'draft', SYSDATETIME(), SYSDATETIME()),

-- 帳號安全
(N'password-reset', 12, -1,
 N'我忘記密碼了怎麼辦？',
 N'透過註冊信箱可重設密碼。',
 N'請在登入頁點擊「忘記密碼」，系統會寄送重設連結至您的註冊信箱。',
 0, SYSDATETIME(), 132, 88, 4, N'published', SYSDATETIME(), SYSDATETIME()),

(N'phishing-warning', 13, -1,
 N'如何辨識釣魚網站？',
 N'小心假連結與非官方付款方式。',
 N'請僅透過 Airbnb 官方網站或 APP 付款，切勿點擊陌生連結或在平台外轉帳。',
 0, SYSDATETIME(), 95, 72, 10, N'published', SYSDATETIME(), SYSDATETIME()),

-- 客服與糾紛
(N'contact-support', 14, -1,
 N'如何聯繫客服？',
 N'提供 24 小時的線上客服支援。',
 N'您可以透過「幫助中心」或 APP 聯繫客服，部分地區支援 24 小時即時回覆。',
 1, SYSDATETIME(), 160, 100, 9, N'published', SYSDATETIME(), SYSDATETIME()),

(N'refund-time', 15, -1,
 N'退款需要多久？',
 N'退款速度依付款方式不同。',
 N'退款通常會在 5-10 個工作天內完成，實際時間取決於您的付款方式與銀行。',
 0, SYSDATETIME(), 123, 80, 15, N'published', SYSDATETIME(), SYSDATETIME()),

-- FAQ + 反饋流程
(N'how-to-use-faq', 16, -1,
 N'如何使用FAQ？',
 N'透過分類或搜尋快速找到答案。',
 N'請在搜尋框輸入關鍵字，或透過分類導覽找到對應的問題。',
 0, SYSDATETIME(), 50, 30, 2, N'published', SYSDATETIME(), SYSDATETIME()),

(N'article-feedback', 17, -1,
 N'文章有幫助嗎？',
 N'您可以點擊 👍 或 👎 提供回饋。',
 N'點擊「有幫助」或「沒幫助」按鈕，即可快速留下您的意見，幫助我們改善內容。',
 0, SYSDATETIME(), 64, 42, 5, N'published', SYSDATETIME(), SYSDATETIME());
