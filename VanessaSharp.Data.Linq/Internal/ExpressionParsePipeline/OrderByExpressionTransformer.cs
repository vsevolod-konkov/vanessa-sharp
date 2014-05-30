using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобразователь LINQ-выражение метода OrderBy[Descending] и ThenBy[Descending] в SQL-условие под ORDER BY.</summary>
    internal sealed class OrderByExpressionTransformer : FieldAccessExpressionTransformerBase<SqlFieldExpression>
    {
        /// <summary>Конструктор принимающий выражение записи данных.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        private OrderByExpressionTransformer(QueryParseContext context, ParameterExpression recordExpression)
            : base(context, recordExpression)
        {}

        /// <summary>Выражение указывающее на поле сортировки.</summary>
        private SqlFieldExpression _fieldExpression;

        /// <summary>Получение результата трансформации.</summary>
        protected override SqlFieldExpression GetTransformResult()
        {
            return _fieldExpression;
        }

        /// <summary>Фабрика преобразователя.</summary>
        private sealed class Factory : TransformerFactoryBase
        {
            public override FieldAccessExpressionTransformerBase<SqlFieldExpression> Create(QueryParseContext context, ParameterExpression recordExpression)
            {
                return new OrderByExpressionTransformer(context, recordExpression);
            }
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="context">Контекст разбора.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        public static SqlFieldExpression Transform(
            QueryParseContext context,
            LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
            Contract.Requires<ArgumentException>(sortKeyExpression.Type.GetGenericArguments()[0] == typeof(OneSDataRecord));
            Contract.Ensures(Contract.Result<SqlFieldExpression>() != null);

            return Transform<Factory>(context, sortKeyExpression);
        }

        /// <summary>Посещение доступа к полю.</summary>
        /// <param name="fieldExpression">Выражение доступа к полю источника.</param>
        protected override void VisitFieldAccess(SqlFieldExpression fieldExpression)
        {
            if (_fieldExpression != null)
            {
                throw new InvalidOperationException(string.Format(
                    "Неоднозначность результата поля для сортировки: \"{0}\" и \"{1}\".",
                    _fieldExpression.FieldName, fieldExpression.FieldName));
            }

            _fieldExpression = fieldExpression;
        }
    }
}
