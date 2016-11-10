namespace JanHafner.Smartbar.PackageAuthoring
{
    using System;
    using System.Linq;
    using System.Management.Automation;
    using JetBrains.Annotations;
    using Settings = JanHafner.Smartbar.PackageAuthoring.Properties.Settings;

    [Cmdlet("List", "SmartbarPlugin")]
    public class ListSmartbarPlugin : PSCmdlet
    {
        [Parameter(HelpMessage = "Lists local packages; otherwise remote packages.")]
        public SwitchParameter ForceLocal { get; set; }

        [Parameter(HelpMessage = "Lists only packages with this id.")]
        [CanBeNull]
        public String PluginPackageId { get; set; }

        [Parameter(HelpMessage = "Used in conjunction with PluginPackageId. Lists only packages with this version.")]
        [CanBeNull]
        public String PluginPackageVersion { get; set; }

        [Parameter(HelpMessage = "When -ForceLocal than this specifies the path to the local repository.")]
        [CanBeNull]
        public String LocalPackagesRepositoryDirectory { get; set; }

        protected override void ProcessRecord()
        {
            var repository = this.ForceLocal
                ? this.ToFullPath(this.LocalPackagesRepositoryDirectory)
                : Settings.Default.NugetServerUrl + "/" + Settings.Default.NugetServerPackageFeedSuffix;

            this.WriteVerbose($"Using \"{repository}\" as repository");

            var packageRepository = NuGet.PackageRepositoryFactory.Default.CreateRepository(repository);

            var packages = packageRepository.GetPackages().FilterByPackageIdAndVersion(this.PluginPackageId, this.PluginPackageVersion).ToList();

            this.WriteObject(packages, true);
        }
    }
}