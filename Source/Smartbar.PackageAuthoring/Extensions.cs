namespace JanHafner.Smartbar.PackageAuthoring
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Management.Automation;
    using JetBrains.Annotations;
    using NuGet;

    internal static class Extensions
    {
        [NotNull]
        [LinqTunnel]
        [Pure]
        public static IEnumerable<IPackage> FilterByPackageIdAndVersion([NotNull] this IQueryable<IPackage> packages, [CanBeNull] String packageId, [CanBeNull] String packageVersion)
        {
            Func<IPackage, Boolean> filterExpression = package => package.Tags.Contains("smartbar");
            if (!String.IsNullOrWhiteSpace(packageId))
            {
                filterExpression = package => package.Id == packageId && package.Tags.Contains("smartbar");

                if (!String.IsNullOrWhiteSpace(packageVersion))
                {
                    filterExpression = package => package.Id == packageId && package.Version == SemanticVersion.Parse(packageVersion) && package.Tags.Contains("smartbar");
                }
            }

            return packages.Where(filterExpression);
        }

        [CanBeNull]
        [Pure]
        public static String ToFullPath(this PSCmdlet psCmdlet, String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            ProviderInfo providerInfo;
            return psCmdlet.GetResolvedProviderPathFromPSPath(path, out providerInfo)[0];
        }
    }
}
