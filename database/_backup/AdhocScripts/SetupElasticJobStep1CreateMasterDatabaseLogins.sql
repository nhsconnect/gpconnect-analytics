/*
	Create Master Database Login for execution of Elastic Agent Jobs.
	These must be executed in the Master database

	Ensure you set the following variables:

    	**ADD-PASSWORD-HERE**
*/

CREATE LOGIN [GPConnectAnalytics-MasterUser]
WITH PASSWORD = '**ADD-PASSWORD-HERE**';

CREATE LOGIN [GPConnectAnalytics-JobUser]
WITH PASSWORD = '**ADD-PASSWORD-HERE**';