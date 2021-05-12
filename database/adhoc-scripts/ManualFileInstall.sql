/*
    Manually install files
*/

-- add ASID lookup file
exec ApiReader.AddFile 1, 'asid-lookup-data\gpcanalytics-asidlookup-20210101T000000-20210107T000000-cloud-20210430T120000.csv'


-- add SSP transactions files
exec ApiReader.AddFile 2, 'ssp-transactions\spinea\gpcanalytics-ssptrans-20200101T000000-20200201T000000-spinea-20210429T123000.csv'
exec ApiReader.AddFile 2, 'ssp-transactions\spinea\gpcanalytics-ssptrans-20200201T000000-20200301T000000-spinea-20210429T123000.csv'


-- install ASID lookup file
exec Import.InstallNextFile 1;


-- install SSP transactions file
declare @MoreFilesToInstall bit = 0;

while (@MoreFilesToInstall = 1)
begin
    begin try
        exec Import.InstallNextFile 2, @MoreFilesToInstall output;
    end try
    begin catch
        throw;
    end catch;
end;

