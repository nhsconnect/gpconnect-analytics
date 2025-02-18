using Core.Services.Interfaces;
using Dapper;
using Functions.Services;
using Moq;

namespace Functions.Tests.Services;

public class FileServiceTests
{
    [Fact]
    public void ApiReaderAddFile_ShouldCallStoredProcedure()
    {
        // Arrange
        var _mockDataService = new Mock<IDataService>();
        var _fileService = new FileService(_mockDataService.Object);

        // Act
        _fileService.ApiReaderAddFile(1, "path/to/file", true);

        // Assert
        _mockDataService.Verify(x => x.ExecuteStoredProcedure("ApiReader.AddFile",
                It.Is<DynamicParameters>(p =>
                    p.Get<int>("FileTypeId") == 1 &&
                    p.Get<string>("FilePath") == "path/to/file" &&
                    p.Get<bool>("Override") == true
                )),
            Times.Once);
    }
}