CREATE TABLE [dbo].[ExternalLogins](
    [external_login_id] BIGINT IDENTITY(1,1) PRIMARY KEY,
    [user_id]           INT NOT NULL,
    [provider]          NVARCHAR(50) NOT NULL,     -- 'Google' / 'LINE'
    [provider_user_id]  NVARCHAR(256) NOT NULL,    -- Google sub / LINE userId
    [email]             NVARCHAR(256) NULL,
    [display_name]      NVARCHAR(256) NULL,
    [picture_url]       NVARCHAR(1024) NULL,
    [created_at]        DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UX_ExternalLogins UNIQUE ([provider], [provider_user_id]),
    CONSTRAINT FK_ExternalLogins_User FOREIGN KEY ([user_id]) REFERENCES [dbo].[USER]([user_id]) ON DELETE CASCADE
);