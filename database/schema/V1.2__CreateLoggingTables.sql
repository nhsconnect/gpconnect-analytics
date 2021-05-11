/*
    Schema V1.2 - Create tables in the ApiReader schema
*/

CREATE TABLE Logging.Log
( 
	LogId BIGINT NOT NULL IDENTITY(1, 1),
	Application VARCHAR(100) NULL,
    Logged DATETIME2 NULL,
    Level VARCHAR(100) NULL,
    Message VARCHAR(8000) NULL,
    Logger VARCHAR(8000) NULL, 
    Callsite VARCHAR(8000) NULL, 
    Exception VARCHAR(8000) NULL,

    CONSTRAINT PK_Logging_Log_LogId PRIMARY KEY CLUSTERED (LogId)
);
