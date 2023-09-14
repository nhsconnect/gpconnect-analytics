IF (object_id('Data.SspTransactionView') IS NOT NULL)
    DROP VIEW Data.SspTransactionView;
GO

CREATE VIEW [Data].[SspTransactionView]
AS
    SELECT
        s.SspTransactionId,
	CONVERT(DATE, FORMAT(TRY_CONVERT(DATE, s.Time), 'dd MMMM yyyy')) AS 'Date',
	CONVERT(TIME(0), FORMAT(s.Time, 'HH:mm:ss')) AS 'Time',
        TRY_CONVERT(BIGINT, s.FromAsid) AS FromAsid,
        TRY_CONVERT(BIGINT, s.ToAsid) AS ToAsid,
	s.SspTraceId,
	s.InteractionId,
        TRY_CONVERT(INTEGER, s.ResponseCode) AS 'ResponseCode',
        s.Duration,
        s.ResponseSize,
        s.ResponseErrorMessage,
        s.Method		
    FROM
	Data.SspTransaction s;
GO


