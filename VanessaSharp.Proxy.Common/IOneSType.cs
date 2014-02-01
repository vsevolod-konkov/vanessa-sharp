namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс типа значения.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSType))]
    public interface IOneSType : IGlobalContextBound
    {}
}
