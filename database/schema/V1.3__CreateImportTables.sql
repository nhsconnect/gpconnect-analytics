/*
    Schema V1.3 - create Import tables
*/

CREATE TABLE Import.[File]
(
	FileId INTEGER NOT NULL IDENTITY(1, 1),
	FileTypeId SMALLINT NOT NULL,
	FilePath VARCHAR(500) NOT NULL,
	QueryFromDate DATETIME2 NOT NULL,
	QueryToDate DATETIME2 NOT NULL,
	SplunkInstance VARCHAR(10) NOT NULL,
	ExtractDate DATETIME2 NOT NULL,
	IsInstalling BIT NOT NULL,
	IsInstalled BIT NOT NULL,
	InstalledDate DATETIME2 NULL,
	RowsAdded INT NULL,
	RowsUpdated INT NULL,
	InstallDuration INT NULL

	CONSTRAINT PK_Import_File_FileId PRIMARY KEY CLUSTERED (FileId),
	CONSTRAINT FK_Import_File_FileTypeId FOREIGN KEY (FileTypeId) REFERENCES Configuration.FileType (FileTypeId),
	CONSTRAINT UQ_Import_File_FilePath UNIQUE (FilePath),
	CONSTRAINT CK_Import_File_QueryFromDate_QueryToDate_ExtractDate CHECK ((QueryFromDate < QueryToDate) and (QueryToDate < ExtractDate)),
	CONSTRAINT FK_Import_File_SplunkInstance FOREIGN KEY (SplunkInstance) REFERENCES Configuration.SplunkInstance (SplunkInstance),
	CONSTRAINT CK_Import_File_IsInstalling_IsInstalled CHECK ((CONVERT(INTEGER, IsInstalling) + CONVERT(INTEGER, IsInstalled)) <= 1),
	CONSTRAINT CK_Import_File_IsInstalled_InstalledDate_RowsAdded_RowsUpdated_InstallDuration CHECK
	(
		(IsInstalled = 0 AND InstalledDate IS NULL AND RowsAdded IS NULL AND InstallDuration IS NULL)
		OR (IsInstalled = 1 AND InstalledDate IS NOT NULL AND RowsAdded IS NOT NULL AND RowsUpdated IS NOT NULL AND InstallDuration IS NOT NULL)
	)
);

CREATE TABLE Import.AsidLookupStaging
(
	ASID VARCHAR(1000) NULL,
	MName VARCHAR(1000) NULL,
	NACS VARCHAR(1000) NULL,
	OrgName VARCHAR(1000) NULL,
	OrgType VARCHAR(1000) NULL,
	PName VARCHAR(1000) NULL,
	PostCode VARCHAR(1000) NULL
);

CREATE TABLE Import.SspTransactionStaging
(
	_time VARCHAR(1000) NULL,
	sspFrom VARCHAR(1000) NULL,
	sspTo VARCHAR(1000) NULL,
	SspTraceId VARCHAR(1000) NULL,
	interaction VARCHAR(1000) NULL,
	responseCode VARCHAR(1000) NULL,
	duration VARCHAR(1000) NULL,
	responseSize VARCHAR(1000) NULL,
	responseErrorMessage VARCHAR(1000) NULL,
	method VARCHAR(1000) NULL
);