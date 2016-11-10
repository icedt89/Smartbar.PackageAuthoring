namespace JanHafner.Smartbar.PackageAuthoring
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using JetBrains.Annotations;
    using NuGet;
    using Settings = JanHafner.Smartbar.PackageAuthoring.Properties.Settings;

    [Cmdlet(VerbsData.Unpublish, "SmartbarPlugin")]
    public class UnpublishSmartbarPlugin : PSCmdlet
    {
        [Parameter(HelpMessage = "Removes only packages with this id.")]
        [CanBeNull]
        public String PluginPackageId { get; set; }

        [Parameter(HelpMessage = "Used in conjunction with PluginPackageId. Removes only packages with this version.")]
        [CanBeNull]
        public String PluginPackageVersion { get; set; }

        protected override void ProcessRecord()
        {
            var packageServer = new PackageServer(Settings.Default.NugetServerUrl, "SmartbarPackageAuthoring");

            var repository = PackageRepositoryFactory.Default.CreateRepository(Settings.Default.NugetServerUrl + "/" + Settings.Default.NugetServerPackageFeedSuffix);
            var packages = repository.GetPackages().FilterByPackageIdAndVersion(this.PluginPackageId, this.PluginPackageVersion).ToList();

            foreach (var package in packages)
            {
                var result = this.UnpublishPackage(packageServer, package);
                if (result.Successful)
                {
                    this.WriteVerbose($"Successfully unpublished \"{package.Id} ({package.Version})\"");
                }
                else
                {
                    this.WriteWarning($"Could not unpublish \"{package.Id} ({package.Version})\" because {result.Exception.Message}");
                }

                this.WriteObject(result);
            }
        }

        private UnpublishPackageResult UnpublishPackage(PackageServer packageServer, IPackage package)
        {
            try
            {
                packageServer.DeletePackage(Settings.Default.ApiKey, package.Id, package.Version.ToString());

                return UnpublishPackageResult.Success(package);
            }
            catch (Exception ex)
            {
                return UnpublishPackageResult.Faulted(package, ex);
            }
        }

        private sealed class UnpublishPackageResult
        {
            private UnpublishPackageResult()
            {
            }

            public IPackage Package { get; private set; }

            public Exception Exception { get; private set; }

            public Boolean Successful
            {
                get { return this.Exception == null; }
            }

            public static UnpublishPackageResult Success(IPackage package)
            {
                return new UnpublishPackageResult
                {
                    Package = package
                };
            }

            public static UnpublishPackageResult Faulted(IPackage package, Exception exception)
            {
                return new UnpublishPackageResult
                {
                    Exception = exception,
                    Package = package
                };
            }
        }
    }
}