/*
    Schema V1.4 - create Data tables
*/

CREATE TABLE Data.AsidLookup
(
	Asid VARCHAR(20) NOT NULL,
	OrgName VARCHAR(200) NOT NULL,
	OdsCode VARCHAR(20) NOT NULL,
	OrgType VARCHAR(50) NOT NULL,
	Postcode VARCHAR(10) NOT NULL,
	SupplierName VARCHAR(200) NOT NULL,
	ProductName VARCHAR(200) NOT NULL,
	FileId INTEGER NOT NULL

	CONSTRAINT PK_Data_AsidLookup_ASID PRIMARY KEY CLUSTERED (Asid),
	CONSTRAINT FK_Data_AsidLookup_FileId FOREIGN KEY (FileId) REFERENCES Import.[File] (FileId)
);

CREATE table Data.Interaction
(
	InteractionId SMALLINT NOT NULL,
	InteractionName VARCHAR(100) NOT NULL,
	ServiceName VARCHAR(100) NOT NULL

	CONSTRAINT PK_Data_Interaction_InteractionId PRIMARY KEY CLUSTERED (InteractionId),
	CONSTRAINT UQ_Data_Interaction_InteractionName UNIQUE (InteractionName),
	CONSTRAINT CK_Data_Interaction_ServiceName CHECK ((ServiceName = 'gpconnect') or (ServiceName = 'other'))
);

CREATE TABLE Data.SspTransaction
(
	SspTransactionId BIGINT NOT NULL IDENTITY(1, 1),
	Time DATETIMEOFFSET NOT NULL,
	FromAsid VARCHAR(20) NOT NULL,
	ToAsid VARCHAR(20) NOT NULL,
	SspTraceId VARCHAR(50) NULL,
	InteractionId SMALLINT NOT NULL,
	ResponseCode VARCHAR(1000) NULL,
	Duration DECIMAL(8, 3) NOT NULL,
	ResponseSize INTEGER null,
	ResponseErrorMessage VARCHAR(1000) NULL,
	Method VARCHAR(100) NULL,
	FileId INTEGER NOT NULL

	CONSTRAINT PK_Data_SspTransaction_SspTransactionId PRIMARY KEY CLUSTERED (SspTransactionId),
	CONSTRAINT FK_Data_SspTransaction_FromAsid FOREIGN KEY (FromAsid) REFERENCES Data.AsidLookup (Asid),
	CONSTRAINT FK_Data_SspTransaction_ToAsid FOREIGN KEY (ToAsid) REFERENCES Data.AsidLookup (Asid),
	CONSTRAINT FK_Data_SspTransaction_InteractionId FOREIGN KEY (InteractionId) REFERENCES Data.Interaction (InteractionId),
	CONSTRAINT FK_Data_SspTransaction_FileId FOREIGN KEY (FileId) REFERENCES Import.[File] (FileId)
);
