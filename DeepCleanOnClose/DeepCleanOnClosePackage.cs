using System;
using System.Collections.Generic;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace DeepCleanOnClose
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Deep Clean On Close", "Cleans bin, obj, and files within packages folder on solution close", "0.0.1")]
    [ProvideAutoLoad(Microsoft.VisualStudio.VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CleanOnClosePackage : AsyncPackage
    {
        private DTE2 _dte;
        private EnvDTE.SolutionEvents _solutionEvents;

        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            _dte = await GetServiceAsync(typeof(DTE)) as DTE2;

            if (_dte != null)
            {
                _solutionEvents = _dte.Events.SolutionEvents;

                _solutionEvents.BeforeClosing += SolutionEvents_OnBeforeCloseSolution;

                _dte.Events.DTEEvents.OnBeginShutdown += DTEEvents_OnBeginShutdown;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Error gettiing DTE");
            }
        }

        private void SolutionEvents_OnBeforeCloseSolution()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            PerformCleanup();
        }

        private void DTEEvents_OnBeginShutdown()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            PerformCleanup();
        }

        private void PerformCleanup()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string solutionDir = _dte?.Solution?.FullName != null ? Path.GetDirectoryName(_dte.Solution.FullName) : null;

            if (!string.IsNullOrEmpty(solutionDir))
            {
                // Ensure the "packages" folder remains but its contents are deleted
                DeleteFolderContents(Path.Combine(solutionDir, "packages"));

                foreach (Project proj in GetAllProjects(_dte.Solution))
                {
                    try
                    {
                        string projDir = Path.GetDirectoryName(proj.FullName);
                        if (!string.IsNullOrEmpty(projDir))
                        {
                            DeleteFolder(Path.Combine(projDir, "bin"));
                            DeleteFolder(Path.Combine(projDir, "obj"));
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error cleaning project: {ex.Message}");
                    }
                }
            }
        }

        private void DeleteFolderContents(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    // Delete all files inside the folder
                    foreach (string file in Directory.GetFiles(folderPath))
                    {
                        File.Delete(file);
                    }

                    // Delete all subdirectories inside the folder
                    foreach (string subDir in Directory.GetDirectories(folderPath))
                    {
                        Directory.Delete(subDir, true);
                    }

                    System.Diagnostics.Debug.WriteLine($"Cleaned contents of: {folderPath}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error cleaning folder {folderPath}: {ex.Message}");
                }
            }
        }


        private IEnumerable<Project> GetAllProjects(Solution solution)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            List<Project> projects = new List<Project>();

            foreach (Project project in solution.Projects)
            {
                GetProjectsRecursive(project, projects);
            }

            return projects;
        }

        private void GetProjectsRecursive(Project project, List<Project> projects)
        {
            if (project == null) return;

            ThreadHelper.ThrowIfNotOnUIThread();

            if (project.Kind != EnvDTE.Constants.vsProjectKindSolutionItems)
            {
                projects.Add(project);
            }
            else if (project.ProjectItems != null)
            {
                foreach (ProjectItem item in project.ProjectItems)
                {
                    if (item.SubProject != null)
                    {
                        GetProjectsRecursive(item.SubProject, projects);
                    }
                }
            }
        }

        private void DeleteFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                try
                {
                    // First attempt to delete normally
                    Directory.Delete(folderPath, true);
                    System.Diagnostics.Debug.WriteLine($"Deleted: {folderPath}");
                }
                catch (IOException)
                {
                    // If folder is locked, retry deletion after forcing file unlock
                    System.Diagnostics.Debug.WriteLine($"Retrying deletion of locked folder: {folderPath}");
                    ForceDelete(folderPath);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error deleting folder {folderPath}: {ex.Message}");
                }
            }
        }

        private void ForceDelete(string folderPath)
        {
            try
            {
                foreach (string file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        File.SetAttributes(file, FileAttributes.Normal); // Remove read-only attributes
                        File.Delete(file);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not delete file {file}: {ex.Message}");
                    }
                }

                foreach (string dir in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Could not delete directory {dir}: {ex.Message}");
                    }
                }

                Directory.Delete(folderPath, true); // Final attempt to delete the root folder
                System.Diagnostics.Debug.WriteLine($"Force deleted: {folderPath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Final deletion failed for {folderPath}: {ex.Message}");
            }
        }
    }
}