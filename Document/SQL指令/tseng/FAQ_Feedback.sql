-- article_id 是 哪一篇FAQ
-- sentiment 有幫助 沒幫助
-- reason 原因
-- contact_email 聯繫的email
-- escalated_to_ticket 是否升級成工單

INSERT INTO faq_feedback
(article_id, user_id, sentiment, reason, contact_email, escalated_to_ticket, created_at)
VALUES
(1, -1, N'yes', N'流程清楚，快速完成預訂。', N'user11@example.com', 0, SYSDATETIME()),
(2, -1, N'no', N'取消政策太嚴格，無法全額退款。', N'user12@example.com', 1, SYSDATETIME()),
(3, -1, N'yes', N'入住指南很詳細，順利拿到鑰匙。', N'user13@example.com', 0, SYSDATETIME()),
(4, -1, N'yes', N'刊登房源步驟簡單，成功上架。', N'host14@example.com', 0, SYSDATETIME()),
(5, -1, N'no', N'無法設定每週不同的最低入住天數。', N'host15@example.com', 1, SYSDATETIME()),
(6, -1, N'yes', N'重設密碼流程簡單，馬上解決問題。', N'user16@example.com', 0, SYSDATETIME()),
(7, -1, N'no', N'收到疑似釣魚郵件，想要更多防護建議。', N'user17@example.com', 1, SYSDATETIME()),
(8, -1, N'yes', N'客服回覆很快，解決了我的入住問題。', N'user18@example.com', 0, SYSDATETIME()),
(9, -1, N'no', N'退款等太久，已經超過 10 天還沒收到。', N'user19@example.com', 1, SYSDATETIME()),
(10, -1, N'no', N'FAQ 搜尋功能還行，但關鍵字匹配度不高。', N'user20@example.com', 0, SYSDATETIME()),
(11, -1, N'yes', N'👍 功能很方便，回饋很快。', N'user21@example.com', 0, SYSDATETIME()),

(1, -1, N'positive', N'訂房流程容易理解，畫面清晰。', N'user21@example.com', 0, SYSDATETIME()),
(2, -1, N'negative', N'政策太複雜，看不懂哪一條適用。', N'user22@example.com', 1, SYSDATETIME()),
(3, -1, N'neutral', N'房東提供的密碼鎖有點舊，但文章說明還算清楚。', N'user23@example.com', 0, SYSDATETIME()),
(4, -1, N'positive', N'刊登房源成功，指引很有幫助。', N'host24@example.com', 0, SYSDATETIME()),
(5, -1, N'negative', N'價格設定方式太死板，想要更多彈性。', N'host25@example.com', 1, SYSDATETIME()),
(6, -1, N'positive', N'收到重設密碼的信件，馬上解決登入問題。', N'user26@example.com', 0, SYSDATETIME()),
(7, -1, N'negative', N'被釣魚網站騙過，希望有更明確的警示。', N'user27@example.com', 1, SYSDATETIME()),
(8, -1, N'positive', N'客服回應快速，24 小時內解決。', N'user28@example.com', 0, SYSDATETIME()),
(9, -1, N'negative', N'退款等待時間太長，已經超過兩週。', N'user29@example.com', 1, SYSDATETIME()),
(10, -1, N'neutral', N'搜尋功能普通，輸入錯字就找不到結果。', N'user30@example.com', 0, SYSDATETIME()),
(11, -1, N'positive', N'👍 回饋機制很方便。', N'user31@example.com', 0, SYSDATETIME()),

(1, -1, N'positive', N'很快就能找到符合需求的住宿。', N'user32@example.com', 0, SYSDATETIME()),
(2, -1, N'negative', N'我取消預訂卻只退一半，覺得不合理。', N'user33@example.com', 1, SYSDATETIME()),
(3, -1, N'positive', N'入住資訊正確，流程順利。', N'user34@example.com', 0, SYSDATETIME()),
(4, -1, N'neutral', N'刊登功能不錯，但圖片上傳有點慢。', N'host35@example.com', 0, SYSDATETIME()),
(5, -1, N'negative', N'希望可以設定不同日期的價格規則。', N'host36@example.com', 1, SYSDATETIME()),
(6, -1, N'positive', N'重設密碼速度快，沒有問題。', N'user37@example.com', 0, SYSDATETIME()),
(7, -1, N'negative', N'希望有更多提示避免釣魚詐騙。', N'user38@example.com', 1, SYSDATETIME()),
(8, -1, N'positive', N'客服態度很好，很滿意。', N'user39@example.com', 0, SYSDATETIME()),
(9, -1, N'negative', N'退款太慢，影響使用體驗。', N'user40@example.com', 1, SYSDATETIME()),
(10, -1, N'neutral', N'FAQ 有些文章過時，希望能更新。', N'user41@example.com', 0, SYSDATETIME()),
(11, -1, N'positive', N'回饋功能簡單又直覺。', N'host24@example.com', 0, SYSDATETIME()),

-- 訂單／入住 (article_id: 1001~1003)
(1, -1, N'positive', N'預訂過程很順利。', N'user81@example.com', 0, SYSDATETIME()),
(1, -1, N'negative', N'搜尋結果太少。', N'user82@example.com', 0, SYSDATETIME()),
(2, -1, N'negative', N'退款條款太嚴格。', N'user83@example.com', 1, SYSDATETIME()),
(2, -1, N'neutral', N'取消政策描述不清楚。', N'user84@example.com', 0, SYSDATETIME()),
(3, -1, N'positive', N'入住指南清楚易懂。', N'user85@example.com', 0, SYSDATETIME()),
(3, -1, N'negative', N'房東沒依照入住說明操作。', N'user86@example.com', 1, SYSDATETIME()),
(1, -1, N'positive', N'找到房源很快。', N'user87@example.com', 0, SYSDATETIME()),
(2, -1, N'negative', N'取消後退款時間過長。', N'user88@example.com', 1, SYSDATETIME()),
(3, -1, N'neutral', N'入住時間有點混亂，但可接受。', N'user89@example.com', 0, SYSDATETIME()),
(1, -1, N'positive', N'平台操作簡單。', N'user90@example.com', 0, SYSDATETIME()),

