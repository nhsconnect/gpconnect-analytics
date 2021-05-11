/*
	Create Database Users for execution of Elastic Agent Jobs.
*/

USE GPConnectAnalytics;

CREATE USER [GPConnectAnalytics-MasterUser]
FROM LOGIN [GPConnectAnalytics-MasterUser]

CREATE USER [GPConnectAnalytics-JobUser]
FROM LOGIN [GPConnectAnalytics-JobUser]
 
ALTER ROLE db_owner
ADD MEMBER [GPConnectAnalytics-JobUser];  
GO