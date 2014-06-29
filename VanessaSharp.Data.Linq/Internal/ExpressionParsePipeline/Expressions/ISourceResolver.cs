using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Разрешитель имен источников.</summary>
    [ContractClass(typeof(SourceResolverContract))]
    internal interface ISourceResolver
    {
        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        string ResolveSourceNameForTypedRecord<T>();
    }

    [ContractClassFor(typeof(ISourceResolver))]
    internal abstract class SourceResolverContract : ISourceResolver
    {
        string ISourceResolver.ResolveSourceNameForTypedRecord<T>()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return null;
        }
    }
}