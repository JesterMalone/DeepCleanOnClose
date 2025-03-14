using System.Collections.Generic;
using DeepCleanOnClose.Interfaces;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace DeepCleanOnClose.Helpers
{
    public class ProjectHelper : IProjectHelper
    {
        public IEnumerable<Project> GetAllProjects(Solution solution)
        {
            var projects = new HashSet<Project>();

            foreach (Project project in solution.Projects)
            {
                GetProjectsRecursive(project, projects);
            }

            return projects;
        }

        private void GetProjectsRecursive(Project project, HashSet<Project> projects)
        {
            if (project != null && string.IsNullOrEmpty(project.FullName))
            {
                projects.Add(project);

                if (project.ProjectItems != null)
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
        }
    }
}