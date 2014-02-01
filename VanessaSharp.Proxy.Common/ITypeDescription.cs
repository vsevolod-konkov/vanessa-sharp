namespace VanessaSharp.Proxy.Common
{
    /// <summary>Описание типа 1С.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSTypeDescription))]
    public interface ITypeDescription : IGlobalContextBound
    {
        /// <summary>Типы в описании.</summary>
        IOneSArray<IOneSType> Types { get; }
    }
}
