using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using DeepCleanOnClose.Interfaces;

namespace DeepCleanOnClose.Events
{
    public class ShutdownEventHandler
    {
        private static DTE2 _dte;
        private readonly ISolutionCleaner _solutionCleaner;

        public ShutdownEventHandler(ISolutionCleaner solutionCleaner)
        {
            _solutionCleaner = solutionCleaner;
        }

        public async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            
            _dte = (DTE2)await package.GetServiceAsync(typeof(DTE));

            if (_dte != null)
            {
                _dte.Events.DTEEvents.OnBeginShutdown += () => Task.Run(() => _solutionCleaner.CleanSolutionAsync(_dte));
            }
        }
    }
}