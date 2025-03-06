using System.Text;
using Core;
using Core.DTOs.Response.Configuration;
using Core.DTOs.Response.Splunk;
using Core.Helpers;
using Core.Services.Interfaces;

namespace Functions.HelperClasses;

public class FilePathHelper(IConfigurationService configurationService, ITimeProvider timeProvider, Extract extract)
{
    public async Task<string> ConstructFilePath(SplunkInstance splunkInstance, FileType fileType, bool isToday,
        bool setDateAsMidnight = false)
    {
        //TODO: fix this - replace with less convoluted approach to filePath building

        var filePathConstants = await configurationService.GetFilePathConstants();
        var filePathString = new StringBuilder();
        filePathString.Append(fileType.DirectoryName);
        filePathString.Append(filePathConstants.PathSeparator);
        filePathString.Append(splunkInstance.Source);
        filePathString.Append(filePathConstants.PathSeparator);
        filePathString.Append(extract.QueryFromDate.ToString(DateFormatConstants.FilePathQueryDateYearMonth));
        filePathString.Append(filePathConstants.PathSeparator);
        filePathString.Append(filePathConstants.ProjectNameFilePrefix);
        filePathString.Append(filePathConstants.ComponentSeparator);
        filePathString.Append(fileType.FileTypeFilePrefix);
        filePathString.Append(filePathConstants.ComponentSeparator);
        filePathString.Append(
            $"{extract.QueryFromDate.ToString(DateFormatConstants.FilePathQueryDate)}T{extract.QueryHour.ToString(DateFormatConstants.FilePathQueryHour)}");
        filePathString.Append(filePathConstants.ComponentSeparator);
        filePathString.Append(
            $"{extract.QueryToDate.ToString(DateFormatConstants.FilePathQueryDate)}T{extract.QueryHour.ToString(DateFormatConstants.FilePathQueryHour)}");
        filePathString.Append(filePathConstants.ComponentSeparator);
        filePathString.Append(splunkInstance.Source);
        filePathString.Append(filePathConstants.ComponentSeparator);
        
        //TODO: is the correct ? seems should be the other way round , or a simpler logic for Today Midnight, Today End of Day, or Right Now.
        if (!isToday)
        {
            filePathString.Append(setDateAsMidnight
                ? timeProvider.CurrentDate().ToString(DateFormatConstants.FilePathNowDate)
                : timeProvider.UtcDateTime().ToString(DateFormatConstants.FilePathNowDate));
        }
        else
        {
            filePathString.Append(timeProvider.CurrentDate().AddDays(1).AddSeconds(-1)
                .ToString(DateFormatConstants.FilePathNowDate));
        }

        filePathString.Append(filePathConstants.FileExtension);
        return filePathString.ToString();
    }
}