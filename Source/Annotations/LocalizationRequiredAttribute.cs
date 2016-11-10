namespace JetBrains.Annotations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Indicates that marked element should be localized or not
    /// </summary>
    /// <example><code>
    /// [LocalizationRequiredAttribute(true)]
    /// public class Foo {
    ///   private string str = "my string"; // Warning: Localizable string
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class LocalizationRequiredAttribute : Attribute
    {
        public LocalizationRequiredAttribute() : this(true) { }
        public LocalizationRequiredAttribute(bool required)
        {
            this.Required = required;
        }

        public bool Required { get; private set; }
    }
}