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
	FileId integer not null

	constraint PK_Data_SspTransaction_SspTransactionId primary key clustered (SspTransactionId),
	constraint FK_Data_SspTransaction_FromAsid foreign key (FromAsid) references Data.AsidLookup (Asid),
	constraint FK_Data_SspTransaction_ToAsid foreign key (ToAsid) references Data.AsidLookup (Asid),
	constraint FK_Data_SspTransaction_InteractionId foreign key (InteractionId) references Data.Interaction (InteractionId),
	constraint FK_Data_SspTransaction_FileId foreign key (FileId) references Import.[File] (FileId)
);
