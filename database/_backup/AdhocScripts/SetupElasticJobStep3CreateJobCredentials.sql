/*
	Create Database Credentials for execution of Elastic Agent Jobs.

	Ensure you set the following variables:

    	**ADD-PASSWORD-HERE**
*/

CREATE MASTER KEY ENCRYPTION BY PASSWORD = '**ADD-PASSWORD-HERE**';

CREATE DATABASE SCOPED CREDENTIAL JobRun WITH IDENTITY = 'GPConnectAnalytics-JobUser',
    SECRET = '**ADD-PASSWORD-HERE**';

CREATE DATABASE SCOPED CREDENTIAL MasterCred WITH IDENTITY = 'GPConnectAnalytics-MasterUser',
    SECRET = '**ADD-PASSWORD-HERE**';

