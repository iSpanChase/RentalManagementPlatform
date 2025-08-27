
-- related_article_id 是 參考的FAQ可空 0~11
-- created_by_user_id 是 是哪位使用者 可空 0~199
-- assigned_staff_id 是 指派對象(員工) 可空FK(staff_users.id)
-- subject 主旨
-- content 問題描述
-- contact_email 聯絡email
-- priority 重要姓
-- source 來源 (faq_form,phone)
INSERT INTO SUPPORT_TICKETS
(related_article_id, created_by_user_id, assigned_staff_id,
 subject, content, contact_email, priority, source, status, created_at, updated_at)
VALUES
-- 旅客：預訂問題
(1, 11, 201,
 N'無法完成預訂流程',
 N'我在選擇日期後點擊付款，卻顯示「系統錯誤」。請協助確認。',
 N'user11@example.com', N'high', N'faq_form', N'open', SYSDATETIME(), SYSDATETIME()),

-- 旅客：退款爭議
(2, 12, 202,
 N'取消訂單未收到退款',
 N'我依照規則在期限內取消，但退款遲遲未到帳，請協助處理。',
 N'user12@example.com', N'urgent', N'faq_form', N'in_progress', SYSDATETIME(), SYSDATETIME()),

-- 旅客：入住問題
(3, 13, 203,
 N'入住當天無法進入房間',
 N'我依照入住指南輸入密碼，門鎖卻無法開啟，房東也無回覆。',
 N'user13@example.com', N'urgent', N'phone', N'resolved', SYSDATETIME(), SYSDATETIME()),

-- 房東：刊登問題
(4, 14, 204,
 N'房源刊登失敗',
 N'我上傳照片後，系統顯示「未知錯誤」，無法完成刊登。',
 N'host14@example.com', N'normal', N'faq_form', N'open', SYSDATETIME(), SYSDATETIME()),

-- 房東：定價問題
(5, 15, 205,
 N'最低入住天數無法更改',
 N'我嘗試修改最低入住天數，系統顯示「設定失敗」。',
 N'host15@example.com', N'high', N'faq_form', N'open', SYSDATETIME(), SYSDATETIME()),

-- 帳號安全：忘記密碼
(6, 16, 206,
 N'忘記密碼無法重設',
 N'我收不到重設密碼的信件，已經嘗試三次。請幫忙檢查。',
 N'user16@example.com', N'normal', N'faq_form', N'closed', SYSDATETIME(), SYSDATETIME()),

-- 帳號安全：釣魚詐騙
(7, 17, 207,
 N'收到釣魚郵件',
 N'我收到假冒 Airbnb 的郵件，內含可疑連結，已附上截圖。',
 N'user17@example.com', N'high', N'email', N'in_progress', SYSDATETIME(), SYSDATETIME()),

-- 客服：聯繫問題
(8, 18, 208,
 N'無法聯繫到客服',
 N'我在 APP 內點選客服，卻顯示錯誤訊息，無法提交案件。',
 N'user18@example.com', N'normal', N'faq_form', N'open', SYSDATETIME(), SYSDATETIME()),

-- 客服：退款進度
 (9, 19, 209,
 N'退款進度查詢',
 N'我的退款已超過 7 個工作天，想知道目前處理狀態。',
 N'user19@example.com', N'high', N'email', N'in_progress', SYSDATETIME(), SYSDATETIME()),

-- FAQ 反饋：文章錯誤
(10, 20, 210,
 N'FAQ 內容錯誤回報',
 N'FAQ 上寫退款 3 天內完成，但實際需要 7~10 天，建議修正。',
 N'user20@example.com', N'low', N'faq_form', N'resolved', SYSDATETIME(), SYSDATETIME()),
 (11, 21, 211,
 N'FAQ 回饋未被處理',
 N'我在 FAQ 留下 👎 回饋，但沒有收到任何後續通知，想確認是否有客服跟進。',
 N'user21@example.com', N'normal', N'faq_form', N'open', SYSDATETIME(), SYSDATETIME());

 INSERT INTO [SUPPORT_TICKETS] 
