BEGIN TRAN;

DECLARE @UserId INT = 225;  -- 你要刪的 user_id

-- 先刪關聯表中的記錄
DELETE FROM USER_ROLES
WHERE user_id = @UserId;

-- 若還有其他表也參照 USER（例：RefreshTokens、EmailVerifications…）一併清
DELETE FROM Refresh_Tokens        WHERE user_id = @UserId;
DELETE FROM EmailVerifications   WHERE user_id = @UserId;
-- ... 依你的實際表補上

-- 最後刪 USER
DELETE FROM [USER]
WHERE user_id = @UserId;

COMMIT TRAN;
