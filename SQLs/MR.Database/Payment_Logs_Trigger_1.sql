USE [aspnet-mreferenda.Server-44bd1c16-4782-4de1-8743-3aee3305f17d]
GO
/****** Object:  Trigger [MRPayments].[trg_Payment_Log]    Script Date: 26/04/2023 12:49:03 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create or ALTER TRIGGER [MRPayments].[trg_Payment_Log]
ON [MRPayments].[Payments]
AFTER INSERT, UPDATE
AS

SET NOCOUNT ON;

INSERT INTO MRPayments.Payment_Logs
    (PaymentId, Action, OldValues, NewValues, LogDate)
SELECT
  ISNULL(i.Id, d.Id),

  IIF(i.Id IS NULL, 'DELETE', IIF(d.Id IS NULL, 'INSERT', 'UPDATE')),

  CASE WHEN i.Id IS NOT NULL AND d.Id IS NOT NULL THEN
    CONCAT_WS(NCHAR(13) + NCHAR(10),
      'PaymentStatus: ' + ISNULL(CONVERT(nvarchar(max), d.PaymentStatus), 'NULL'),
      'ApplicationUserId: ' + ISNULL(CONVERT(nvarchar(max), d.ApplicationUserId), 'NULL'),
    'PaymentValuePLN: ' + ISNULL(CONVERT(nvarchar(max), d.PaymentValuePLN), 'NULL')
    )
  END,

  CONCAT_WS(NCHAR(13) + NCHAR(10),
    'PaymentStatus: ' + ISNULL(CAST(i.PaymentStatus AS nvarchar(max)), 'NULL'),
    'ApplicationUserId: ' + ISNULL(i.ApplicationUserId, 'NULL'),
    'PaymentValuePLN: ' + ISNULL(CAST(i.PaymentValuePLN AS nvarchar(max)), 'NULL')
    ),

  GETDATE()

FROM inserted i
FULL JOIN deleted d ON i.Id = d.Id;

