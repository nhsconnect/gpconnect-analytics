/*
    Schema V1.8 - update Configuration tables
*/

INSERT INTO [Configuration].[SplunkInstance]
           ([SplunkInstance]
           ,[SplunkInstanceGroup])
     VALUES
           ('other',
           'other')
GO

ALTER TABLE Configuration.SplunkClient ADD ApiToken VARCHAR(1000) NULL;

UPDATE Configuration.SplunkClient SET ApiToken='*** SET APITOKEN VALUE HERE ***';

ALTER TABLE Configuration.SplunkClient ALTER COLUMN ApiToken VARCHAR(1000) NOT NULL;

ALTER TABLE Configuration.SplunkClient ADD CONSTRAINT CK_Configuration_SplunkClient_ApiToken  CHECK (trim(ApiToken ) != '')


