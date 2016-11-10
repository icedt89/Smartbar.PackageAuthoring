namespace JetBrains.Annotations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// This attribute is intended to mark publicly available API
    /// which should not be removed and so is treated as used
    /// </summary>
    [MeansImplicitUse]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class PublicAPIAttribute : Attribute
    {
        public PublicAPIAttribute() { }
        public PublicAPIAttribute([NotNull] string comment)
        {
            this.Comment = comment;
        }

        public string Comment { get; private set; }
    }
}