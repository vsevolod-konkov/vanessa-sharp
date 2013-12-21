using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Сервис определения типа объекта 1С.
    /// </summary>
    [ContractClass(typeof(IOneSTypeResolverContract))]
    public interface IOneSTypeResolver
    {
        /// <summary>
        /// Возвращение имени типа объекта в 1С
        /// соответствующего типу CLR.
        /// </summary>
        /// <param name="requestedType">
        /// Тип CLR для которого ищется соответствие среди типов 1С.
        /// </param>
        string GetTypeNameFor(Type requestedType);
    }

    [ContractClassFor(typeof(IOneSTypeResolver))]
    internal abstract class IOneSTypeResolverContract : IOneSTypeResolver
    {
        /// <summary>
        /// Возвращение имени типа объекта в 1С
        /// соответствующего типу CLR.
        /// </summary>
        /// <param name="requestedType">
        /// Тип CLR для которого ищется соответствие среди типов 1С.
        /// </param>
        string IOneSTypeResolver.GetTypeNameFor(Type requestedType)
        {
            Contract.Requires<ArgumentNullException>(requestedType != null);
            Contract.Ensures(!string.IsNullOrEmpty(Contract.Result<string>()));

            return default(string);
        }
    }
}
