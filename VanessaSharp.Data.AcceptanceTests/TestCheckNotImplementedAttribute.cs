using System;

namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>
    /// Маркировка тестовых методов,
    /// проверяющих, что нереализованные в текущей версии методы
    /// выдают корректные исключения.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class TestCheckNotImplementedAttribute : Attribute
    {}
}
