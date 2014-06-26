using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Методы преобразования LINQ-выражений методов-запросов.
    /// </summary>
    [ContractClass(typeof(ExpressionTransformMethodsContract))]
    internal interface IExpressionTransformMethods
    {
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        SelectionPartParseProduct<TOutput> TransformSelectExpression<TInput, TOutput>(
            QueryParseContext context, Expression<Func<TInput, TOutput>> selectExpression);

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        SqlCondition TransformWhereExpression<T>(
            QueryParseContext context,
            Expression<Func<T, bool>> filterExpression);

        /// <summary>Преобразование выражения получения ключа сортировки в SQL-выражения поля под выражением ORDER BY.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        SqlFieldExpression TransformOrderByExpression(QueryParseContext context, LambdaExpression sortKeyExpression);

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        string GetTypedRecordSourceName<T>();
        
        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        SelectionPartParseProduct<T> TransformSelectTypedRecord<T>();
    }

    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    [ContractClassFor(typeof(IExpressionTransformMethods))]
    internal abstract class ExpressionTransformMethodsContract : IExpressionTransformMethods
    {
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        SelectionPartParseProduct<TOutput> IExpressionTransformMethods.TransformSelectExpression<TInput, TOutput>(
           QueryParseContext context, Expression<Func<TInput, TOutput>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(selectExpression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<TOutput>>() != null);

            return null;
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        SqlCondition 
            IExpressionTransformMethods.TransformWhereExpression<T>(QueryParseContext context, Expression<Func<T, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(filterExpression != null);
            Contract.Ensures(Contract.Result<SqlCondition>() != null);

            return null;
        }

        /// <summary>Преобразование выражения получения ключа сортировки в SQL-выражения поля под выражением ORDER BY.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="sortKeyExpression">Выражение ключа сортировки.</param>
        SqlFieldExpression IExpressionTransformMethods.TransformOrderByExpression(QueryParseContext context, LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Ensures(Contract.Result<SqlFieldExpression>() != null);

            return null;
        }

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        string IExpressionTransformMethods.GetTypedRecordSourceName<T>()
        {
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

            return null;
        }

        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        SelectionPartParseProduct<T> IExpressionTransformMethods.TransformSelectTypedRecord<T>()
        {
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<T>>() != null);

            return null;
        }
    }
}