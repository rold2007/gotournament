using System.IO;
using GoTournament.Interface;
using GoTournament.Service;

using Xunit;

namespace GoTournament.UnitTest
{
    using System;

    public class FileServiceTests
    {
        [Fact]
        public void FileExistsTest()
        {
            IFileService fileService = new FileService();
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.False(fileService.FileExists(tempFile));
            File.Create(tempFile).Close();
            Assert.True(fileService.FileExists(tempFile));
            File.Delete(tempFile);
            Assert.False(fileService.FileExists(tempFile));
        }

        [Fact]
        public void FileReadWriteTest()
        {
            IFileService fileService = new FileService();
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var content = DateTime.Now.ToLongTimeString();
            fileService.FileWriteAllText(tempFile, content);
            Assert.Equal(content, fileService.FileReadAllText(tempFile));
            if (fileService.FileExists(tempFile))
                File.Delete(tempFile);
        }
    }
}
