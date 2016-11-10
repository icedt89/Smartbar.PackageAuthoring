namespace JetBrains.Annotations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Indicates how method invocation affects content of the collection
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class CollectionAccessAttribute : Attribute
    {
        public CollectionAccessAttribute(CollectionAccessType collectionAccessType)
        {
            this.CollectionAccessType = collectionAccessType;
        }

        public CollectionAccessType CollectionAccessType { get; private set; }
    }
}