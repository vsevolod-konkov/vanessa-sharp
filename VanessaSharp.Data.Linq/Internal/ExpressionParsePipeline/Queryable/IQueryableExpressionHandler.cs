using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Обработчик парсинга выражений генерируемых <see cref="Queryable"/>.
    /// </summary>
    [ContractClass(typeof(IQueryableExpressionHandlerContract))]
    internal interface IQueryableExpressionHandler
    {
        /// <summary>Обработка начала парсинга.</summary>
        void HandleStart();

        /// <summary>Обработка завершения парсинга.</summary>
        void HandleEnd();
        
        /// <summary>Получение перечислителя.</summary>
        /// <param name="itemType">Имя элемента.</param>
        void HandleGettingEnumerator(Type itemType);

        /// <summary>Обработка выборки.</summary>
        /// <param name="selectExpression">Выражение выборки.</param>
        void HandleSelect(LambdaExpression selectExpression);

        /// <summary>Обработка фильтрации.</summary>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        void HandleFilter(LambdaExpression filterExpression);

        /// <summary>Обработка старта сортировки.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void HandleOrderBy(LambdaExpression sortKeyExpression);

        /// <summary>Обработка старта сортировки, начиная с сортировки по убыванию..</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void HandleOrderByDescending(LambdaExpression sortKeyExpression);

        /// <summary>Обработка продолжения сортировки, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void HandleThenBy(LambdaExpression sortKeyExpression);

        /// <summary>Обработка продолжения сортировки по убыванию, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void HandleThenByDescending(LambdaExpression sortKeyExpression);

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        void HandleGettingRecords(string sourceName);

        /// <summary>Получение всех типизированных записей.</summary>
        /// <param name="dataType">Тип запрашиваемых записей.</param>
        void HandleGettingTypedRecords(Type dataType);
    }

    [ContractClassFor(typeof(IQueryableExpressionHandler))]
    internal abstract class IQueryableExpressionHandlerContract : IQueryableExpressionHandler
    {
        void IQueryableExpressionHandler.HandleStart()
        {}

        void IQueryableExpressionHandler.HandleEnd()
        {}

        void IQueryableExpressionHandler.HandleGettingEnumerator(Type itemType)
        {
            Contract.Requires<ArgumentNullException>(itemType != null);
        }

        void IQueryableExpressionHandler.HandleSelect(LambdaExpression selectExpression)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);
            Contract.Requires<ArgumentException>(
                selectExpression.Type.IsGenericType 
                && selectExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
        }

        void IQueryableExpressionHandler.HandleFilter(LambdaExpression filterExpression)
        {
            Contract.Requires<ArgumentNullException>(filterExpression != null);
        }

        void IQueryableExpressionHandler.HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(
               sortKeyExpression.Type.IsGenericType
               && sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
        }

        void IQueryableExpressionHandler.HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(
                sortKeyExpression.Type.IsGenericType
                && sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
        }

        void IQueryableExpressionHandler.HandleThenBy(LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(
               sortKeyExpression.Type.IsGenericType
               && sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
        }

        void IQueryableExpressionHandler.HandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            Contract.Requires<ArgumentNullException>(sortKeyExpression != null);
            Contract.Requires<ArgumentException>(
               sortKeyExpression.Type.IsGenericType
               && sortKeyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));
        }

        void IQueryableExpressionHandler.HandleGettingRecords(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
        }

        void IQueryableExpressionHandler.HandleGettingTypedRecords(Type dataType)
        {
            Contract.Requires<ArgumentNullException>(dataType != null);
        }
    }
}
