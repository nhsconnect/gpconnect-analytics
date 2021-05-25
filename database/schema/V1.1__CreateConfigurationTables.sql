/*
    Schema V1.1 - create Configuration tables
*/


create table Configuration.SplunkInstance
(
	SplunkInstance varchar(10) not null,
    SplunkInstanceGroup varchar(10) not null

	constraint PK_Configuration_SplunkInstance_SplunkInstance primary key clustered (SplunkInstance),
    constraint CK_Configuration_SplunkInstance_SplunkInstance check (trim(SplunkInstance) != ''),
    constraint CK_Configuration_SplunkInstance_SplunkInstance_Group check (trim(SplunkInstanceGroup) != '')
);

insert into Configuration.SplunkInstance 
(
	SplunkInstance,
    SplunkInstanceGroup
) 
values
(
    'cloud',
    'cloud'
),
(
    'spinea',
    'spine'
),
(
    'spineb',
    'spine'
),
(
    'other',
    'other'
);

create table Configuration.FilePathConstants
(
    SingleRowLock bit not null,
    PathSeparator varchar(1) not null,
    ProjectNameFilePrefix varchar(50) not null,
    ComponentSeparator varchar(1) not null,
    FileExtension varchar(5) not null

    constraint PK_Configuration_FilePathConstants_SingleRowLock primary key clustered (SingleRowLock),
    constraint CK_Configuration_FilePathConstants_SingleRowLock check (SingleRowLock = 1)
);

insert into Configuration.FilePathConstants
(
    SingleRowLock,
    PathSeparator,
    ProjectNameFilePrefix,
    ComponentSeparator,
    FileExtension
)
values
(
    1,
    '\',
    'gpcanalytics',
    '-',
    '.csv'
);

create table Configuration.FileType
(
    FileTypeId smallint not null,
    DirectoryName varchar(200) not null,
    FileTypeFilePrefix varchar(50) not null,
    SplunkQuery varchar(8000) not null,
    QueryFromBaseDate datetime2 not null,
    QueryPeriodHours integer not null,
    StagingTableName varchar(200) not null

    constraint PK_Configuration_FileType_FileTypeId primary key clustered (FileTypeId),
    constraint UQ_Configuration_FileType_DirectoryName unique (DirectoryName),
    constraint UQ_Configuration_FileType_FileTypeFilePrefix unique (FileTypeFilePrefix),
    constraint UQ_Configuration_FileType_StagingTableName unique (StagingTableName)
);

insert into Configuration.FileType
(
    FileTypeId,
    DirectoryName,
    FileTypeFilePrefix,
    SplunkQuery,
    QueryFromBaseDate,
    QueryPeriodHours,
    StagingTableName
)
values
(
    1,
    'asid-lookup-data',
    'asidlookup',
    '| inputlookup asidLookup.csv',
    convert(datetime2, '2020-01-01 00:00:00'),
    24 * 7,
    'Import.AsidLookupStaging'
),
(
    2,
    'ssp-transactions',
    'ssptrans',
    'search index=spine2vfmmonitor (logReference=SSP0001 OR logReference=SSP0015 OR logReference=SSP0016) earliest="{earliest}" latest="{latest}" | transaction internalID maxspan=1h keepevicted=true | table _time, SspTraceId, sspFrom, sspTo, interaction, responseCode, duration, responseSize, responseErrorMessage, method | eval _time=strftime(_time, "%Y-%m-%dT%H:%M:%S.%Q%z")',
    convert(datetime2, '2020-01-01 00:00:00'),
    24,
    'Import.SspTransactionStaging'
);

create table Configuration.SplunkClient
(
    SingleRowLock bit not null,
    HostName varchar(500) not null,
    HostPort integer not null,
    BaseUrl varchar(1000) not null,
    QueryParameters varchar(1000) not null,
    QueryTimeout smallint not null,
    SplunkInstance varchar(10) not null,
    ApiToken varchar(1000) not null

    constraint PK_Configuration_SplunkClient_SingleRowLock primary key (SingleRowLock),
    constraint CK_Configuration_SplunkClient_SingleRowLock check (SingleRowLock = 1),
    constraint FK_Configuration_SplunkClient_SplunkInstance foreign key (SplunkInstance) references Configuration.SplunkInstance (SplunkInstance)
);

insert into Configuration.SplunkClient
(
    SingleRowLock,
    HostName,
    HostPort,
    BaseUrl,
    QueryParameters,
    QueryTimeout,
    SplunkInstance,
    ApiToken
)
values
(
    1,
    '***SET-HOST-NAME***',	
    443,
    '***SET-BASE-URL***',
    '?QueryDateFrom={0}&QueryDateTo={1}',
    30,
    'cloud',
    '***SET-APITOKEN-VALUE***'
);

create table Configuration.BlobStorage
(
	SingleRowLock bit not null,
	BlobPrimaryKey varchar(1000) not null,
	ConnectionString varchar(1000) not null,
	ContainerName varchar(255) not null,
    QueueName varchar(255) not null,
    SqlExternalDataSourceName varchar(255) not null

	constraint PK_Configuration_BlobStorage_SingleRowLock primary key clustered (SingleRowLock),
	constraint CK_Configuration_BlobStorage_SingleRowLock check (SingleRowLock = 1),
    constraint CK_Configuration_BlobStorage_SqlExternalDataSourceName check (trim(SqlExternalDataSourceName) != '')
);

insert into Configuration.BlobStorage
(
    SingleRowLock,
    BlobPrimaryKey,
    ConnectionString,
    ContainerName,
    QueueName,
    SqlExternalDataSourceName
)
values
(
    1,
    '***SET-BLOB-PRIMARY-KEY***',
    '***SET-CONNECTION-STRING***',
    '***SET-CONTAINER-NAME***',
    '***SET-QUEUE-NAME***',
    'GPConnectAnalyticsBlobStore'
);

create table Configuration.Email
(
    SingleRowLock bit not null,
    SenderAddress varchar(100) not null,
    Hostname varchar(100) not null,
    Port smallint not null,
    Encryption varchar(10) not null,
    AuthenticationRequired bit not null,
    Username varchar(100) not null,
    Password varchar(100) not null,
    DefaultSubject varchar(100) not null

    constraint PK_Configuration_Email_SingleRowLock primary key (SingleRowLock),
    constraint CK_Configuration_Email_SingleRowLock check (SingleRowLock = 1),
    constraint CK_Configuration_Email_SenderAddress check (len(trim(SenderAddress)) > 0),
    constraint CK_Configuration_Email_Hostname check (len(trim(Hostname)) > 0),
    constraint CK_Configuration_Email_Port check (port > 0),
    constraint CK_Configuration_Email_Encryption check (len(trim(Encryption)) > 0),
    constraint CK_Configuration_Email_DefaultSubject check (len(trim(DefaultSubject)) > 0)
);


insert into Configuration.Email
(
    SingleRowLock,
    SenderAddress,
    Hostname, 
    Port, 
    Encryption, 
    AuthenticationRequired, 
    Username, 
    Password, 
    DefaultSubject
)
values
(
    1, 
    'test@test.com', 
    'smtp.test.com', 
    100, 
    'Tls12', 
    1, 
    '', 
    '', 
    'GP Connect Analytics'
);
