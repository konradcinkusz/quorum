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
        'UserEmail: ' + COALESCE(CONVERT(nvarchar(max), deleted.UserEmail), 'NULL') + CHAR(13) + CHAR(10) +
        'PaymentLink: ' + COALESCE(CONVERT(nvarchar(max), deleted.PaymentLink), 'NULL') + CHAR(13) + CHAR(10) +
        'ClientReferenceId: ' + COALESCE(CONVERT(nvarchar(max), deleted.ClientReferenceId), 'NULL') + CHAR(13) + CHAR(10) +
        'PaymentIntentId: ' + COALESCE(CONVERT(nvarchar(max), deleted.PaymentIntentId), 'NULL') + CHAR(13) + CHAR(10) +
        'SessionId: ' + COALESCE(CONVERT(nvarchar(max), deleted.SessionId), 'NULL') + CHAR(13) + CHAR(10) +
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
        'UserEmail: ', COALESCE(NEW.UserEmail, 'NULL'), CHAR(13), CHAR(10),
        'PaymentLink: ', COALESCE(NEW.PaymentLink, 'NULL'), CHAR(13), CHAR(10),
        'ClientReferenceId: ', COALESCE(NEW.ClientReferenceId, 'NULL'), CHAR(13), CHAR(10),
        'PaymentIntentId: ', COALESCE(NEW.PaymentIntentId, 'NULL'), CHAR(13), CHAR(10),
        'SessionId: ', COALESCE(NEW.SessionId, 'NULL'), CHAR(13), CHAR(10),
        'PaymentStatus: ', COALESCE(NEW.PaymentStatus, 'NULL'), CHAR(13), CHAR(10),
        'ApplicationUserId: ', COALESCE(NEW.ApplicationUserId, 'NULL'), CHAR(13), CHAR(10),
        'PaymentValuePLN: ', COALESCE(CAST(NEW.PaymentValuePLN AS nvarchar(max)), 'NULL')
    )
    FROM inserted AS NEW
    WHERE NEW.Id = @PaymentId

    RETURN @NewValues
END
