/* 1) user_id = 1 固定配 role_id = 6（若已有就略過） */
INSERT INTO dbo.USER_ROLES (user_id, role_id)
SELECT 1, 6
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.USER_ROLES ur WHERE ur.user_id = 1
);

/* 2) user_id = 2~200 各自隨機配 1~5 其中一個角色（若已有就略過） */
;WITH UsersToAssign AS (
    SELECT u.user_id
    FROM dbo.[USER] AS u
    WHERE u.user_id BETWEEN 2 AND 200
),
RolePool AS (
    -- 若要嚴格只從 1~5 挑，直接寫 BETWEEN；也可改成從 Roles 表撈 IsActive=1 等條件
    SELECT r.role_id
    FROM dbo.Roles AS r
    WHERE r.role_id BETWEEN 1 AND 5
),
RandomPick AS (
    SELECT 
        u.user_id,
        (
            SELECT TOP (1) rp.role_id
            FROM RolePool rp
            ORDER BY CHECKSUM(NEWID(), u.user_id, rp.role_id)
        ) AS role_id
    FROM UsersToAssign u
)
INSERT INTO dbo.USER_ROLES (user_id, role_id)
SELECT rp.user_id, rp.role_id
FROM RandomPick rp
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.USER_ROLES ur
    WHERE ur.user_id = rp.user_id
);
