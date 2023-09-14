IF (object_id('Data.MeshTransactionView') IS NOT NULL)
    DROP VIEW Data.MeshTransactionView;
GO

CREATE VIEW [Data].[MeshTransactionView]
AS
	SELECT
		m.MeshTransactionId,
		CONVERT(DATE, FORMAT(TRY_CONVERT(DATE, m.Time), 'dd MMMM yyyy')) AS 'Date',
		CONVERT(TIME(0), FORMAT(m.Time, 'HH:mm:ss')) AS 'Time',
	        m.Sender,
		m.SenderOdsCode,
		m.SenderName,
		m.Recipient,
		m.RecipientOdsCode,
		m.RecipientName,
		m.Workflow,
		m.Filesize
	FROM
		Data.MeshTransaction m;
GO


