CREATE TRIGGER trg_Subscription_Log
ON MRBasics.Subscriptions
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

    INSERT INTO MRBasics.Subscription_Logs (SubscriptionId, EntityName, Action, OldValues, NewValues, LogDate)
    SELECT 
        CASE 
            WHEN @Action = 'DELETE' THEN deleted.Id 
            ELSE inserted.Id 
        END,
        'Subscription',
        @Action,
        CASE 
            WHEN @Action = 'UPDATE' THEN dbo.GetOldValues_Subscription(deleted.Id) 
            ELSE NULL 
        END,
        CASE 
            WHEN @Action = 'INSERT' THEN dbo.GetNewValues_Subscription(inserted.Id) 
            ELSE dbo.GetNewValues_Subscription(deleted.Id) 
        END,
        GETDATE()
    FROM inserted
    FULL OUTER JOIN deleted ON inserted.Id = deleted.Id
END

GO

CREATE FUNCTION dbo.GetOldValues_Subscription(@SubscriptionId uniqueidentifier)
RETURNS nvarchar(max)
AS
BEGIN
    DECLARE @OldValues nvarchar(max)

    SELECT @OldValues = 
        'ApplicationUserId: ' + COALESCE(CONVERT(nvarchar(max), deleted.ApplicationUserId), 'NULL') + CHAR(13) + CHAR(10) +
        'PaymentId: ' + COALESCE(CONVERT(nvarchar(max), deleted.PaymentId), 'NULL') + CHAR(13) + CHAR(10) +
        'Id: ' + COALESCE(CONVERT(nvarchar(max), deleted.Id), 'NULL') + CHAR(13) + CHAR(10)
    FROM deleted
    WHERE deleted.Id = @SubscriptionId

    RETURN @OldValues
END

GO

CREATE FUNCTION GetNewValues_Subscription (@SubscriptionId uniqueidentifier)
RETURNS nvarchar(max)
AS
BEGIN
    DECLARE @NewValues nvarchar(max)

    SELECT @NewValues = CONCAT(
        'Id: ', COALESCE(NEW.Id, 'NULL'), CHAR(13), CHAR(10),
        'ApplicationUserId: ', COALESCE(NEW.ApplicationUserId, 'NULL'), CHAR(13), CHAR(10),
        'PaymentId: ', COALESCE(CAST(NEW.PaymentId AS nvarchar(max)), 'NULL')
    )
    FROM inserted AS NEW
    WHERE NEW.Id = @SubscriptionId

    RETURN @NewValues
END
