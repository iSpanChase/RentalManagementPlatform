/* INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id) */

/* 訪客（未登入） */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN ('site.view','listing.view')
WHERE r.role_code = 'GUEST';

/* 房客會員 */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN (
    'site.view','listing.view',
    'booking.create','chat.use','support.use',
    'points.use','coupon.use'
)
WHERE r.role_code = 'TENANT';

/* 房東會員 */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN (
    'site.view','listing.view',
    'host.booking.view_guest',
    'host.listing.create','host.listing.update','host.listing.delete',
    'host.listing.publish','host.listing.unpublish',
    'chat.reply','subscription.manage'
)
WHERE r.role_code = 'HOST';

/* 廠商會員 */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN (
    'supplier.post.view_post',
    'supplier.post.create','supplier.post.update','supplier.post.delete',
    'supplier.post.publish','supplier.post.unpublish'
)
WHERE r.role_code = 'SUPPLIER';

/* 系統管理員（給完整後台權限；是否要同時擁TENANT/HOST 功能依政策而定） */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN (
    'admin.dashboard.view','admin.analytics.view',
    'admin.listing.manage','admin.order.approve',
    'admin.property_notice.manage','admin.member.manage',
    'admin.all'
)
WHERE r.role_code = 'OPERATOR';

/* 最高權限系統管理員（給完整後台權限；是否要同時擁TENANT/HOST 功能依政策而定） */
INSERT INTO dbo.ROLE_PERMISSIONS (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM dbo.Roles r
JOIN dbo.Permissions p ON p.perm_code IN (
    'admin.dashboard.view','admin.analytics.view',
    'admin.listing.manage','admin.order.approve',
    'admin.property_notice.manage','admin.member.manage',
    'admin.all'
)
WHERE r.role_code = 'ADMIN';
