/*
    Schema V1.3 - create Import tables
*/

create table Import.[File]
(
	FileId integer not null identity(1, 1),
	FileTypeId smallint not null,
	FilePath varchar(500) not null,
	QueryFromDate datetime2 not null,
	QueryToDate datetime2 not null,
	SplunkInstance varchar(10) not null,
	ExtractDate datetime2 not null,
	IsInstalling bit not null,
	IsInstalled bit not null,
	InstalledDate datetime2 null,
	RowsAdded integer null,
	RowsUpdated integer null,
	RowsDeleted integer null,
	InstallDuration integer null,
	RowsInstalledPerSecond as (RowsAdded + RowsUpdated) / (case when InstallDuration = 0 then 1 else InstallDuration end)

	constraint PK_Import_File_FileId primary key clustered (FileId),
	constraint FK_Import_File_FileTypeId foreign key (FileTypeId) references Configuration.FileType (FileTypeId),
	constraint UQ_Import_File_FilePath unique (FilePath),
	constraint CK_Import_File_QueryFromDate_QueryToDate_ExtractDate check ((QueryFromDate < QueryToDate) and (QueryToDate < ExtractDate)),
	constraint FK_Import_File_SplunkInstance foreign key (SplunkInstance) references Configuration.SplunkInstance (SplunkInstance),
	constraint CK_Import_File_IsInstalling_IsInstalled check ((convert(integer, IsInstalling) + convert(integer, IsInstalled)) <= 1),
	constraint CK_Import_File_IsInstalled_InstalledDate_RowsAdded_RowsUpdated_RowsDeleted_InstallDuration check
	(
		(IsInstalled = 0 and InstalledDate is null and RowsAdded is null and RowsUpdated is null and RowsDeleted is null and InstallDuration is null)
		or (IsInstalled = 1 and InstalledDate is not null and RowsAdded is not null and RowsUpdated is not null and RowsDeleted is not null and InstallDuration is not null)
	)
);

create table Import.AsidLookupStaging
(
	ASID varchar(1000) null,
	MName varchar(1000) null,
	NACS varchar(1000) null,
	OrgName varchar(1000) null,
	OrgType varchar(1000) null,
	PName varchar(1000) null,
	PostCode varchar(1000) null
);

create table Import.SspTransactionStaging
(
	_time varchar(1000) null,
	sspFrom varchar(1000) null,
	sspTo varchar(1000) null,
	SspTraceId varchar(1000) null,
	interaction varchar(1000) null,
	responseCode varchar(1000) null,
	duration varchar(1000) null,
	responseSize varchar(1000) null,
	responseErrorMessage varchar(1000) null,
	method varchar(1000) null
);

CREATE TABLE [Import].[MeshTransactionStaging]
(
	[_time] [varchar](1000) NULL,
	[sender] [varchar](1000) NULL,
	[senderOdsCode] [varchar](1000) NULL,
	[senderName] [varchar](1000) NULL,
	[recipient] [varchar](1000) NULL,
	[recipientOdsCode] [varchar](1000) NULL,
	[recipientName] [varchar](1000) NULL,
	[workflow] [varchar](1000) NULL,
	[fileSize] [varchar](1000) NULL
);