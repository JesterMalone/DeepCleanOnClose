using System;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using DeepCleanOnClose.Interfaces;

namespace DeepCleanOnClose.Services
{
    public class SolutionCleaner : ISolutionCleaner
    {
        private readonly IFileHelper _fileHelper;
        private readonly IProjectHelper _projectHelper;

        public SolutionCleaner(IFileHelper fileHelper, IProjectHelper projectHelper)
        {
            _fileHelper = fileHelper;
            _projectHelper = projectHelper;
        }

        public async Task CleanSolutionAsync(DTE2 dte)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (dte?.Solution?.FullName != null)
            {
                string solutionDir = Path.GetDirectoryName(dte.Solution.FullName);
                System.Diagnostics.Debug.WriteLine($"🛠 Cleaning solution: {solutionDir}");

                if (!string.IsNullOrEmpty(solutionDir))
                {
                    await _fileHelper.DeleteFolderContentsAsync(Path.Combine(solutionDir, "packages"));
                    System.Diagnostics.Debug.WriteLine($"✅ Cleaned packages folder");

                    foreach (Project project in _projectHelper.GetAllProjects(dte.Solution))
                    {
                        try
                        {
                            string projDir = Path.GetDirectoryName(project.FullName);

                            if (!string.IsNullOrEmpty(projDir))
                            {
                                await _fileHelper.DeleteFolderAsync(Path.Combine(projDir, "bin"));
                                await _fileHelper.DeleteFolderAsync(Path.Combine(projDir, "obj"));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error cleaning project: {ex.Message}");
                        }
                    }
                }
            }
        }
    }
}