using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using DeepCleanOnClose.Interfaces;

namespace DeepCleanOnClose.Events
{
    internal class SolutionEventHandler
    {
        private readonly ISolutionCleaner _solutionCleaner;
        private static DTE2 _dte;
        private static SolutionEvents _solutionEvents;

        public SolutionEventHandler(ISolutionCleaner solutionCleaner)
        {
            _solutionCleaner = solutionCleaner;
        }

        public async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            _dte = (DTE2)await package.GetServiceAsync(typeof(DTE));
            
            if (_dte != null)
            {
                _solutionEvents = _dte.Events.SolutionEvents;
                _solutionEvents.BeforeClosing += () => Task.Run(() => _solutionCleaner.CleanSolutionAsync(_dte));
            }
        }
    }
}