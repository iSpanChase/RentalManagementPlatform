-- orders_delete_data.sql
SET NOCOUNT ON;
BEGIN TRY
    BEGIN TRAN;

    -- 清空子表 BOOKING_GUEST
    IF OBJECT_ID('dbo.BOOKING_GUEST','U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.BOOKING_GUEST;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.BOOKING_GUEST'))
            DBCC CHECKIDENT ('dbo.BOOKING_GUEST', RESEED, 0);
    END

    -- 清空主表 BOOKING
    IF OBJECT_ID('dbo.BOOKING','U') IS NOT NULL
    BEGIN
        DELETE FROM dbo.BOOKING;
        IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID('dbo.BOOKING'))
            DBCC CHECKIDENT ('dbo.BOOKING', RESEED, 0);
    END

    COMMIT TRAN;
    PRINT N'BOOKING 與 BOOKING_GUEST 資料已清空並重設流水號。';
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE(), @ErrNum INT = ERROR_NUMBER();
    RAISERROR(N'[清空失敗] (%d) %s', 16, 1, @ErrNum, @ErrMsg);
END CATCH;