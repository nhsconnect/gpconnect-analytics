UPDATE Configuration.FileType SET SplunkQuery='search index=spine2vfmmonitor (logReference=SSP0001 OR logReference=SSP0015 OR logReference=SSP0016) earliest="{earliest}" latest="{latest}" | transaction internalID maxspan=1h keepevicted=true | table _time, SspTraceId, sspFrom, sspTo, interaction, responseCode, duration, responseSize, responseErrorMessage, method | eval _time=strftime(_time, "%Y-%m-%dT%H:%M:%S.%Q%z")' WHERE FileTypeId=2
GO

UPDATE Configuration.FileType SET SplunkQuery='| inputlookup asidLookup.csv' WHERE FileTypeId=1
GO