/*
    Schema V1.1 - create Configuration tables
*/


CREATE TABLE Configuration.SplunkInstance
(
	SplunkInstance VARCHAR(10) NOT NULL,
    SplunkInstanceGroup VARCHAR(10) NOT NULL

	CONSTRAINT PK_Configuration_SplunkInstance_SplunkInstance PRIMARY KEY CLUSTERED (SplunkInstance),
    CONSTRAINT CK_Configuration_SplunkInstance_SplunkInstance CHECK (TRIM(SplunkInstance) != ''),
    CONSTRAINT CK_Configuration_SplunkInstance_SplunkInstance_Group CHECK (TRIM(SplunkInstanceGroup) != '')
);

INSERT INTO Configuration.SplunkInstance 
(
	SplunkInstance,
    SplunkInstanceGroup
) 
VALUES
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
);

CREATE TABLE Configuration.FilePathConstants
(
    SingleRowLock BIT NOT NULL,
    PathSeparator VARCHAR(1) NOT NULL,
    ProjectNameFilePrefix VARCHAR(50) NOT NULL,
    ComponentSeparator VARCHAR(1) NOT NULL,
    FileExtension VARCHAR(5) NOT NULL

    CONSTRAINT PK_Configuration_FilePathConstants_SingleRowLock PRIMARY KEY CLUSTERED (SingleRowLock),
    CONSTRAINT CK_Configuration_FilePathConstants_SingleRowLock CHECK (SingleRowLock = 1)
);

INSERT INTO Configuration.FilePathConstants
(
    SingleRowLock,
    PathSeparator,
    ProjectNameFilePrefix,
    ComponentSeparator,
    FileExtension
)
VALUES
(
    1,
    '\',
    'gpcanalytics',
    '-',
    '.csv'
);

CREATE TABLE Configuration.FileType
(
    FileTypeId SMALLINT NOT NULL,
    DirectoryName VARCHAR(200) NOT NULL,
    FileTypeFilePrefix VARCHAR(50) NOT NULL,
    SplunkQuery VARCHAR(8000) NOT NULL,
    QueryFromBaseDate DATETIME2 NOT NULL,
    QueryPeriodHours INTEGER NOT NULL,
    StagingTableName VARCHAR(200) NOT NULL

    CONSTRAINT PK_Configuration_FileType_FileTypeId PRIMARY KEY CLUSTERED (FileTypeId),
    CONSTRAINT UQ_Configuration_FileType_DirectoryName UNIQUE (DirectoryName),
    CONSTRAINT UQ_Configuration_FileType_FileTypeFilePrefix UNIQUE (FileTypeFilePrefix),
    CONSTRAINT UQ_Configuration_FileType_StagingTableName UNIQUE (StagingTableName)
);

INSERT INTO Configuration.FileType
(
    FileTypeId,
    DirectoryName,
    FileTypeFilePrefix,
    SplunkQuery,
    QueryFromBaseDate,
    QueryPeriodHours,
    StagingTableName
)
VALUES
(
    1,
    'asid-lookup-data',
    'asidlookup',
    '| inputlookup asidLookup.csv',
    CONVERT(DATETIME2, '2021-01-01 00:00:00'),
    24 * 7,
    'Import.AsidLookupStaging'
),
(
    2,
    'ssp-transactions',
    'ssptrans',
    'index=spinevfmlog (logReference=SSP0001 OR logReference=SSP0004 OR logReference=SSP0012) | transaction internalID startswith=SSP0001 endswith=SSP0004 keepevicted=true maxspan=1h | table _time, sspFrom, sspTo, SspTraceId, interaction, responseCode, duration, responseSize, responseErrorMessage, method | sort 0 _time',
    CONVERT(DATETIME2, '2020-01-01 00:00:00'),
    24,
    'Import.SspTransactionStaging'
);

