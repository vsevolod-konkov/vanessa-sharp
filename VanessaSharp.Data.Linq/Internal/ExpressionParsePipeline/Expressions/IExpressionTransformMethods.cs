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
    internal interface IExpressionTransformMethods : ISourceResolver
    {
        /// <summary>Преобразование LINQ-выражения метода Select.</summary>
        /// <typeparam name="TInput">Тип элементов исходной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности - результатов выборки.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="selectExpression">Преобразуемое выражение.</param>
        SelectionPartParseProduct<TOutput> TransformSelectExpression<TInput, TOutput>(
            QueryParseContext context, Expression<Func<TInput, TOutput>> selectExpression);

        /// <summary>Преобразование выражения в SQL-условие.</summary>
        /// <typeparam name="T">Тип элементов.</typeparam>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="predicateExpression">Выражение фильтрации.</param>
        SqlCondition TransformCondition<T>(
            QueryParseContext context,
            Expression<Func<T, bool>> predicateExpression);

        /// <summary>Преобразование выражения в SQL-выражения.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Выражение ключа сортировки.</param>
        SqlExpression TransformExpression(QueryParseContext context, LambdaExpression expression);

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
        /// <param name="predicateExpression">Выражение фильтрации.</param>
        SqlCondition 
            IExpressionTransformMethods.TransformCondition<T>(QueryParseContext context, Expression<Func<T, bool>> predicateExpression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(predicateExpression != null);
            Contract.Ensures(Contract.Result<SqlCondition>() != null);

            return null;
        }

        /// <summary>Преобразование выражения получения ключа сортировки в SQL-выражения поля под выражением ORDER BY.</summary>
        /// <param name="context">Контекст разбора запроса.</param>
        /// <param name="expression">Выражение ключа сортировки.</param>
        SqlExpression IExpressionTransformMethods.TransformExpression(QueryParseContext context, LambdaExpression expression)
        {
            Contract.Requires<ArgumentNullException>(context != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            Contract.Ensures(Contract.Result<SqlExpression>() != null);

            return null;
        }

        /// <summary>Преобразование получения типизированных записей.</summary>
        /// <typeparam name="T">Тип записей.</typeparam>
        SelectionPartParseProduct<T> IExpressionTransformMethods.TransformSelectTypedRecord<T>()
        {
            Contract.Ensures(Contract.Result<SelectionPartParseProduct<T>>() != null);

            return null;
        }

        /// <summary>Получение имени источника данных для типизированной записи.</summary>
        /// <typeparam name="T">Тип записи.</typeparam>
        public abstract string ResolveSourceNameForTypedRecord<T>();
    }
}