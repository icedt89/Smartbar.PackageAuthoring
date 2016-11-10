namespace JetBrains.Annotations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Indicates that parameter is regular expression pattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class RegexPatternAttribute : Attribute { }
}