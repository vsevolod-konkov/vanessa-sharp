using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь LINQ-выражение метода OrderBy[Descending] и ThenBy[Descending] в SQL-условие под ORDER BY.</summary>
    internal sealed class OrderByExpressionTransformer : FieldAccessExpressionTransformerBase<SqlFieldExpression>
    {
        /// <summary>Конструктор принимающий выражение записи данных.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="fieldAccessVisitorStrategy">Стратегия посещения доступа к полю.</param>
        /// <param name="fieldHolder">Хранитель выражения поля.</param>
        private OrderByExpressionTransformer(
            QueryParseContext context, FieldAccessVisitorStrategy fieldAccessVisitorStrategy, FieldHolder fieldHolder)
            : base(context, fieldAccessVisitorStrategy)
        {
            _fieldHolder = fieldHolder;
        }

        /// <summary>Выражение указывающее на поле сортировки.</summary>
        private readonly FieldHolder _fieldHolder;

        /// <summary>Получение результата трансформации.</summary>
        protected override SqlFieldExpression GetTransformResult()
        {
            return _fieldHolder.FieldExpression;
        }

        /// <summary>Фабрика преобразователя.</summary>
        private sealed class Factory : TransformerFactoryBase
        {
            public override FieldAccessExpressionTransformerBase<SqlFieldExpression> Create(
                IOneSMappingProvider mappingProvider, QueryParseContext context, ParameterExpression recordExpression)
            {
                var fieldHolder = new FieldHolder();
                var fieldAccessVisitorStrategy = CreateFieldAccessVisitorStrategy(
                    fieldHolder, recordExpression, mappingProvider);
                
                return new OrderByExpressionTransformer(context, fieldAccessVisitorStrategy, fieldHolder);
            }
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        public static SqlFieldExpression Transform(
            IOneSMappingProvider mappingProvider,
            QueryParseContext context,
            LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
            Contract.Ensures(Contract.Result<SqlFieldExpression>() != null);

            return Transform<Factory>(mappingProvider, context, sortKeyExpression);
        }

        /// <summary>Держатель выражения поля.</summary>
        private sealed class FieldHolder : IFieldAccessVisitorForOnlySql
        {
            /// <summary>Выражение поле.</summary>
            public SqlFieldExpression FieldExpression { get; private set; }

            /// <summary>Посещение узла доступа к полю.</summary>
            /// <param name="fieldExpression">SQL-Выражение поля.</param>
            void IFieldAccessVisitorForOnlySql.VisitFieldAccess(SqlFieldExpression fieldExpression)
            {
                if (FieldExpression != null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Неоднозначность результата поля для сортировки: \"{0}\" и \"{1}\".",
                        FieldExpression.FieldName, fieldExpression.FieldName));
                }

                FieldExpression = fieldExpression;
            }
        }
    }
}
