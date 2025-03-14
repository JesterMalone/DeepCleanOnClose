using System.Threading.Tasks;

namespace DeepCleanOnClose.Interfaces
{
    public interface IFileHelper
    {
        Task DeleteFolderAsync(string folderPath);
        Task DeleteFolderContentsAsync(string folderPath);
    }
}