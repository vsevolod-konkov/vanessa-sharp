using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Базовый класс преобразователя linq-выражения в sql-модель выражения или условия.
    /// </summary>
    /// <typeparam name="T">Тип результата преобразования.</typeparam>
    internal abstract class ExpressionToSqlObjectTransformer<T> : ExpressionTransformerBase
        where T : class
    {
        /// <summary>Стековая машина для генерации условия.</summary>
        private readonly SqlObjectBuilder _sqlConditionBuilder;

        /// <summary>Конструктор.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="recordExpression">Выражение записи.</param>
        /// <param name="mappingProvider">Поставщик соответствий типам CLR источников данных 1С.</param>
        protected ExpressionToSqlObjectTransformer(
            QueryParseContext context,
            ParameterExpression recordExpression,
            IOneSMappingProvider mappingProvider)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(recordExpression != null);
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            
            _sqlConditionBuilder = new SqlObjectBuilder(context, recordExpression, mappingProvider);
        }

        /// <summary>
        /// Получение модели условия.
        /// </summary>
        protected SqlCondition GetCondition()
        {
            Contract.Ensures(Contract.Result<SqlCondition>() != null);
            
            return _sqlConditionBuilder.GetCondition();
        }

        /// <summary>
        /// Получение модели выражения.
        /// </summary>
        /// <returns></returns>
        protected SqlExpression GetExpression()
        {
            Contract.Ensures(Contract.Result<SqlExpression>() != null);
            
            return _sqlConditionBuilder.GetExpression();
        }

        /// <summary>
        /// Получение результа преобразования.
        /// </summary>
        /// <returns></returns>
        protected abstract T GetResult();

        /// <summary>Преобразование лямбда-выражения.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="lambda">Преобразуемое лямбда выражение.</param>
        protected static T Transform<TFactory>(IOneSMappingProvider mappingProvider, QueryParseContext context, LambdaExpression lambda)
            where TFactory : ITransformerFactory, new()
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(lambda != null);
            Contract.Requires<ArgumentException>(lambda.Type.GetGenericTypeDefinition() == typeof(Func<,>));
            Contract.Ensures(Contract.Result<T>() != null);

            return new TFactory()
                .Create(context, lambda.Parameters[0], mappingProvider)
                .TransformBody(lambda.Body);
        }

        /// <summary>
        /// Преобразование тела лямбда-выражения.
        /// </summary>
        /// <param name="lambdaBody">Тело преобразуемого лямбда-выражения.</param>
        private T TransformBody(Expression lambdaBody)
        {
            Contract.Requires<ArgumentNullException>(lambdaBody != null);
            Contract.Ensures(Contract.Result<T>() != null);

            Visit(lambdaBody);

            return GetResult();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!_sqlConditionBuilder.HandleMethodCall(node))
                throw node.CreateExpressionNotSupportedException();

            return node;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitMember(MemberExpression node)
        {
            if (!_sqlConditionBuilder.HandleMember(node))
                throw node.CreateExpressionNotSupportedException();

            return node;
        }

        /// <summary>
        /// Просматривает выражение <see cref="T:System.Linq.Expressions.ConstantExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitConstant(ConstantExpression node)
        {
            if (!_sqlConditionBuilder.HandleConstant(node))
                throw node.CreateExpressionNotSupportedException();

            return node;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.BinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitBinary(BinaryExpression node)
        {
            var result = DefaultVisitBinary(node);

            if (!_sqlConditionBuilder.HandleBinary(node))
                throw node.CreateExpressionNotSupportedException();

            return result;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.UnaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected sealed override Expression VisitUnary(UnaryExpression node)
        {
            var result = DefaultVisitUnary(node);

            if (!_sqlConditionBuilder.HandleUnary(node))
                throw node.CreateExpressionNotSupportedException();

            return result;
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.TypeBinaryExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            var result = DefaultVisitTypeBinary(node);

            if (!_sqlConditionBuilder.HandleVisitTypeBinary(node))
                throw node.CreateExpressionNotSupportedException();

            return result;
        }

        #region Вспомогательные типы

        /// <summary>
        /// Фабрика преобразователя.
        /// </summary>
        protected interface ITransformerFactory
        {
            ExpressionToSqlObjectTransformer<T> Create(
                QueryParseContext context,
                ParameterExpression recordExpression,
                IOneSMappingProvider mappingProvider);
        }

        #endregion
    }
}