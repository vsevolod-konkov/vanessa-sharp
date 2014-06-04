using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Стандартная реализация <see cref="IQueryableExpressionHandler"/></summary>
    /// <remarks>
    /// Преобразовывает выражение генерируемое <see cref="Queryable"/>
    /// в объект запроса <see cref="SimpleQuery"/>.
    /// </remarks>
    internal sealed class SimpleQueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>Состояния построителя-обработчика.</summary>
        private enum HandlerState
        {
            Starting,
            Started,
            Enumerable,
            Selected,
            Records,
            Ended,
        }

        /// <summary>Текущее состояние.</summary>
        private HandlerState _currentState = HandlerState.Starting;

        /// <summary>Тип элементов в последовательности.</summary>
        private Type _itemType;

        /// <summary>Источник данных.</summary>
        private string _sourceName;

        /// <summary>Выражение фильтрации записей.</summary>
        private Expression<Func<OneSDataRecord, bool>> _filterExpression;

        /// <summary>Выражение сортировки.</summary>
        private SortExpression _sortExpression;

        /// <summary>Фабрика создания запроса.</summary>
        private Func<string, SimpleQuery> _queryFactory;

        /// <summary>Конструктор.</summary>
        public SimpleQueryBuilder()
        {
            _queryFactory = source => new DataRecordsQuery(
                source, _filterExpression, CreateSortExpressionList());
        }

        /// <summary>Создание списка выражений сортировки.</summary>
        private ReadOnlyCollection<SortExpression> CreateSortExpressionList()
        {
            var array = (_sortExpression == null)
                            ? new SortExpression[0]
                            : new [] {_sortExpression};

            return new ReadOnlyCollection<SortExpression>(array);
        }

        /// <summary>Обработка начала парсинга.</summary>
        public void HandleStart()
        {
            if (_currentState != HandlerState.Starting)
               ThrowException(MethodBase.GetCurrentMethod());

            _currentState = HandlerState.Started;
        }

        /// <summary>Обработка завершения парсинга.</summary>
        public void HandleEnd()
        {
            if (_currentState != HandlerState.Records)
                ThrowException(MethodBase.GetCurrentMethod());

            _builtQuery = _queryFactory(_sourceName);
            _currentState = HandlerState.Ended;
        }

        /// <summary>Получение перечислителя.</summary>
        /// <param name="itemType">Имя элемента.</param>
        public void HandleGettingEnumerator(Type itemType)
        {
            if (_currentState != HandlerState.Started)
                ThrowException(MethodBase.GetCurrentMethod());

            _itemType = itemType;
            _currentState = HandlerState.Enumerable;
        }

        /// <summary>Обработка выборки.</summary>
        /// <param name="selectExpression">Выражение выборки.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            if (_currentState != HandlerState.Enumerable)
                ThrowException(MethodBase.GetCurrentMethod());

            if (_itemType != selectExpression.ReturnType)
            {
                throw new InvalidOperationException(string.Format(
                    "Тип \"{0}\" результата выборки не приемлем. Ожидался тип \"{1}\".",
                    selectExpression.ReturnType, _itemType));
            }

            _queryFactory = source => CreateDataTypeQuery(_sourceName, _filterExpression, CreateSortExpressionList(), selectExpression);
            _currentState = HandlerState.Selected;
        }

        /// <summary>Обработка фильтрации.</summary>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        public void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            if (_currentState == HandlerState.Enumerable
                ||
                _currentState == HandlerState.Selected)
            {
                _filterExpression = filterExpression;
                return;
            }
            
            ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>Обработка старта сортировки.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            HandleOrderBy(sortKeyExpression, SortKind.Ascending);
        }

        /// <summary>Обработка старта сортировки, начиная с сортировки по убыванию..</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            HandleOrderBy(sortKeyExpression, SortKind.Descending);
        }

        /// <summary>Обработка старта сортировки.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        /// <param name="sortKind">Порядок сортировки.</param>
        private void HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
        {
            if (_currentState == HandlerState.Enumerable
                ||
                _currentState == HandlerState.Selected)
            {
                var lambdaType = sortKeyExpression.Type;
                if (lambdaType.GetGenericTypeDefinition() != typeof(Func<,>) ||
                    lambdaType.GetGenericArguments()[0] != typeof(OneSDataRecord))
                {
                    throw new ArgumentException(string.Format(
                        "В текущем состоянии выражение сортировки \"{0}\" типа \"{1}\" не допустимо.",
                        sortKeyExpression, lambdaType));
                }

                _sortExpression = new SortExpression(sortKeyExpression, sortKind);

                return;
            }

            ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>Создание запроса коллекции элементов кастомного типа.</summary>
        /// <param name="source">Имя источника.</param>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        /// <param name="sortExpressionList">Список выражений сортировки.</param>
        /// <param name="selectExpression">Выражение выборки.</param>
        private static SimpleQuery CreateDataTypeQuery(string source, Expression<Func<OneSDataRecord, bool>> filterExpression, ReadOnlyCollection<SortExpression> sortExpressionList, LambdaExpression selectExpression)
        {
            var queryType = typeof(CustomDataTypeQuery<>).MakeGenericType(selectExpression.ReturnType);
            return (SimpleQuery)Activator.CreateInstance(queryType, source, filterExpression, sortExpressionList, selectExpression);
        }

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        public void HandleGettingRecords(string sourceName)
        {
            if (_currentState != HandlerState.Enumerable && _currentState != HandlerState.Selected)
                ThrowException(MethodBase.GetCurrentMethod());

            _sourceName = sourceName;
            _currentState = HandlerState.Records;
        }

        /// <summary>Построенный запрос, после обработки выражения.</summary>
        public SimpleQuery BuiltQuery
        {
            get
            {
                if (_currentState != HandlerState.Ended)
                    ThrowException(MethodBase.GetCurrentMethod());

                return _builtQuery;
            }
        }
        private SimpleQuery _builtQuery;

        /// <summary>Создание исключения недопустимой операции.</summary>
        /// <param name="method">Метод вызываемой операции.</param>
        private void ThrowException(MethodBase method)
        {
            throw new InvalidOperationException(string.Format(
                "Недопустимо вызывать метод \"{0}\" в состоянии \"{1}\".",
                method, _currentState));
        }
    }
}