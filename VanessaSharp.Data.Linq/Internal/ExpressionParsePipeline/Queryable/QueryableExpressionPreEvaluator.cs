using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>
    /// Обработчик выражений <see cref="IQueryable"/> производящий предвычисления.
    /// Подробности <see cref="PreEvaluator"/>.
    /// </summary>
    internal sealed class QueryableExpressionPreEvaluator : IQueryableExpressionHandler
    {
        /// <summary>
        /// Следующий обработчик в цепочке.
        /// </summary>
        private readonly IQueryableExpressionHandler _internalHandler;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="internalHandler">
        /// Следующий обработчик в цепочке.
        /// </param>
        public QueryableExpressionPreEvaluator(IQueryableExpressionHandler internalHandler)
        {
            Contract.Requires<ArgumentNullException>(internalHandler != null);
            
            _internalHandler = internalHandler;
        }

        /// <summary>Обработка начала парсинга.</summary>
        void IQueryableExpressionHandler.HandleStart()
        {
            _internalHandler.HandleStart();
        }

        /// <summary>Обработка завершения парсинга.</summary>
        void IQueryableExpressionHandler.HandleEnd()
        {
            _internalHandler.HandleEnd();
        }

        /// <summary>Получение перечислителя.</summary>
        /// <param name="itemType">Имя элемента.</param>
        void IQueryableExpressionHandler.HandleGettingEnumerator(Type itemType)
        {
            _internalHandler.HandleGettingEnumerator(itemType);
        }

        /// <summary>Обработка агрегации данных.</summary>
        /// <param name="outputItemType">Тип элементов выходной последовательности.</param>
        /// <param name="function">Функция агрегации.</param>
        /// <param name="scalarType">Тип результата.</param>
        void IQueryableExpressionHandler.HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
        {
            _internalHandler.HandleAggregate(outputItemType, function, scalarType);
        }

        /// <summary>Обработка выборки различных записей.</summary>
        void IQueryableExpressionHandler.HandleDistinct()
        {
            _internalHandler.HandleDistinct();
        }

        /// <summary>Обработка взятия ограниченного количетсва элементов.</summary>
        /// <param name="count">Максимальное количество элементов.</param>
        void IQueryableExpressionHandler.HandleTake(int count)
        {
            _internalHandler.HandleTake(count);
        }

        /// <summary>Обработка выборки.</summary>
        /// <param name="selectExpression">Выражение выборки.</param>
        void IQueryableExpressionHandler.HandleSelect(LambdaExpression selectExpression)
        {
            _internalHandler.HandleSelect(
                PreEvaluator.Evaluate(selectExpression));
        }

        /// <summary>Обработка фильтрации.</summary>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        void IQueryableExpressionHandler.HandleFilter(LambdaExpression filterExpression)
        {
            _internalHandler.HandleFilter(
                PreEvaluator.Evaluate(filterExpression));
        }

        /// <summary>Обработка старта сортировки.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void IQueryableExpressionHandler.HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            _internalHandler.HandleOrderBy(
                PreEvaluator.Evaluate(sortKeyExpression));
        }

        /// <summary>Обработка старта сортировки, начиная с сортировки по убыванию..</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void IQueryableExpressionHandler.HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            _internalHandler.HandleOrderByDescending(
                PreEvaluator.Evaluate(sortKeyExpression));
        }

        /// <summary>Обработка продолжения сортировки, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void IQueryableExpressionHandler.HandleThenBy(LambdaExpression sortKeyExpression)
        {
            _internalHandler.HandleThenBy(
                PreEvaluator.Evaluate(sortKeyExpression));
        }

        /// <summary>Обработка продолжения сортировки по убыванию, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        void IQueryableExpressionHandler.HandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            _internalHandler.HandleThenByDescending(
               PreEvaluator.Evaluate(sortKeyExpression));
        }

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        void IQueryableExpressionHandler.HandleGettingRecords(string sourceName)
        {
            _internalHandler.HandleGettingRecords(sourceName);
        }

        /// <summary>Получение всех типизированных записей.</summary>
        /// <param name="dataType">Тип запрашиваемых записей.</param>
        void IQueryableExpressionHandler.HandleGettingTypedRecords(Type dataType)
        {
            _internalHandler.HandleGettingTypedRecords(dataType);
        }
    }
}
