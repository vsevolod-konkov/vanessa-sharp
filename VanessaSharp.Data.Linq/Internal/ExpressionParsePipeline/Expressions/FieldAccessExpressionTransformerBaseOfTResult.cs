using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Базовый класс преобразования выражения с доступом к полям источника.</summary>
    /// <typeparam name="TResult">Тип результата преобразования.</typeparam>
    internal abstract class FieldAccessExpressionTransformerBase<TResult> : FieldAccessExpressionTransformerBase
        where TResult : class
    {
        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи данных.</param>
        protected FieldAccessExpressionTransformerBase(QueryParseContext context, ParameterExpression recordExpression)
            : base(context, recordExpression)
        {
        }

        /// <summary>Получение результата трансформации.</summary>
        protected abstract TResult GetTransformResult();

        /// <summary>
        /// Преобразование тела лямбда-выражения.
        /// </summary>
        /// <param name="lambdaBody">Тело преобразуемого лямбда-выражения.</param>
        private TResult TransformBody(Expression lambdaBody)
        {
            Contract.Requires<ArgumentNullException>(lambdaBody != null);
            Contract.Ensures(Contract.Result<TResult>() != null);

            Visit(lambdaBody);

            var result = GetTransformResult();

            if (result == null)
            {
                throw new ArgumentException(string.Format(
                        "Выражение \"{0}\" не было преобразовано к нужному типу.",
                        lambdaBody));
            }

            return result;
        }

        /// <summary>Преобразование лямбда-выражения.</summary>
        /// <typeparam name="TFactory">Тип фабрики преобразователя.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="lambda">Преобразуемое лямбда выражение.</param>
        protected static TResult Transform<TFactory>(QueryParseContext context, LambdaExpression lambda)
            where TFactory : TransformerFactoryBase,  new()
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(lambda != null);
            Contract.Requires<ArgumentException>(lambda.Type.GetGenericTypeDefinition() == typeof(Func<,>));
            Contract.Requires<ArgumentException>(lambda.Type.GetGenericArguments()[0] == typeof(OneSDataRecord));
            Contract.Ensures(Contract.Result<TResult>() != null);
            
            return new TFactory()
                .Create(context, lambda.Parameters[0])
                .TransformBody(lambda.Body);
        }

        /// <summary>
        /// Базовый класс фабрики преобразователя.
        /// </summary>
        protected abstract class TransformerFactoryBase
        {
            /// <summary>Создание преобразователя выражения.</summary>
            /// <param name="context">Контекст разбора запроса.</param>
            /// <param name="recordExpression">Выражение записи данных</param>
            public abstract FieldAccessExpressionTransformerBase<TResult> Create(
                QueryParseContext context, ParameterExpression recordExpression);
        }
    }
}
