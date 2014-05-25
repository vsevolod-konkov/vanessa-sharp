using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    using M = OneSQueryExpressionHelper;
    
    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    internal sealed class SelectExpressionTransformer : ExpressionVisitorBase
    {
        private static readonly IDictionary<MethodInfo, MethodInfo>
            _methods = new Dictionary<MethodInfo, MethodInfo>
            {
                { M.DataRecordGetStringMethod, M.ValueConverterToStringMethod },
                { M.DataRecordGetInt32Method, M.ValueConverterToInt32Method },
                { M.DataRecordGetDoubleMethod, M.ValueConverterToDoubleMethod },
                { M.DataRecordGetDateTimeMethod, M.ValueConverterToDateTimeMethod },
                { M.DataRecordGetBooleanMethod, M.ValueConverterToBooleanMethod },
                { M.DataRecordGetCharMethod, M.ValueConverterToCharMethod },
            };

        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="T">Тип элемента.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Выражение.</param>
        public static SelectionPartParseProduct<T> Transform<T>(QueryParseContext context, Expression<Func<OneSDataRecord, T>> expression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<T>>() != null);

            var instance = new SelectExpressionTransformer(expression.Parameters[0]);
            var resultExpression = instance.Visit(expression.Body);

            return new SelectionPartParseProduct<T>(
                new ReadOnlyCollection<SqlExpression>(instance._columns), 
                instance.CreateItemReader<T>(resultExpression));
        }

        /// <summary>Выражение соответствующая записи данных из которой делается выборка.</summary>
        private readonly ParameterExpression _recordExpression;

        /// <summary>Выражения колонок.</summary>
        private readonly List<SqlExpression> _columns = new List<SqlExpression>();

        /// <summary>Имена колонок полей.</summary>
        /// <remarks>Для быстрого поиска.</remarks>
        private readonly List<string> _fieldNames = new List<string>(); 

        /// <summary>Параметр для результирующего делегата создания элемента - конвертер значений.</summary>
        private readonly ParameterExpression _converterParameter 
            = Expression.Parameter(typeof(IValueConverter), "valueConverter");

        /// <summary>Параметр для результирующего делегата создания элемента - массив вычитанных значений.</summary>
        private readonly ParameterExpression _valuesParameter
            = Expression.Parameter(typeof(object[]), "values");

        /// <summary>Конструктор, в который передается параметр исходного лямбда-выражения.</summary>
        /// <param name="recordExpression">Выражение записи данных, из которой производится вычитка.</param>
        private SelectExpressionTransformer(ParameterExpression recordExpression)
        {
            _recordExpression = recordExpression;
        }

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

        /// <summary>Получение индекса поля по имени.</summary>
        /// <param name="fieldName">Имя поля.</param>
        private int GetFieldIndex(string fieldName)
        {
            var index = _fieldNames.IndexOf(fieldName);
            if (index == -1)
                return AddNewField(fieldName);

            return index;
        }

        /// <summary>Добавление вычитки нового поля.</summary>
        private int AddNewField(string fieldName)
        {
            _columns.Add(new SqlFieldExpression(fieldName));
            _fieldNames.Add(fieldName);

            return _fieldNames.Count - 1;
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
                MethodInfo valueConverterMethod;
                if (_methods.TryGetValue(node.Method, out valueConverterMethod))
                    return GetGetValueAndConvertExpression(node, valueConverterMethod);

                throw CreateExpressionNotSupportedException(node);
            }
            
            return base.VisitMethodCall(node);
        }

        /// <summary>
        /// Получение выражения получения значения из массива и конвертация его к нужному типу.
        /// </summary>
        /// <param name="node">Исходный узел вызова метода получения данных из записи <see cref="OneSDataRecord"/>.</param>
        /// <param name="valueConverterMethod">Метод конвертации значения.</param>
        private Expression GetGetValueAndConvertExpression(MethodCallExpression node, MethodInfo valueConverterMethod)
        {
            var fieldName = GetConstant<string>(node.Arguments[0]);
            var index = GetFieldIndex(fieldName);

            var valueExpression = Expression.ArrayIndex(_valuesParameter, Expression.Constant(index));
            return Expression.Call(_converterParameter, valueConverterMethod, valueExpression);
        }
    }
}
