using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Преобразователь запросов.
    /// </summary>
    [ContractClass(typeof(QueryTransformerContract))]
    internal interface IQueryTransformer
    {
        /// <summary>Преобразовывает запросы в результат парсинга LINQ-выражения готового в выполнению.</summary>
        /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
        /// <param name="query">Объект запроса.</param>
        CollectionReadExpressionParseProduct<TOutput> Transform<TInput, TOutput>(IQuery<TInput, TOutput> query);
    }

    [ContractClassFor(typeof(IQueryTransformer))]
    internal abstract class QueryTransformerContract : IQueryTransformer
    {
        /// <summary>Преобразовывает запросы в результат парсинга LINQ-выражения готового в выполнению.</summary>
        /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
        /// <param name="query">Объект запроса.</param>
        CollectionReadExpressionParseProduct<TOutput> IQueryTransformer.Transform<TInput, TOutput>(IQuery<TInput, TOutput> query)
        {
            Contract.Requires<ArgumentNullException>(query != null);
            Contract.Ensures(Contract.Result<CollectionReadExpressionParseProduct<TOutput>>() != null);

            return null;
        }
    }
}
