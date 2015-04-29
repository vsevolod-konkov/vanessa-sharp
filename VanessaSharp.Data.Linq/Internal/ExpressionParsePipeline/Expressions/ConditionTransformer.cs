using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь LINQ-выражения в SQL-условие.</summary>
    internal sealed class ConditionTransformer : ExpressionToSqlObjectTransformer<SqlCondition>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        private ConditionTransformer(QueryParseContext context,
                                           ParameterExpression recordExpression,
                                           IOneSMappingProvider mappingProvider)
            : base(context, recordExpression, mappingProvider)
        {}

        /// <summary>
        /// Получение результа преобразования.
        /// </summary>
        /// <returns></returns>
        protected override SqlCondition GetResult()
        {
            return GetCondition();
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип фильтруемых элементов.</typeparam>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        public static SqlCondition Transform<T>(
            IOneSMappingProvider mappingProvider,
            QueryParseContext context, 
            Expression<Func<T, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(filterExpression != null);
            Contract.Ensures(Contract.Result<SqlCondition>() != null);

            return Transform<Factory>(mappingProvider, context, filterExpression);
        }

        private sealed class Factory : ITransformerFactory
        {
            public ExpressionToSqlObjectTransformer<SqlCondition> Create(
                QueryParseContext context, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
            {
                return new ConditionTransformer(context, recordExpression, mappingProvider);
            }
        }
    }
}
