using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
    /// в <see cref="SimpleQuery"/>.
    /// </summary>
    [ContractClass(typeof(IQueryableExpressionTransformerContract))]
    internal interface IQueryableExpressionTransformer
    {
        /// <summary>Преобразование linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="SimpleQuery"/>.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        ISimpleQuery Transform(Expression expression);
    }

    [ContractClassFor(typeof(IQueryableExpressionTransformer))]
    internal abstract class IQueryableExpressionTransformerContract : IQueryableExpressionTransformer
    {
        /// <summary>Преобразование linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="SimpleQuery"/>.
        /// </summary>
        /// <param name="expression">Выражение.</param>
        ISimpleQuery IQueryableExpressionTransformer.Transform(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<ISimpleQuery>() != null);

            return null;
        }
    }
}
