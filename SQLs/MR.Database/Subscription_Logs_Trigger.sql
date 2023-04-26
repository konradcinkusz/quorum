USE [aspnet-mreferenda.Server-44bd1c16-4782-4de1-8743-3aee3305f17d]
GO
/****** Object:  Trigger [MRPayments].[trg_Payment_Log]    Script Date: 26/04/2023 12:49:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER TRIGGER MRBasics.trg_Subscription_Log
ON MRBasics.Subscriptions
AFTER INSERT, UPDATE, DELETE
AS

SET NOCOUNT ON;

INSERT INTO MRBasics.Subscription_Logs
    (SubscriptionId, Action, OldValues, NewValues, LogDate)
SELECT
  ISNULL(i.Id, d.Id),

  IIF(i.Id IS NULL, 'DELETE', IIF(d.Id IS NULL, 'INSERT', 'UPDATE')),

  CASE WHEN i.Id IS NOT NULL AND d.Id IS NOT NULL THEN
    CONCAT_WS(NCHAR(13) + NCHAR(10),
      'Id: ' + ISNULL(CONVERT(nvarchar(max), d.Id), 'NULL'),
      'ApplicationUserId: ' + ISNULL(CONVERT(nvarchar(max), d.ApplicationUserId), 'NULL')
    )
  END,

  CONCAT_WS(NCHAR(13) + NCHAR(10),
    'Id: ' + ISNULL(CAST(i.Id AS nvarchar(max)), 'NULL'),
    'ApplicationUserId: ' + ISNULL(i.ApplicationUserId, 'NULL')
    ),

  GETDATE()

FROM inserted i
FULL JOIN deleted d ON i.Id = d.Id;

Go