CREATE TABLE Configuration.SplunkClient
(
    SingleRowLock BIT NOT NULL,
    HostName VARCHAR(500) NOT NULL,
    HostPort INTEGER NOT NULL,
    BaseUrl VARCHAR(1000) NOT NULL,
    QueryParameters VARCHAR(1000) NOT NULL,
    QueryTimeout SMALLINT NOT NULL,
    SplunkInstance VARCHAR(10) NOT NULL

    CONSTRAINT PK_Configuration_SplunkClient_SingleRowLock PRIMARY KEY (SingleRowLock),
    CONSTRAINT CK_Configuration_SplunkClient_SingleRowLock CHECK (SingleRowLock = 1),
    CONSTRAINT FK_Configuration_SplunkClient_SplunkInstance FOREIGN KEY (SplunkInstance) REFERENCES Configuration.SplunkInstance (SplunkInstance)
);

INSERT INTO Configuration.SplunkClient
(
    SingleRowLock,
    HostName,
    HostPort,
    BaseUrl,
    QueryParameters,
    QueryTimeout,
    SplunkInstance
)
VALUES
(
    1,
    '***SET-HOST-NAME***',	
    443,
    '***SET-BASE-URL***',
    '?QueryDateFrom={0}&QueryDateTo={1}',
    30,
    'cloud'
);

CREATE TABLE Configuration.BlobStorage
(
	SingleRowLock BIT NOT NULL,
	BlobPrimaryKey VARCHAR(1000) NOT NULL,
	ConnectionString VARCHAR(1000) NOT NULL,
	ContainerName VARCHAR(255) NOT NULL,
    QueueName VARCHAR(255) NOT NULL,
    SqlExternalDataSourceName VARCHAR(255) NOT NULL

	CONSTRAINT PK_Configuration_BlobStorage_SingleRowLock PRIMARY KEY CLUSTERED (SingleRowLock),
	CONSTRAINT CK_Configuration_BlobStorage_SingleRowLock CHECK (SingleRowLock = 1),
    CONSTRAINT CK_Configuration_BlobStorage_SqlExternalDataSourceName CHECK (trim(SqlExternalDataSourceName) != '')
);

INSERT INTO Configuration.BlobStorage
(
    SingleRowLock,
    BlobPrimaryKey,
    ConnectionString,
    ContainerName,
    QueueName,
    SqlExternalDataSourceName
)
VALUES
(
    1,
    '***SET-BLOB-PRIMARY-KEY***',
    '***SET-CONNECTION-STRING***',
    '***SET-CONTAINER-NAME***',
    '***SET-QUEUE-NAME***',
    'GPConnectAnalyticsBlobStore'
);

CREATE TABLE Configuration.Email
(
    SingleRowLock BIT NOT NULL,
    SenderAddress VARCHAR(100) NOT NULL,
    Hostname VARCHAR(100) NOT NULL,
    Port SMALLINT NOT NULL,
    Encryption VARCHAR(10) NOT NULL,
    AuthenticationRequired BIT NOT NULL,
    Username VARCHAR(100) NOT NULL,
    Password VARCHAR(100) NOT NULL,
    DefaultSubject VARCHAR(100) NOT NULL

    CONSTRAINT PK_Configuration_Email_SingleRowLock PRIMARY KEY (SingleRowLock),
    CONSTRAINT CK_Configuration_Email_SingleRowLock CHECK (SingleRowLock = 1),
    CONSTRAINT CK_Configuration_Email_SenderAddress CHECK (LEN(TRIM(SenderAddress)) > 0),
    CONSTRAINT CK_Configuration_Email_Hostname CHECK (LEN(TRIM(Hostname)) > 0),
    CONSTRAINT CK_Configuration_Email_Port CHECK (port > 0),
    CONSTRAINT CK_Configuration_Email_Encryption CHECK (LEN(TRIM(Encryption)) > 0),
    CONSTRAINT CK_Configuration_Email_DefaultSubject CHECK (LEN(TRIM(DefaultSubject)) > 0)
);


INSERT INTO Configuration.Email
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
VALUES
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
