CREATE TRIGGER trg_Payment_Log
ON MRPayments.Payments
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DECLARE @Action nvarchar(10)

    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        IF EXISTS (SELECT * FROM deleted)
            SET @Action = 'UPDATE'
        ELSE
            SET @Action = 'INSERT'
    END
    ELSE
        SET @Action = 'DELETE'

    INSERT INTO MRLogs.Payment_Logs (PaymentId, EntityName, Action, OldValues, NewValues, LogDate)
    SELECT 
        CASE 
            WHEN @Action = 'DELETE' THEN deleted.Id 
            ELSE inserted.Id 
        END,
        'Payment',
        @Action,
        CASE 
            WHEN @Action = 'UPDATE' THEN dbo.GetOldValues_Payment(deleted.Id) 
            ELSE NULL 
        END,
        CASE 
            WHEN @Action = 'INSERT' THEN dbo.GetNewValues_Payment(inserted.Id) 
            ELSE dbo.GetNewValues_Payment(deleted.Id) 
        END,
        GETDATE()
    FROM inserted
    FULL OUTER JOIN deleted ON inserted.Id = deleted.Id
END

GO

CREATE FUNCTION dbo.GetOldValues_Payment(@PaymentId uniqueidentifier)
RETURNS nvarchar(max)
AS
BEGIN
    DECLARE @OldValues nvarchar(max)

    SELECT @OldValues = 
        'PaymentStatus: ' + COALESCE(CONVERT(nvarchar(max), deleted.PaymentStatus), 'NULL') + CHAR(13) + CHAR(10) +
        'ApplicationUserId: ' + COALESCE(CONVERT(nvarchar(max), deleted.ApplicationUserId), 'NULL') + CHAR(13) + CHAR(10) +
        'PaymentValuePLN: ' + COALESCE(CONVERT(nvarchar(max), deleted.PaymentValuePLN), 'NULL') + CHAR(13) + CHAR(10)
    FROM deleted
    WHERE deleted.Id = @PaymentId

    RETURN @OldValues
END

GO

CREATE FUNCTION GetNewValues_Payment (@PaymentId uniqueidentifier)
RETURNS nvarchar(max)
AS
BEGIN
    DECLARE @NewValues nvarchar(max)

    SELECT @NewValues = CONCAT(
        'PaymentStatus: ', COALESCE(NEW.PaymentStatus, 'NULL'), CHAR(13), CHAR(10),
        'ApplicationUserId: ', COALESCE(NEW.ApplicationUserId, 'NULL'), CHAR(13), CHAR(10),
        'PaymentValuePLN: ', COALESCE(CAST(NEW.PaymentValuePLN AS nvarchar(max)), 'NULL')
    )
    FROM inserted AS NEW
    WHERE NEW.Id = @PaymentId

    RETURN @NewValues
END
