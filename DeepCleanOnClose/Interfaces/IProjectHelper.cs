using EnvDTE;
using System.Collections.Generic;

namespace DeepCleanOnClose.Interfaces
{
    public interface IProjectHelper
    {
        IEnumerable<Project> GetAllProjects(Solution solution);
    }
}