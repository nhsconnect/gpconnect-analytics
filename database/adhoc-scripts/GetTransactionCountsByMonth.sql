/*
	Get transaction counts by month and splunk instance
*/

select
	datepart(year, t.[Time]) as Year,
	datepart(month, t.[Time]) as Month,
	f.SplunkInstance,
	count(*) as TransactionCount
from Data.SspTransaction t
inner join Import.[File] f on t.FileId = f.FileId
group by 
	datepart(year, t.[Time]), 
	datepart(month, t.[Time]),
	f.SplunkInstance;
