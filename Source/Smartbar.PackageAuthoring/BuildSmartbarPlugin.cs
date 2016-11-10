namespace JanHafner.Smartbar.PackageAuthoring
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using JetBrains.Annotations;
    using NuGet;
    using Settings = JanHafner.Smartbar.PackageAuthoring.Properties.Settings;

    [Cmdlet("Build", "SmartbarPlugin")]
    public class BuildSmartbarPlugin : PSCmdlet
    {
        [Parameter(HelpMessage = "Specifies the path to the nuspec file or a directory where multiple nuspec files are located.")]
        [NotNull]
        public String SourceNuspecPath { get; set; }

        [Parameter(HelpMessage = "Specifies the path to the base directory from where package dependencies are resolved. If -SourceNuspecPath is a directory this value will be implied by the path to the nuspec file.")]
        [CanBeNull]
        public String BaseDependencyDirectory { get; set; }

        [Parameter(HelpMessage = "Specifies the path to the local repository where nupkg files are stored.")]
        [CanBeNull]
        public String LocalPackagesRepositoryDirectory { get; set; }

        protected override void BeginProcessing()
        {
            this.SourceNuspecPath = this.ToFullPath(this.SourceNuspecPath);
            this.BaseDependencyDirectory = this.ToFullPath(this.BaseDependencyDirectory);
        }

        protected override void ProcessRecord()
        {
            var creatableNugetPackageData = new List<CreateNugetPackageData>();
            if (Directory.Exists(this.SourceNuspecPath))
            {
                this.WriteVerbose("-SourceNuspecPath is a directory");

                foreach (
                    var nuspecFile in
                        Directory.EnumerateFiles(this.SourceNuspecPath, "*." + Settings.Default.NuspecFileExtension, SearchOption.TopDirectoryOnly))
                {
                    var baseDependencyDirectory = new FileInfo(nuspecFile).DirectoryName;

                    creatableNugetPackageData.Add(CreateNugetPackageData.Create(nuspecFile, baseDependencyDirectory, this.ToFullPath(this.LocalPackagesRepositoryDirectory)));
                }
            }
            else if (File.Exists(this.SourceNuspecPath))
            {
                var baseDependencyDirectory = Directory.Exists(this.BaseDependencyDirectory) ? this.BaseDependencyDirectory : new FileInfo(this.SourceNuspecPath).DirectoryName;

                creatableNugetPackageData.Add(CreateNugetPackageData.Create(this.SourceNuspecPath, baseDependencyDirectory, this.ToFullPath(this.LocalPackagesRepositoryDirectory)));
            }
            else
            {
                this.WriteWarning("Non existing or invalid directory path or nuspec file path supplied via -SourceNuspecPath");
                return;
            }

            foreach (var createNugetPackageData in creatableNugetPackageData)
            {
                var result = this.ProcessNuspecFile(createNugetPackageData);
                if (result.Successful)
                {
                    this.WriteVerbose($"Successfully built \"{result.Manifest.Metadata.Id} ({result.Manifest.Metadata.Version})\"");
                }
                else
                {
                    this.WriteWarning($"Could not build \"{result.Manifest.Metadata.Id} ({result.Manifest.Metadata.Version})\" because {result.Exception.Message}");
                }

                this.WriteObject(result);
            }
        }

        private BuildPackageResult ProcessNuspecFile([NotNull] CreateNugetPackageData createNugetPackageData)
        {
            if (createNugetPackageData == null)
            {
                throw new ArgumentNullException(nameof(createNugetPackageData));
            }

            Manifest nuspecManifest;
            using (var nuspecFileStream = new FileStream(createNugetPackageData.SourceNuspecFile, FileMode.Open, FileAccess.Read))
            {
                nuspecManifest = Manifest.ReadFrom(nuspecFileStream, true);
            }

            var finalNugetPackageFile = Path.Combine(createNugetPackageData.TargetNugetPackageDirectory,
                String.Format(Settings.Default.NugetPackageFileNamePattern, nuspecManifest.Metadata.Id,
                    nuspecManifest.Metadata.Version) + "." + Settings.Default.PackageFileExtension);
            var nugetPackageBuilder = new PackageBuilder();
            using (var nugetPackageFileStream = new FileStream(finalNugetPackageFile, FileMode.Create))
            {
                try
                {
                    nugetPackageBuilder.Populate(nuspecManifest.Metadata);
                    nugetPackageBuilder.PopulateFiles(createNugetPackageData.BaseDependencyDirectory, nuspecManifest.Files);
                    nugetPackageBuilder.Save(nugetPackageFileStream);

                    return BuildPackageResult.Success(nuspecManifest, finalNugetPackageFile);
                }
                catch (Exception ex)
                {
                    return BuildPackageResult.Faulted(nuspecManifest, ex);
                }
            }
        }

        private sealed class CreateNugetPackageData
        {
            private CreateNugetPackageData(String sourceNuspecFile, String baseDependencyDirectory, String targetNugetPackageDirectory)
            {
                if (String.IsNullOrWhiteSpace(sourceNuspecFile))
                {
                    throw new ArgumentNullException(nameof(sourceNuspecFile));
                }

                if (String.IsNullOrWhiteSpace(baseDependencyDirectory))
                {
                    throw new ArgumentNullException(nameof(baseDependencyDirectory));
                }

                if (String.IsNullOrWhiteSpace(targetNugetPackageDirectory))
                {
                    throw new ArgumentNullException(nameof(targetNugetPackageDirectory));
                }

                this.SourceNuspecFile = sourceNuspecFile;
                this.BaseDependencyDirectory = baseDependencyDirectory;
                this.TargetNugetPackageDirectory = targetNugetPackageDirectory;
            }

            public String SourceNuspecFile { get; private set; }

            public String BaseDependencyDirectory { get; private set; }

            public String TargetNugetPackageDirectory { get; private set; }

            public static CreateNugetPackageData Create([NotNull] String sourceNuspecFile, String baseDependencyPath,
                String targetNugetPackagePath)
            {
                return new CreateNugetPackageData(sourceNuspecFile, baseDependencyPath, targetNugetPackagePath);
            }
        }

        private sealed class BuildPackageResult
        {
            private BuildPackageResult()
            {
            }

            [NotNull]
            public Manifest Manifest { get; private set; }

            [CanBeNull]
            public Exception Exception { get; private set; }

            [CanBeNull]
            public String FinalNugetPackageFile { get; private set; }

            public Boolean Successful
            {
                get { return this.Exception == null; }
            }

            public static BuildPackageResult Success([NotNull] Manifest manifest, [NotNull] String finalNugetPackageFile)
            {
                if (manifest == null)
                {
                    throw new ArgumentNullException(nameof(manifest));
                }

                if (String.IsNullOrWhiteSpace(finalNugetPackageFile))
                {
                    throw new ArgumentNullException(nameof(finalNugetPackageFile));
                }

                return new BuildPackageResult
                {
                    Manifest = manifest,
                    FinalNugetPackageFile =  finalNugetPackageFile 
                };
            }

            public static BuildPackageResult Faulted([NotNull] Manifest manifest, [NotNull] Exception exception)
            {
                if (manifest == null)
                {
                    throw new ArgumentNullException(nameof(manifest));
                }

                if (exception == null)
                {
                    throw new ArgumentNullException(nameof(exception));
                }

                return new BuildPackageResult
                {
                    Exception = exception,
                    Manifest = manifest
                };
            }
        }
    }
}