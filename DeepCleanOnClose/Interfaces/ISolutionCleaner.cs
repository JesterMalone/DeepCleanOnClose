using System.Threading.Tasks;
using EnvDTE80;

namespace DeepCleanOnClose.Interfaces
{
    public interface ISolutionCleaner
    {
        Task CleanSolutionAsync(DTE2 dte);
    }
}