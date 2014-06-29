using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
    /// в <see cref="IQuery"/>.
    /// </summary>
    [ContractClass(typeof(QueryableExpressionTransformerContract))]
    internal interface IQueryableExpressionTransformer
    {
        /// <summary>
        /// Преобразование linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="IQuery"/>.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        IQuery Transform(Expression expression);
    }

    [ContractClassFor(typeof(IQueryableExpressionTransformer))]
    internal abstract class QueryableExpressionTransformerContract : IQueryableExpressionTransformer
    {
        /// <summary>
        /// Преобразование linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="IQuery"/>.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        IQuery IQueryableExpressionTransformer.Transform(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<IQuery>() != null);

            return null;
        }
    }
}
