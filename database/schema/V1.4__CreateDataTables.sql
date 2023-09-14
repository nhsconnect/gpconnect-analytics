/*
    Schema V1.4 - create Data tables
*/

create table Data.AsidLookup
(
	Asid varchar(50) not null,
	OrgName varchar(200) not null,
	OdsCode varchar(20) not null,
	OrgType varchar(50) not null,
	Postcode varchar(10) not null,
	SupplierName varchar(200) not null,
	ProductName varchar(200) not null,
	IsDeleted bit not null,
	FileId integer not null

	constraint PK_Data_AsidLookup_ASID primary key clustered (Asid),
	constraint FK_Data_AsidLookup_FileId foreign key (FileId) references Import.[File] (FileId)
);

create table Data.Interaction
(
	InteractionId smallint not null,
	InteractionName varchar(100) not null,
	ServiceName varchar(100) not null

	constraint PK_Data_Interaction_InteractionId primary key clustered (InteractionId),
	constraint UQ_Data_Interaction_InteractionName unique (InteractionName),
	constraint CK_Data_Interaction_ServiceName check ((ServiceName = 'gpconnect') or (ServiceName = 'other'))
);

create table Data.SspTransaction
(
	SspTransactionId bigint not null identity(1, 1),
	Time datetimeoffset not null,
	FromAsid varchar(50) not null,
	ToAsid varchar(50) not null,
	SspTraceId varchar(50) null,
	InteractionId smallint not null,
	ResponseCode varchar(1000) null,
	Duration decimal(8, 3) not null,
	ResponseSize integer null,
	ResponseErrorMessage varchar(1000) null,
	Method varchar(100) null,
	FileId integer not null,
	[Date] AS (CONVERT([date],[Time])) PERSISTED NOT NULL,

	constraint PK_Data_SspTransaction_Date_SspTransactionId primary key clustered (Date, SspTransactionId),
	constraint UQ_Data_SspTransaction_SspTransactionId unique (SspTransactionId),
	constraint FK_Data_SspTransaction_FromAsid foreign key (FromAsid) references Data.AsidLookup (Asid),
	constraint FK_Data_SspTransaction_ToAsid foreign key (ToAsid) references Data.AsidLookup (Asid),
	constraint FK_Data_SspTransaction_InteractionId foreign key (InteractionId) references Data.Interaction (InteractionId),
	constraint FK_Data_SspTransaction_FileId foreign key (FileId) references Import.[File] (FileId)
);

CREATE TABLE [Data].[MeshTransaction]
(
	[MeshTransactionId] [bigint] IDENTITY(1,1) NOT NULL,
	[Time] [datetimeoffset](7) NOT NULL,
	[Sender] [varchar](50) NOT NULL,
	[SenderOdsCode] [varchar](50) NULL,
	[SenderName] [varchar](1000) NULL,
	[Recipient] [varchar](50) NOT NULL,
	[RecipientOdsCode] [varchar](50) NULL,
	[RecipientName] [varchar](1000) NULL,
	[Workflow] [varchar](50) NOT NULL,
	[Filesize] [int] NULL,
	[FileId] [int] NOT NULL,
	[Date] AS (CONVERT([date],[Time])) PERSISTED NOT NULL,

	CONSTRAINT [PK_Data_MeshTransaction_Date_MeshTransactionId] PRIMARY KEY CLUSTERED ([Date] ASC, [MeshTransactionId] ASC),
	CONSTRAINT [UQ_Data_MeshTransaction_MeshTransactionId] UNIQUE NONCLUSTERED ([MeshTransactionId] ASC),
	CONSTRAINT [FK_Data_MeshTransaction_FileId] FOREIGN KEY([FileId]) REFERENCES [Import].[File] ([FileId])
);
