using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Интерфейс запроса.</summary>
    [ContractClass(typeof(QueryContract))]
    internal interface IQuery
    {
        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        /// <param name="transformService">Сервис преобразования запросов.</param>
        ExpressionParseProduct Transform(IQueryTransformService transformService);
    }

    [ContractClassFor(typeof(IQuery))]
    internal abstract class QueryContract : IQuery
    {
        /// <summary>Преобразование результат парсинга запроса, готового к выполенению.</summary>
        /// <param name="transformService">Сервис преобразования запросов.</param>
        ExpressionParseProduct IQuery.Transform(IQueryTransformService transformService)
        {
            Contract.Requires<ArgumentNullException>(transformService != null);
            Contract.Ensures(Contract.Result<ExpressionParseProduct>() != null);

            return null;
        }
    }
}
