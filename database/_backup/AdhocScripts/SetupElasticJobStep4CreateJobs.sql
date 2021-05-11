/*
	Create Job definitions.
*/

EXEC jobs.sp_add_target_group 'PoolGroup';

DECLARE @ServerName VARCHAR = (SELECT @@SERVERNAME + '.database.windows.net')

EXEC jobs.sp_add_target_group_member
@target_group_name = 'PoolGroup',
@target_type = 'SqlDatabase',
@server_name = @ServerName,
@database_name = 'GPConnectAnalytics';

EXEC jobs.sp_add_job 
	@job_name = 'ExecuteDataImport', 
	@description = 'Execute Data Import',
	@enabled = 1,
	@schedule_interval_type = 'Hours',
	@schedule_interval_count = 1;

EXEC jobs.sp_add_jobstep 
	@job_name = 'ExecuteDataImport',
	@command = N'EXEC DataImport',
	@credential_name = 'JobRun',
	@target_group_name = 'PoolGroup';