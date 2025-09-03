INSERT INTO dbo.PERMISSIONS (perm_code, perm_name, module, action, description) VALUES
-- 一般瀏覽
('site.view',                   N'瀏覽網站',                 'site',         'view',     N'可瀏覽網站基本頁面'),
('listing.view',                N'瀏覽房源資訊',             'listing',      'view',     N'可檢視房源列表與詳細'),

-- 房客功能
('booking.create',              N'建立訂房',                 'booking',      'create',   N'建立訂單/預約'),
('chat.use',                    N'使用聊天室/問答',           'chat',         'use',      N'房東問答或客服聊天'),
('support.use',                 N'使用客服系統',             'support',      'use',      N'透過客服回報/提問'),
('points.use',                  N'累積與使用點數',           'points',       'use',      N'點數累積、查詢、兌換'),
('coupon.use',                  N'使用優惠券',               'coupon',       'use',      N'輸入/套用優惠券'),

-- 房東功能
('host.booking.view_guest',     N'檢視訂房房客資訊',         'host',         'view',     N'可見訂單的房客聯絡/識別資訊(依隱私設定)'),
('host.listing.create',         N'新增房型',                 'listing',      'create',   N'新增房型/方案'),
('host.listing.update',         N'修改房型',                 'listing',      'update',   N'編輯房型/方案'),
('host.listing.delete',         N'刪除房型',                 'listing',      'delete',   N'刪除房型/方案'),
('host.listing.publish',        N'上架房型',                 'listing',      'publish',  N'將房型公開'),
('host.listing.unpublish',      N'下架房型',                 'listing',      'unpublish',N'將房型隱藏'),
('chat.reply',                  N'回覆問答/聊天室訊息',       'chat',         'update',   N'回覆訪客/房客提問'),
('subscription.manage',         N'管理房東訂閱方案',         'subscription', 'manage',   N'訂閱/取消/升降級；影響上架費與上架時長'),

-- 廠商功能
('supplier.post.view_post',	   N'檢視公告資訊',			  'supplier',	  'view',		N'可查看公告欄中的廠商服務資訊'),
('supplier.post.create',		   N'新增服務公告',			  'posting',	  'create',		N'新增服務公告'),
('supplier.post.update',		   N'修改服務公告',			  'posting',	  'update',		N'編輯服務公告'),
('supplier.post.delete',		   N'刪除服務公告',			  'posting',	  'delete',		N'刪除服務公告'),
('supplier.post.publish',	   N'上架服務公告',			  'posting',	  'publish',	N'公開服務公告'),
('supplier.post.unpublish',	   N'下架服務公告',			  'posting',	  'unpublish',	N'隱藏服務公告'),



-- 後台/系統管理（儀表板、資料、會員）
('admin.dashboard.view',        N'後台儀表板檢視',            'admin',        'view',     N'登入後台與查看總覽'),
('admin.analytics.view',        N'營運數據分析檢視',          'admin',        'analytics',N'當月/季/年度訂房數據等'),
('admin.listing.manage',        N'管理房源資料',             'admin',        'manage',   N'增刪改查/上下架/標記'),
('admin.order.approve',         N'訂單/審核作業',            'admin',        'approve',  N'人工審核或流程節點'),
('admin.property_notice.manage',N'物業管理公告與審核',        'admin',        'manage',   N'公告/審核/上下架'),
('admin.member.manage',         N'會員管理',                 'admin',        'manage',   N'所有會員的增刪改查/停權'),

-- 超級權限（可視為保險絲）
('admin.all',                   N'後台最高權限',             'admin',        'manage',   N'對所有後台資源具有最高操作權');
