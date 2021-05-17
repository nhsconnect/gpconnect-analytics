/*
    Schema V1.6 - Create a partition on SspTransactions for super fast querying based on date, month and year
*/

/* Create a partition function for every day/month/year between 1 January 2020 and 1 January 2050 */

DECLARE @DatePartitionFunction nvarchar(max) = 
    N'CREATE PARTITION FUNCTION DatePartitionFunction (datetimeoffset(7)) 
    AS RANGE RIGHT FOR VALUES (';  
DECLARE @i datetime2 = '20200101';  
WHILE @i < '20500101'
BEGIN  
SET @DatePartitionFunction += '''' + CAST(@i as nvarchar(10)) + '''' + N', ';  
SET @i = DATEADD(MM, 1, @i);  
END  
SET @DatePartitionFunction += '''' + CAST(@i as nvarchar(10))+ '''' + N');';  
EXEC sp_executesql @DatePartitionFunction;  

/* Create partition scheme based on the date partition function */

CREATE PARTITION SCHEME [DatePartitionScheme]
    AS PARTITION [DatePartitionFunction]
    ALL TO ('PRIMARY');

/* Recreate the primary indexes for Data.SspTransaction */

ALTER TABLE [Data].[SspTransaction] DROP CONSTRAINT [PK_Data_SspTransaction_SspTransactionId] WITH ( ONLINE = OFF );

ALTER TABLE [Data].[SspTransaction]
    ADD CONSTRAINT [PK_SspTransaction_Partition_SspTransactionId_Time]
PRIMARY KEY CLUSTERED (
    [SspTransactionId] ASC,
    [Time] ASC
    )
    WITH (
        ONLINE = ON
    )
    ON DatePartitionScheme([Time]);
GO