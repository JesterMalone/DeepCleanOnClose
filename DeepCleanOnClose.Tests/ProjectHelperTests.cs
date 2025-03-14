using System.Collections.Generic;
using System.Linq;
using DeepCleanOnClose.Helpers;
using DeepCleanOnClose.Interfaces;
using EnvDTE;
using Moq;
using Xunit;

namespace DeepCleanOnClose.Tests
{
    public class ProjectHelperTests
    {
        private readonly Mock<Solution> _mockSolution;
        private readonly Mock<Project> _mockProject1;
        private readonly Mock<Project> _mockProject2;
        private readonly Mock<ProjectItem> _mockProjectItem;
        private readonly IProjectHelper _projectHelper;

        public ProjectHelperTests()
        {
            _mockSolution = new Mock<Solution>();
            _mockProject1 = new Mock<Project>();
            _mockProject2 = new Mock<Project>();
            _mockProjectItem = new Mock<ProjectItem>();
            _projectHelper = new ProjectHelper();
        }

        public class GetAllProjects : ProjectHelperTests
        {
            [Fact]
            public void GIVEN_multiple_projects_in_solutions_WHEN_GetAllProjects_is_called_THEN_projects_are_returned()
            {
                var projects = new List<Project> { _mockProject1.Object, _mockProject2.Object };
                var mockProjectsCollection = new Mock<Projects>();
                mockProjectsCollection.Setup(p => p.GetEnumerator()).Returns(projects.GetEnumerator());

                _mockSolution.Setup(s => s.Projects).Returns(mockProjectsCollection.Object);

                var result = _projectHelper.GetAllProjects(_mockSolution.Object);

                Assert.NotNull(result);
                Assert.Equal(2, result.Count());
            }
        }

        public class GetProjectsRecursive : ProjectHelperTests
        {
            [Fact]
            public void GIVEN_project_has_nested_projects_WHEN_GetProjectsRecursive_is_called_THEN_nested_projects_are_returned()
            {
                var nestedProject = new Mock<Project>();
                _mockProjectItem.Setup(pi => pi.SubProject).Returns(nestedProject.Object);
                var mockProjectItems = new Mock<ProjectItems>();
                mockProjectItems.Setup(pi => pi.GetEnumerator()).Returns(new HashSet<ProjectItem> { _mockProjectItem.Object }.GetEnumerator());

                _mockProject1.Setup(p => p.ProjectItems).Returns(mockProjectItems.Object);

                var projects = new HashSet<Project>(); 

                var helper = new ProjectHelper();
                var method = typeof(ProjectHelper).GetMethod("GetProjectsRecursive", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(helper, new object[] { _mockProject1.Object, projects });

                Assert.Contains(nestedProject.Object, projects);
            }
        }
    }
}