namespace JanHafner.Smartbar.PackageAuthoring
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using System.Threading;
    using JetBrains.Annotations;
    using NuGet;
    using Settings = JanHafner.Smartbar.PackageAuthoring.Properties.Settings;

    [Cmdlet(VerbsData.Publish, "SmartbarPlugin")]
    public class PublishSmartbarPlugin : PSCmdlet
    {
        private const String packageIdAndVersionParameterSetName = "PackageIdAndVersion";
        private const String packageFileParameterSetName = "PackageFile";

        [Parameter(HelpMessage = "Publishes only packages with this id.", ParameterSetName = PublishSmartbarPlugin.packageIdAndVersionParameterSetName)]
        [CanBeNull]
        public String PluginPackageId { get; set; }

        [Parameter(HelpMessage = "Used in conjunction with PluginPackageId. Publishes only packages with this version.", ParameterSetName = PublishSmartbarPlugin.packageIdAndVersionParameterSetName)]
        [CanBeNull]
        public String PluginPackageVersion { get; set; }

        [Parameter(HelpMessage = "The package file itself.", ParameterSetName = PublishSmartbarPlugin.packageFileParameterSetName)]
        [CanBeNull]
        public String PluginPackageFile { get; set; }

        [Parameter(HelpMessage = "Specifies the path to the local repository.", ParameterSetName = PublishSmartbarPlugin.packageIdAndVersionParameterSetName)]
        [CanBeNull]
        public String LocalPackagesRepositoryDirectory { get; set; }

        protected override void ProcessRecord()
        {
            var packages = Enumerable.Empty<IPackage>();
            if (this.ParameterSetName == PublishSmartbarPlugin.packageFileParameterSetName)
            {
                packages = new[] {new ZipPackage(this.ToFullPath(this.PluginPackageFile)) };
            }
            else
            {
                var packageRepositoryDirectory = PackageRepositoryFactory.Default.CreateRepository(this.ToFullPath(this.LocalPackagesRepositoryDirectory));
                packages = packageRepositoryDirectory.GetPackages().FilterByPackageIdAndVersion(this.PluginPackageId, this.PluginPackageVersion).ToList();
            }
            
            var packageServer = new PackageServer(Settings.Default.NugetServerUrl, "SmartbarPackageAuthoring");

            foreach (var package in packages)
            {
                var result = this.PublishPackage(packageServer, package);
                if (result.Successful)
                {
                    this.WriteVerbose($"Successfully published \"{package.Id} ({package.Version})\"");
                }
                else
                {
                    this.WriteWarning($"Could not publish \"{package.Id} ({package.Version})\" because {result.Exception.Message}");
                }

                this.WriteObject(result);
            }
        }

        private PublishPackageResult PublishPackage(PackageServer packageServer, IPackage package)
        {
            try
            {
                packageServer.PushPackage(Settings.Default.ApiKey, package, package.GetStream().Length, Timeout.Infinite, false);

                return PublishPackageResult.Success(package);
            }
            catch (Exception ex)
            {
                return PublishPackageResult.Faulted(package, ex);
            }
        }

        private sealed class PublishPackageResult
        {
            private PublishPackageResult()
            {
            }

            public IPackage Package { get; private set; }

            public Exception Exception { get; private set; }

            public Boolean Successful
            {
                get { return this.Exception == null; }
            }

            public static PublishPackageResult Success(IPackage package)
            {
                return new PublishPackageResult
                {
                    Package = package
                };
            }

            public static PublishPackageResult Faulted(IPackage package, Exception exception)
            {
                return new PublishPackageResult
                {
                    Exception = exception,
                    Package = package
                };
            }
        }
    }
}
