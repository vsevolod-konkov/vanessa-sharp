using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Маркировка перечислений,
    /// соответствующих перечислениям 1С.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    internal sealed class OneSEnumAttribute : Attribute
    {}
}
