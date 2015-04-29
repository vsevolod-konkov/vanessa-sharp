using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь LINQ-выражения в SQL-выражение.</summary>
    internal sealed class ExpressionTransformer : ExpressionToSqlObjectTransformer<SqlExpression>
    {
        /// <summary>Конструктор принимающий выражение записи данных.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        private ExpressionTransformer(
            QueryParseContext context,
            ParameterExpression recordExpression,
            IOneSMappingProvider mappingProvider)
            : base(context, recordExpression, mappingProvider)
        {}

        /// <summary>
        /// Получение результа преобразования.
        /// </summary>
        /// <returns></returns>
        protected override SqlExpression GetResult()
        {
            return GetExpression();
        }

        /// <summary>Преобразование выражения в SQL-выражение.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        public static SqlExpression Transform(
            IOneSMappingProvider mappingProvider,
            QueryParseContext context,
            LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
            Contract.Ensures(Contract.Result<SqlExpression>() != null);

            return Transform<Factory>(mappingProvider, context, sortKeyExpression);
        }

        private sealed class Factory : ITransformerFactory
        {
            public ExpressionToSqlObjectTransformer<SqlExpression> Create(
                QueryParseContext context,
                ParameterExpression recordExpression,
                IOneSMappingProvider mappingProvider)
            {
                return new ExpressionTransformer(context, recordExpression, mappingProvider);
            }
        }
    }
}
