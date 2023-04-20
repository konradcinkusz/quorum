USE [aspnet-mreferenda.Server-44bd1c16-4782-4de1-8743-3aee3305f17d];
GO

CREATE OR ALTER TRIGGER trg_Payment_InsertUpdateDelete
ON [MRPayments].[Payments]
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @action nvarchar(10)

    IF EXISTS(SELECT * FROM inserted)
        IF EXISTS(SELECT * FROM deleted)
            SET @action = 'UPDATE'
        ELSE
            SET @action = 'INSERT'

    DECLARE @oldValues NVARCHAR(MAX)
    DECLARE @newValues NVARCHAR(MAX)

    SELECT @oldValues = COALESCE(@oldValues + ',', '') + 
        CAST(d.Id AS NVARCHAR(MAX)) + ',' +
        CAST(d.PaymentStatus AS NVARCHAR(MAX)) + ',' +
        CAST(d.PaymentValuePLN AS NVARCHAR(MAX)) + ',' +
        CAST(d.ApplicationUserId AS NVARCHAR(MAX)) + ',' +
        CAST(d.SessionId AS NVARCHAR(MAX))
    FROM deleted d

    SELECT @newValues = COALESCE(@newValues + ',', '') + 
        CAST(i.Id AS NVARCHAR(MAX)) + ',' +
        CAST(i.PaymentStatus AS NVARCHAR(MAX)) + ',' +
        CAST(i.PaymentValuePLN AS NVARCHAR(MAX)) + ',' +
        CAST(i.ApplicationUserId AS NVARCHAR(MAX)) + ',' +
        CAST(i.SessionId AS NVARCHAR(MAX))
    FROM inserted i

    INSERT INTO [MRPayments].[Payment_Logs] (Action, OldValues, NewValues, LogDate, PaymentId)
    SELECT @action, @oldValues, @newValues, GETDATE(), COALESCE(i.Id, d.Id)
    FROM inserted i FULL OUTER JOIN deleted d ON i.Id = d.Id
END
