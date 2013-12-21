namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс типа значения.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSValueType))]
    public interface IValueType : IGlobalContextBound
    {}
}