-- 房東端 (article_id: 1004~1005)
(4, -1, N'positive', N'刊登操作很順利。', N'host91@example.com', 0, SYSDATETIME()),
(4, -1, N'neutral', N'圖片上傳速度普通。', N'host92@example.com', 0, SYSDATETIME()),
(4, -1, N'negative', N'規則設定選項不足。', N'host93@example.com', 1, SYSDATETIME()),
(5, -1, N'positive', N'日曆設定直覺。', N'host94@example.com', 0, SYSDATETIME()),
(5, -1, N'negative', N'無法針對假日調整價格。', N'host95@example.com', 1, SYSDATETIME()),
(5, -1, N'positive', N'最短入住天數設定方便。', N'host96@example.com', 0, SYSDATETIME()),
(4, -1, N'negative', N'刊登時常遇到錯誤。', N'host97@example.com', 1, SYSDATETIME()),
(5, -1, N'neutral', N'功能尚可，但需要更多彈性。', N'host98@example.com', 0, SYSDATETIME()),
(4, -1, N'positive', N'刊登後馬上有訂單。', N'host99@example.com', 0, SYSDATETIME()),
(5, -1, N'positive', N'價格設定功能符合需求。', N'host100@example.com', 0, SYSDATETIME()),

-- 帳號安全 (article_id: 1006~1007)
(6, -1, N'positive', N'重設密碼流程簡單。', N'user101@example.com', 0, SYSDATETIME()),
(6, -1, N'negative', N'忘記密碼信件延遲。', N'user102@example.com', 1, SYSDATETIME()),
(6, -1, N'neutral', N'介面普通，但能解決問題。', N'user103@example.com', 0, SYSDATETIME()),
(7, -1, N'negative', N'詐騙提醒不夠明顯。', N'user104@example.com', 1, SYSDATETIME()),
(7, -1, N'positive', N'釣魚防護資訊清楚。', N'user105@example.com', 0, SYSDATETIME()),
(6, -1, N'positive', N'快速恢復帳號使用。', N'user106@example.com', 0, SYSDATETIME()),
(7, -1, N'neutral', N'提醒訊息有點太頻繁。', N'user107@example.com', 0, SYSDATETIME()),
(7, -1, N'negative', N'被詐騙後客服協助不足。', N'user108@example.com', 1, SYSDATETIME()),
(6, -1, N'positive', N'登入驗證安全可靠。', N'user109@example.com', 0, SYSDATETIME()),
(7, -1, N'positive', N'提供了很有用的安全建議。', N'user110@example.com', 0, SYSDATETIME()),

-- 客服與糾紛 (article_id: 1008~1009)
(8, -1, N'positive', N'客服人員很快解決了我的問題。', N'user111@example.com', 0, SYSDATETIME()),
(8, -1, N'negative', N'客服等待時間過長。', N'user112@example.com', 1, SYSDATETIME()),
(8, -1, N'neutral', N'客服回覆有點制式化。', N'user113@example.com', 0, SYSDATETIME()),
(9, -1, N'negative', N'退款申請流程過於繁瑣。', N'user114@example.com', 1, SYSDATETIME()),
(9, -1, N'positive', N'退款時間合理。', N'user115@example.com', 0, SYSDATETIME()),
(9, -1, N'negative', N'退款金額不完整。', N'user116@example.com', 1, SYSDATETIME()),
(8, -1, N'positive', N'客服態度非常好。', N'user117@example.com', 0, SYSDATETIME()),
(9, -1, N'neutral', N'退款速度普通，可以接受。', N'user118@example.com', 0, SYSDATETIME()),
(8, -1, N'positive', N'24 小時客服很方便。', N'user119@example.com', 0, SYSDATETIME()),
(9, -1, N'negative', N'退款進度查詢不清楚。', N'user120@example.com', 1, SYSDATETIME()),

-- FAQ + 反饋流程 (article_id: 1010~1011)
(10, -1, N'positive', N'FAQ 搜尋功能實用。', N'user121@example.com', 0, SYSDATETIME()),
(10, -1, N'neutral', N'有些文章搜尋不到。', N'user122@example.com', 0, SYSDATETIME()),
(10, -1, N'negative', N'搜尋結果不精準。', N'user123@example.com', 1, SYSDATETIME()),
(11, -1, N'positive', N'回饋機制簡單直覺。', N'user124@example.com', 0, SYSDATETIME()),
(11, -1, N'positive', N'👍 很方便。', N'user125@example.com', 0, SYSDATETIME()),
(11, -1, N'negative', N'按了👎沒有後續追蹤。', N'user126@example.com', 1, SYSDATETIME()),
(10, -1, N'neutral', N'搜尋介面可以再優化。', N'user127@example.com', 0, SYSDATETIME()),
(11, -1, N'positive', N'我常用這個功能，覺得很方便。', N'user128@example.com', 0, SYSDATETIME()),
(10, -1, N'negative', N'搜尋速度太慢。', N'user129@example.com', 1, SYSDATETIME()),
(11, -1, N'positive', N'匿名回饋，覺得有幫助。', NULL, 0, SYSDATETIME());
