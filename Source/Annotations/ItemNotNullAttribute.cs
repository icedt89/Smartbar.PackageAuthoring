namespace JetBrains.Annotations
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Indicates that collection or enumerable value does not contain null elements
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
        AttributeTargets.Delegate | AttributeTargets.Field)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class ItemNotNullAttribute : Attribute { }
}