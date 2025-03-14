using DeepCleanOnClose.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace DeepCleanOnClose.Helpers
{
    public class FileHelper : IFileHelper
    {
        public async Task DeleteFolderAsync(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    await Task.Run(() => Directory.Delete(folderPath, true));
                }
                catch (IOException)
                {
                    await ForceDeleteAsync(folderPath);
                }
            }
        }

        public async Task DeleteFolderContentsAsync(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                foreach (string file in Directory.GetFiles(folderPath))
                {
                    await Task.Run(() => File.Delete(file));
                }

                foreach (string dir in Directory.GetDirectories(folderPath))
                {
                    await Task.Run(() => Directory.Delete(dir, true));
                }
            }
        }

        private async Task ForceDeleteAsync(string folderPath)
        {
            foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                await Task.Run(() => File.Delete(file));
            }

            foreach (string dir in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
            {
                await Task.Run(() => Directory.Delete(dir, true));
            }

            await Task.Run(() => Directory.Delete(folderPath, true));
        }
    }
}