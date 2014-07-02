using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        // TODO Рефакторинг
        private static readonly ISet<MethodInfo>
            _methods = new HashSet<MethodInfo>
            {
                OneSQueryExpressionHelper.DataRecordGetStringMethod,
                OneSQueryExpressionHelper.DataRecordGetCharMethod,
                OneSQueryExpressionHelper.DataRecordGetByteMethod,
                OneSQueryExpressionHelper.DataRecordGetInt16Method,
                OneSQueryExpressionHelper.DataRecordGetInt32Method,
                OneSQueryExpressionHelper.DataRecordGetInt64Method,
                OneSQueryExpressionHelper.DataRecordGetFloatMethod,
                OneSQueryExpressionHelper.DataRecordGetDoubleMethod,
                OneSQueryExpressionHelper.DataRecordGetDecimalMethod,
                OneSQueryExpressionHelper.DataRecordGetDateTimeMethod,
                OneSQueryExpressionHelper.DataRecordGetBooleanMethod,
            };

        /// <summary>Приватный конструктор для инициализаии параметра метода.</summary>
        /// <param name="recordExpression">Параметр метода - выражение записи из которой производиться выборка.</param>
        /// <param name="typeMapping">Соответствие трансформируемого типа источнику данных.</param>
        private SelectExpressionTransformer(ParameterExpression recordExpression, OneSTypeMapping typeMapping)
        {
            Contract.Requires<ArgumentNullException>(recordExpression != null);

            _recordExpression = recordExpression;
            _typeMapping = typeMapping;
            _columnExpressionBuilder = new ColumnExpressionBuilder(_converterParameter, _valuesParameter);
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

            var typeMapping = (typeof (TInput) == typeof(OneSDataRecord))
                                  ? null
                                  : mappingProvider.GetTypeMapping(typeof(TInput));

            return new SelectExpressionTransformer(
                expression.Parameters[0], 
                typeMapping
                )
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

        /// <summary>Выражение соответствующая записи данных из которой делается выборка.</summary>
        private readonly ParameterExpression _recordExpression;

        /// <summary>
        /// Соответствие трансформируемого типа источнику данных.
        /// </summary>
        private readonly OneSTypeMapping _typeMapping;

        /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
        private readonly ParameterExpression _converterParameter 
            = Expression.Parameter(typeof(IValueConverter), "valueConverter");

        /// <summary>Параметр для результирующего делегата создания элемента - массив вычитанных значений.</summary>
        private readonly ParameterExpression _valuesParameter
            = Expression.Parameter(typeof(object[]), "values");

        /// <summary>Построитель выражений для колонок выборки.</summary>
        private readonly ColumnExpressionBuilder _columnExpressionBuilder;

        /// <summary>
        /// Создание делегата создателя элемента вычитываемого из записи.
        /// </summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="body">Тело лямбда-выражения создаваемого делегата.</param>
        private Func<IValueConverter, object[], T> CreateItemReader<T>(Expression body)
        {
            return Expression
                .Lambda<Func<IValueConverter, object[], T>>(body, _converterParameter, _valuesParameter)
                .Compile();
        }

        /// <summary>
        /// Просматривает дочерний элемент выражения <see cref="T:System.Linq.Expressions.MethodCallExpression"/>.
        /// </summary>
        /// <remarks>
        /// Заменяет методы получения значений из записи <see cref="OneSDataRecord"/>
        /// на получение значений из массива <see cref="_valuesParameter"/> вычитанных значений.
        /// </remarks>
        /// <returns>
        /// Измененное выражение в случае изменения самого выражения или любого его подвыражения; в противном случае возвращается исходное выражение.
        /// </returns>
        /// <param name="node">Выражение, которое необходимо просмотреть.</param>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Object == _recordExpression)
            {
                if (_methods.Contains(node.Method))
                {
                    var fieldName = GetConstant<string>(node.Arguments[0]);

                    return _columnExpressionBuilder.GetColumnAccessExpression(fieldName, node.Type);
                }

                throw CreateExpressionNotSupportedException(node);
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
            if (_typeMapping != null)
            {
                var fieldName = _typeMapping.GetFieldNameByMemberInfo(node.Member);
                return _columnExpressionBuilder
                    .GetColumnAccessExpression(fieldName, node.Type);
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
