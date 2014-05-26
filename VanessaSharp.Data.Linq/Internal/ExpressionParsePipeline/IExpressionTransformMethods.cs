using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Методы преобразования LINQ-выражений методов-запросов.
    /// </summary>
    [ContractClass(typeof(ExpressionTransformMethodsContract))]
    internal interface IExpressionTransformMethods
    {
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="T">Тип элементов последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        SelectionPartParseProduct<T> TransformSelectExpression<T>(
            QueryParseContext context, Expression<Func<OneSDataRecord, T>> selectExpression);

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        SqlCondition TransformWhereExpression(
            QueryParseContext context,
            Expression<Func<OneSDataRecord, bool>> filterExpression);
    }

    /// <summary>
    /// Преобразователь выражения метода Select в SQL-инструкцию SELECT и в делегат для вычитки элемента данных из записи.
    /// </summary>
    [ContractClassFor(typeof(IExpressionTransformMethods))]
    internal abstract class ExpressionTransformMethodsContract : IExpressionTransformMethods
    {
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="T">Тип элементов последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        SelectionPartParseProduct<T>
            IExpressionTransformMethods.TransformSelectExpression<T>(QueryParseContext context, Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(selectExpression != null);
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<T>>() != null);

            return null;
        }

        /// <summary>Преобразование выражения в SQL-условие WHERE.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="filterExpression">Фильтрация выражения.</param>
        SqlCondition 
            IExpressionTransformMethods.TransformWhereExpression(QueryParseContext context, Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(filterExpression != null);
            Contract.Ensures(Contract.Result<SqlCondition>() != null);

            return null;
        }
    }
}