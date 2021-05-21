/*
    Manually install files
*/

-- add ASID lookup file
exec ApiReader.AddFile 1, 'asid-lookup-data\gpcanalytics-asidlookup-20210101T000000-20210107T000000-cloud-20210430T120000.csv'


-- add SSP transactions files
exec ApiReader.AddFile 2, 'ssp-transactions\spinea\gpcanalytics-ssptrans-20200101T000000-20200201T000000-spinea-20210429T123000.csv'
exec ApiReader.AddFile 2, 'ssp-transactions\spinea\gpcanalytics-ssptrans-20200201T000000-20200301T000000-spinea-20210429T123000.csv'



-- install ASID lookup file
declare @MoreFilesToInstall bit = 1;
declare @InstallCount integer = 1;

while (@MoreFilesToInstall = 1)
begin
    begin try
        print '';
        print '============  ASID LOOKUP INSTALL ' + convert(varchar, @InstallCount) + ' ============';
        print '';

        exec Import.InstallNextFile 1, @MoreFilesToInstall output;

        set @InstallCount = @InstallCount + 1;
    end try
    begin catch
        throw;
    end catch;
end;



-- install SSP transactions file
declare @MoreFilesToInstall2 bit = 1;
declare @InstallCount2 integer = 1;

while (@MoreFilesToInstall2 = 1)
begin
    begin try
        print '';
        print '============  SSP TRANSACTIONS INSTALL ' + convert(varchar, @InstallCount2) + ' ============';
        print '';

        exec Import.InstallNextFile 2, @MoreFilesToInstall2 output;

        set @InstallCount2 = @InstallCount2 + 1;
    end try
    begin catch
        throw;
    end catch;
end;

