using System;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using DeepCleanOnClose.Events;
using DeepCleanOnClose.Helpers;
using DeepCleanOnClose.Services;

namespace DeepCleanOnClose
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("Deep Clean On Close", "Cleans bin, obj, and files within packages folder on solution close", "0.0.1")]
    [ProvideAutoLoad(Microsoft.VisualStudio.VSConstants.UICONTEXT.SolutionExists_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class CleanOnClosePackage : AsyncPackage
    {
        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine("✅ Deep Clean On Close extension is initializing...");

            var fileHelper = new FileHelper();
            var projectHelper = new ProjectHelper();

            var solutionCleaner = new SolutionCleaner(fileHelper, projectHelper);

            var solutionEventHandler = new SolutionEventHandler(solutionCleaner);
            var shutdownEventHandler = new ShutdownEventHandler(solutionCleaner);
            
            await solutionEventHandler.InitializeAsync(this);
            await shutdownEventHandler.InitializeAsync(this);
        }
    }
}