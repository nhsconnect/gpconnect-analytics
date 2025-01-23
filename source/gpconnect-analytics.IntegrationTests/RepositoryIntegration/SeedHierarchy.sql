IF OBJECT_ID('[DATA].[HierarchyProviderConsumers]', 'U') IS NULL
    BEGIN
        CREATE TABLE [DATA].[HierarchyProviderConsumers]
        (
            OdsCode                nvarchar(450) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL,
            PracticeName           nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
            RegisteredPatientCount int                                                NOT NULL,
            RegionCode             nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
            RegionName             nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
            Icb22Name              nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
            PcnName                nvarchar(MAX) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
            Appointments13000      int                                                NOT NULL,
            CONSTRAINT PK_HierarchyProviderConsumers PRIMARY KEY (OdsCode)
        );
    END;