(related_article_id, created_by_user_id, assigned_staff_id, subject, content, contact_email, priority, source, status, created_at, updated_at)
VALUES
(null, 201, null, N'無法登入帳號', N'我嘗試登入多次，但系統一直顯示帳號或密碼錯誤。', N'user1@example.com', N'High', N'phone', N'Open', '2025-08-01 10:20:00', '2025-08-01 10:20:00'),
(null, 202, null, N'付款失敗問題', N'信用卡付款一直被拒絕，但卡片沒有問題。', N'user2@example.com', N'Medium', N'phone', N'In Progress', '2025-08-02 14:35:00', '2025-08-02 16:00:00'),
(null, null, 303, N'FAQ 文章無法開啟', N'想查看退款流程的 FAQ，但頁面顯示 404。', N'user3@example.com', N'Low', N'phone', N'Resolved', '2025-08-03 09:15:00', '2025-08-04 11:20:00'),
(null, null, 301, N'需要更改訂單資料', N'下單時填錯地址，想修改收件資訊。', N'user4@example.com', N'High', N'faq_form', N'Pending', '2025-08-04 19:40:00', '2025-08-04 19:40:00'),
(null, 205, null, N'App 一直閃退', N'每次打開 App 都會自動關閉，無法使用。', N'user5@example.com', N'Critical', N'phone', N'Open', '2025-08-05 08:50:00', '2025-08-05 08:50:00'),
(null, null, null, N'訂閱退費問題', N'我想取消訂閱並退款，請問要怎麼處理？', N'user6@example.com', N'Medium', N'phone', N'In Progress', '2025-08-06 11:25:00', '2025-08-06 13:10:00'),
(null, 207, 303, N'客服回覆太慢', N'我之前已經送出問題，但一直沒有人回覆。', N'user7@example.com', N'High', N'faq_form', N'Open', '2025-08-07 15:00:00', '2025-08-07 15:00:00'),
(null, null, null, N'FAQ 內容過期', N'常見問題裡的退貨規則跟目前政策不一致。', N'user8@example.com', N'Low', N'phone', N'Resolved', '2025-08-08 17:30:00', '2025-08-09 09:00:00'),
(null, 209, 304, N'無法收到驗證信', N'我註冊後沒有收到驗證信，已經等了半小時。', N'user9@example.com', N'Medium', N'phone', N'Pending', '2025-08-09 12:10:00', '2025-08-09 12:10:00'),
(null, null, 302, N'密碼重設失敗', N'嘗試使用忘記密碼功能，但收不到重設連結。', N'user10@example.com', N'High', N'phone', N'In Progress', '2025-08-10 21:45:00', '2025-08-10 22:10:00'),
(null, 211, 303, N'廣告信件過多', N'每天收到太多廣告信件，想取消通知。', N'user11@example.com', N'Low', N'faq_form', N'Open', '2025-08-11 09:10:00', '2025-08-11 09:10:00'),
(2, 212, 301, N'訂單無法查詢', N'輸入訂單編號後系統顯示查無此單。', N'user12@example.com', N'High', N'phone', N'Pending', '2025-08-12 16:25:00', '2025-08-12 16:25:00'),
(3, 213, 304, N'退款延遲', N'申請退款已經超過兩週還沒收到。', N'user13@example.com', N'Medium', N'phone', N'In Progress', '2025-08-13 11:45:00', '2025-08-13 13:00:00'),
(4, 214, 302, N'無法修改密碼', N'嘗試更改密碼後，系統提示錯誤。', N'user14@example.com', N'High', N'phone', N'Resolved', '2025-08-14 14:10:00', '2025-08-15 09:00:00'),
(5, null, 303, N'收不到推播通知', N'我已經開啟通知，但手機沒有收到任何提醒。', N'user15@example.com', N'Medium', N'phone', N'Open', '2025-08-15 08:30:00', '2025-08-15 08:30:00'),
(6, null, 301, N'FAQ 格式錯誤', N'FAQ 裡面的排版亂掉，看不清楚內容。', N'user16@example.com', N'Low', N'phone', N'Pending', '2025-08-16 18:15:00', '2025-08-16 18:15:00'),
(7, 217, 304, N'APP 更新後異常', N'更新到最新版本後，APP 開啟變得很慢。', N'user17@example.com', N'Medium', N'phone', N'In Progress', '2025-08-17 12:20:00', '2025-08-17 13:10:00'),
(8, 218, 302, N'聯絡客服失敗', N'按下聯絡客服按鈕後沒有任何反應。', N'user18@example.com', N'High', N'phone', N'Open', '2025-08-18 09:45:00', '2025-08-18 09:45:00'),
(9, 219, 303, N'重複扣款', N'同一筆交易被扣了兩次款項。', N'user19@example.com', N'Critical', N'faq_form', N'In Progress', '2025-08-19 20:10:00', '2025-08-19 20:50:00'),
(null, 220, 301, N'Email 無法發送', N'系統寄出的 Email 全部都進垃圾郵件。', N'user20@example.com', N'Medium', N'phone', N'Resolved', '2025-08-20 15:00:00', '2025-08-21 09:00:00'),
(null, 221, null, N'登入驗證問題', N'登入時需要二次驗證，但無法收到簡訊。', N'user21@example.com', N'High', N'faq_form', N'Pending', '2025-08-21 10:40:00', '2025-08-21 10:40:00'),
(null, 222, 302, N'修改付款方式失敗', N'想更換信用卡付款，但一直顯示錯誤。', N'user22@example.com', N'Medium', N'phone', N'In Progress', '2025-08-22 12:55:00', '2025-08-22 14:00:00'),
(3, 223, 303, N'訂閱方案顯示錯誤', N'App 顯示的訂閱價格與網站不同。', N'user23@example.com', N'Low', N'faq_form', N'Open', '2025-08-23 09:05:00', '2025-08-23 09:05:00'),
(4, 224, 301, N'客服回覆內容錯誤', N'客服提供的解法完全不適用。', N'user24@example.com', N'High', N'phone', N'Resolved', '2025-08-24 11:30:00', '2025-08-25 08:00:00'),
(5, 225, 304, N'常見問題找不到', N'我需要的問題找不到相關 FAQ 文章。', N'user25@example.com', N'Low', N'phone', N'Open', '2025-08-25 19:15:00', '2025-08-25 19:15:00'),
(6, null, 302, N'付款成功但訂單未建立', N'款項已扣，但訂單紀錄沒有生成。', N'user26@example.com', N'Critical', N'faq_form', N'In Progress', '2025-08-26 08:50:00', '2025-08-26 09:20:00'),
(null, 227, 303, N'FAQ 搜尋無法使用', N'輸入關鍵字卻顯示「無結果」。', N'user27@example.com', N'Medium', N'phone', N'Pending', '2025-08-26 14:30:00', '2025-08-26 14:30:00'),
(8, 228, 301, N'收不到發票', N'購買後沒有收到電子發票 Email。', N'user28@example.com', N'High', N'phone', N'Open', '2025-08-27 10:25:00', '2025-08-27 10:25:00'),
(9, 229, 304, N'推播通知錯誤', N'收到的推播內容顯示亂碼。', N'user29@example.com', N'Low', N'faq_form', N'Resolved', '2025-08-27 18:40:00', '2025-08-28 09:00:00'),
(null, 230, 302, N'帳號被鎖定', N'登入錯誤太多次，帳號已被鎖定，無法使用。', N'user30@example.com', N'Critical', N'phone', N'In Progress', '2025-08-28 22:15:00', '2025-08-28 22:45:00');
