using System;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Атрибут, которым маркируются члены реализаций общих контрактов,
    /// которые в текущей версии не реализованы,
    /// но чья реализация планируется в следующих версиях.
    /// </summary>
    [AttributeUsage
        ( AttributeTargets.Event 
        | AttributeTargets.Method 
        | AttributeTargets.Property)]
    public sealed class CurrentVersionNotImplementedAttribute : Attribute
    {}
}
