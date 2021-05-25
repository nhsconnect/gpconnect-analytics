/*
    Schema V1.2 - Create tables in the ApiReader schema
*/

create table Logging.Log
( 
	LogId bigint not null identity(1, 1),
	Application varchar(100) null,
    Logged datetime2 null,
    Level varchar(100) null,
    Message varchar(8000) null,
    Logger varchar(8000) null, 
    Callsite varchar(8000) null, 
    Exception varchar(8000) null,

    constraint PK_Logging_Log_LogId primary key clustered (LogId)
);
