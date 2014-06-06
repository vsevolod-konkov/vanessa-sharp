using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Стандартная реализация <see cref="IExpressionParser"/>.</summary>
    internal sealed class ExpressionParser : IExpressionParser
    {
        /// <summary>Экземпляр по умолчанию.</summary>
        public static ExpressionParser Default
        {
            get { return _default; }
        }
        private static readonly ExpressionParser _default = new ExpressionParser();

        /// <summary>Конструктор используемый для тестирования.</summary>
        /// <param name="queryableExpressionTransformer">
        /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="SimpleQuery"/>.
        /// </param>
        internal ExpressionParser(IQueryableExpressionTransformer queryableExpressionTransformer)
        {
            Contract.Requires<ArgumentNullException>(queryableExpressionTransformer != null);
            
            _queryableExpressionTransformer = queryableExpressionTransformer;
        }

        /// <summary>Конструктор для инициализации экзеемпляра по умолчанию.</summary>
        private ExpressionParser() : this(QueryableExpressionTransformer.Default)
        {}

        /// <summary>
        /// Преобразователь linq-выражения <see cref="IQueryable{T}"/>
        /// в <see cref="SimpleQuery"/>.
        /// </summary>
        private readonly IQueryableExpressionTransformer _queryableExpressionTransformer;

        /// <summary>Разбор выражения.</summary>
        /// <param name="expression">Выражение.</param>
        public ExpressionParseProduct Parse(Expression expression)
        {
            return _queryableExpressionTransformer
                .Transform(expression)
                .Transform();
        }
    }
}
