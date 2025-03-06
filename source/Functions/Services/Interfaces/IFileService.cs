namespace Functions.Services.Interfaces;

public interface IFileService
{
    Task<int> ApiReaderAddFile(int fileTypeID, string filePath, bool overrideFile);
}