SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER TRIGGER MRBasics.trg_Subscription_Log
ON MRBasics.Subscriptions
AFTER INSERT, UPDATE
AS

SET NOCOUNT ON;

INSERT INTO MRBasics.Subscription_Logs
    (SubscriptionId, Action, OldValues, NewValues, LogDate)
SELECT
  ISNULL(i.ApplicationUserId, d.ApplicationUserId),

  IIF(i.ApplicationUserId IS NULL, 'DELETE', IIF(d.ApplicationUserId IS NULL, 'INSERT', 'UPDATE')),

  CASE WHEN i.ApplicationUserId IS NOT NULL AND d.ApplicationUserId IS NOT NULL THEN
    CONCAT_WS(NCHAR(13) + NCHAR(10),
  'LOGDATA:',
      'ApplicationUserId: ' + ISNULL(CONVERT(nvarchar(max), d.ApplicationUserId), 'NULL')
    )
  END,

  CONCAT_WS(NCHAR(13) + NCHAR(10),
  'LOGDATA:',
    'ApplicationUserId: ' + ISNULL(i.ApplicationUserId, 'NULL')
    ),

  GETDATE()

FROM inserted i
FULL JOIN deleted d ON i.ApplicationUserId = d.ApplicationUserId;

Go