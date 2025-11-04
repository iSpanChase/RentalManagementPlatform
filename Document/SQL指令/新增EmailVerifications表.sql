CREATE TABLE dbo.EmailVerifications (
    token_Id       INT IDENTITY(1,1) PRIMARY KEY,
    user_Id        INT NOT NULL,
    TokenHash     NVARCHAR(200) NOT NULL,
    Expires_At     DATETIME2     NOT NULL,
    IsUsed        BIT           NOT NULL DEFAULT(0),
    created_At     DATETIME2     NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_EmailVerifications_User FOREIGN KEY (user_Id) REFERENCES dbo.[USER](user_id),
    INDEX IX_EmailVerifications_user_Id_IsUsed (user_Id, IsUsed)
);