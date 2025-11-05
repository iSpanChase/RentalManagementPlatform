IF NOT EXISTS (SELECT 1 FROM PERMISSIONS WHERE perm_code = 'Roles.Assign')
INSERT INTO PERMISSIONS (perm_code, perm_name, module, action) VALUES ('Roles.Assign', 'Roles.Assign', 'Roles', 'Assign');

-- 1) 先抓 ADMIN 角色的 role_id
DECLARE @RoleId INT =
(
    SELECT TOP (1) role_id
    FROM ROLES
    WHERE role_code = 'ADMIN'
);

IF @RoleId IS NULL
BEGIN
    RAISERROR(N'找不到 RoleCode = ADMIN 的角色', 16, 1);
    RETURN;
END;

-- 2) 用 Email 抓使用者的 user_id（← 注意欄位名稱）
DECLARE @UserId INT =
(
    SELECT TOP (1) user_id
    FROM [USER]
    WHERE email = 'tingyirrrrr@gmail.com'   -- ← 換成你的 Email
);

IF @UserId IS NULL
BEGIN
    RAISERROR(N'找不到指定 Email 的使用者', 16, 1);
    RETURN;
END;

-- 3) 若尚未指派，插入 USER_ROLES（← 注意欄位名稱）
INSERT INTO USER_ROLES (user_id, role_id)
SELECT @UserId, @RoleId
WHERE NOT EXISTS
(
    SELECT 1
    FROM USER_ROLES
    WHERE user_id = @UserId AND role_id = @RoleId
);