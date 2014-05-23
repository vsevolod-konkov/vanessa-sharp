﻿using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
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
        void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression);

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        void HandleGettingRecords(string sourceName);
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
                && selectExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>)
                && selectExpression.Type.GetGenericArguments()[0] == typeof(OneSDataRecord));
        }

        public void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            Contract.Requires<ArgumentNullException>(filterExpression != null);
        }

        void IQueryableExpressionHandler.HandleGettingRecords(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
        }
    }
}
