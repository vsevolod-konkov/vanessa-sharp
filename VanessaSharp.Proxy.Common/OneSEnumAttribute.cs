using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Маркировка перечислений,
    /// соответствующих перечислениям 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class OneSEnumAttribute : Attribute
    {}
}
