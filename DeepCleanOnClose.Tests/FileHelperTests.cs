using DeepCleanOnClose.Helpers;
using DeepCleanOnClose.Interfaces;
using EnvDTE;
using Moq;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DeepCleanOnClose.Tests
{
    public class FileHelperTests
    {
        private readonly IFileHelper _systemUnderTest;

        public FileHelperTests()
        {
            _systemUnderTest = new FileHelper();
        }

        public class DeleteFolderAsync : FileHelperTests
        {
            [Fact]
            public async Task GIVEN_folder_exists_WHEN_DeleteFolderAsync_is_called_THEN_folder_is_removed()
            {
                string testDir = Path.Combine(Path.GetTempPath(), "IntegrationTestFolder");
                Directory.CreateDirectory(testDir);
                File.WriteAllText(Path.Combine(testDir, "test.txt"), "Test");

                Assert.True(Directory.Exists(testDir));

                await _systemUnderTest.DeleteFolderAsync(testDir);

                Assert.False(Directory.Exists(testDir));
            }
        }

        public class DeleteFolderContentsAsync : FileHelperTests
        {
            [Fact]
            public async Task GIVEN_folder_has_contents_WHEN_DeleteFolderContentsAsync_is_called_THEN_folders_contents_are_deleted()
            {
                string testDir = Path.Combine(Path.GetTempPath(), "IntegrationTestFolder");

                Directory.CreateDirectory(testDir);

                File.WriteAllText(Path.Combine(testDir, "test1.txt"), "Test");
                File.WriteAllText(Path.Combine(testDir, "test2.txt"), "Test");


                Assert.True(File.Exists(Path.Combine(testDir, "test1.txt")));
                Assert.True(File.Exists(Path.Combine(testDir, "test2.txt")));

                await _systemUnderTest.DeleteFolderContentsAsync(testDir);

                Assert.True(Directory.Exists(testDir));
                Assert.Empty(Directory.GetFiles(testDir));
            }
        }
    }
}