using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        /// <summary>Приватный конструктор для инициализаии параметра метода.</summary>
        private SelectExpressionTransformer(QueryParseContext context, ParameterExpression recordExpression, IOneSMappingProvider mappingProvider)
        {
            _expressionBuilder = new SqlObjectBuilder(context, recordExpression, mappingProvider);
        }

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="mappingProvider">Поставщик соответствий типов источникам данных 1С.</param>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Преобразуемое выражение.</param>
        public static SelectionPartParseProduct<TOutput> Transform<TInput, TOutput>(IOneSMappingProvider mappingProvider, QueryParseContext context, Expression<Func<TInput, TOutput>> expression)
        {
            Contract.Requires<ArgumentNullException>(mappingProvider != null);
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<TOutput>>() != null);

            return new SelectExpressionTransformer(
                context, expression.Parameters[0], mappingProvider)
                
                .TransformLambdaBody<TOutput>(expression.Body);
        }

        /// <summary>Преобразование тела лямбды метода выборки данных.</summary>
        /// <typeparam name="T">Тип элементов выборки.</typeparam>
        /// <param name="lambdaBody">Тело лямбды метода выборки.</param>
        /// <returns>
        /// Результат преобразования - набор колонок и делегат создания элемента из значений колонок.
        /// </returns>
        private SelectionPartParseProduct<T> TransformLambdaBody<T>(Expression lambdaBody)
        {
            var resultExpression = Visit(lambdaBody);

            return new SelectionPartParseProduct<T>(
                _columnExpressionBuilder.Columns,
                CreateItemReader<T>(resultExpression));
        }

        private readonly SqlObjectBuilder _expressionBuilder;

        /// <summary>Построитель выражений для колонок выборки.</summary>
        private readonly ColumnExpressionBuilder _columnExpressionBuilder = new ColumnExpressionBuilder();

        /// <summary>
        /// Создание делегата создателя элемента вычитываемого из записи.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="body">Тело лямбда-выражения создаваемого делегата.</param>
        private Func<IValueConverter, object[], T> CreateItemReader<T>(Expression body)
        {
            return Expression
                .Lambda<Func<IValueConverter, object[], T>>(body, _columnExpressionBuilder.ConverterParameter, _columnExpressionBuilder.ValuesParameter)
                .Compile();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <remarks>
        /// Заменяет методы получения значений из записи <see cref="OneSDataRecord"/>
        /// на получение значений из массива вычитанных значений.
        /// </remarks>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (_expressionBuilder.HandleMethodCall(node))
            {
                return _columnExpressionBuilder.GetColumnAccessExpression(
                            _expressionBuilder.GetExpression(),
                            node.Type);
            }

            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MemberExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMember(MemberExpression node)
        {
            if (_expressionBuilder.HandleMember(node))
            {
                return _columnExpressionBuilder.GetColumnAccessExpression(
                            _expressionBuilder.GetExpression(),
                            node.Type);
            }

            return base.VisitMember(node);
        }

        /// <summary>
        /// Просматривает выражение <see cref="T:System.Linq.Expressions.ParameterExpression"/>.
        /// </summary>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitParameter(ParameterExpression node)
        {
            throw new InvalidOperationException(
                "Недопустимо использовать запись данных в качестве члена в выходной структуре. Можно использовать в выражении запись только для доступа к ее полям.");
        }
    }
}
