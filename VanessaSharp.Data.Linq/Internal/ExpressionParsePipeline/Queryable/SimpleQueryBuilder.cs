using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>Стандартная реализация <see cref="IQueryableExpressionHandler"/></summary>
    /// <remarks>
    /// Преобразовывает выражение генерируемое <see cref="Queryable"/>
    /// в объект запроса <see cref="SimpleQuery"/>.
    /// </remarks>
    internal sealed class SimpleQueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>Объект текущего состояния.</summary>
        private BuilderState _currentState;

        /// <summary>Конструктор.</summary>
        public SimpleQueryBuilder()
        {
            _currentState = new StartingState();
        }

        /// <summary>Обработка начала парсинга.</summary>
        public void HandleStart()
        {
            _currentState = _currentState.HandleStart();
        }

        /// <summary>Обработка завершения парсинга.</summary>
        public void HandleEnd()
        {
            _currentState = _currentState.HandleEnd();
        }

        /// <summary>Получение перечислителя.</summary>
        /// <param name="itemType">Имя элемента.</param>
        public void HandleGettingEnumerator(Type itemType)
        {
            _currentState = _currentState.HandleGettingEnumerator(itemType);
        }

        /// <summary>Обработка выборки.</summary>
        /// <param name="selectExpression">Выражение выборки.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            _currentState = _currentState.HandleSelect(selectExpression);
        }

        /// <summary>Обработка фильтрации.</summary>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        public void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            _currentState.HandleFilter(filterExpression);
        }

        /// <summary>Обработка старта сортировки.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleOrderBy(sortKeyExpression);
        }

        /// <summary>Обработка старта сортировки, начиная с сортировки по убыванию..</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleOrderByDescending(sortKeyExpression);
        }

        /// <summary>Обработка продолжения сортировки, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleThenBy(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleThenBy(sortKeyExpression);
        }

        /// <summary>Обработка продолжения сортировки по убыванию, по вторичным ключам.</summary>
        /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
        public void HandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleThenByDescending(sortKeyExpression);
        }

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        public void HandleGettingRecords(string sourceName)
        {
            _currentState = _currentState.HandleGettingRecords(sourceName);
        }

        /// <summary>Получение всех типизированных записей.</summary>
        /// <param name="dataType">Тип запрашиваемых записей.</param>
        public void HandleGettingTypedRecords(Type dataType)
        {
            _currentState = _currentState.HandleGettingTypedRecords(dataType);
        }

        /// <summary>Построенный запрос, после обработки выражения.</summary>
        public ISimpleQuery BuiltQuery
        {
            get { return _currentState.BuiltQuery; }
        }

        /// <summary>Создание <see cref="TupleQuery{T}"/>.</summary>
        /// <param name="dataType">Тип данных.</param>
        private static ISimpleQuery CreateTupleQuery(Type dataType)
        {
            var queryType = typeof(TupleQuery<>).MakeGenericType(dataType);
            return (ISimpleQuery)Activator.CreateInstance(queryType);
        }

        #region Классы состояний

        /// <summary>Интерфейс данных состояния.</summary>
        private interface IStateData
        {
            /// <summary>Выражение фильтрации записей.</summary>
            Expression<Func<OneSDataRecord, bool>> FilterExpression { set; }

            /// <summary>Добавление сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind);

            /// <summary>Создание объекта запроса.</summary>
            /// <param name="sourceName">Имя источника.</param>
            SimpleQuery CreateQuery(string sourceName);
        }

        /// <summary>Данные состояний когда не происходило выборки.</summary>
        private sealed class StateDataWithoutSelection : IStateData
        {
            public StateDataWithoutSelection(Type itemType)
            {
                _itemType = itemType;
            }
            
            /// <summary>Тип элементов в последовательности.</summary>
            public Type ItemType
            {
                get { return _itemType; }
            }
            private readonly Type _itemType;

            /// <summary>Выражение фильтрации записей.</summary>
            public Expression<Func<OneSDataRecord, bool>> FilterExpression { get; set; }

            /// <summary>Добавление сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                _sortExpressionStack.Push(new SortExpression(sortKeyExpression, sortKind));
            }

            /// <summary>Создание списка выражений сортировки.</summary>
            public ReadOnlyCollection<SortExpression> CreateSortExpressionList()
            {
                return new ReadOnlyCollection<SortExpression>(
                    _sortExpressionStack.ToArray());
            }
            
            /// <summary>Выражение сортировки.</summary>
            private readonly Stack<SortExpression> _sortExpressionStack = new Stack<SortExpression>();

            /// <summary>Создание объекта запроса.</summary>
            /// <param name="sourceName">Имя источника.</param>
            public SimpleQuery CreateQuery(string sourceName)
            {
                return new DataRecordsQuery(sourceName, FilterExpression, CreateSortExpressionList());
            }
        }

        /// <summary>Данные состояния с выражением выборки.</summary>
        private sealed class SelectionStateData : IStateData
        {
            /// <summary>Данные без выборки.</summary>
            private readonly StateDataWithoutSelection _stateData;

            /// <summary>Выражение выборки данных.</summary>
            private readonly LambdaExpression _selectExpression;

            /// <summary>Конструктор.</summary>
            /// <param name="stateData">Данные без выборки.</param>
            /// <param name="selectExpression">Выражение выборки данных.</param>
            public SelectionStateData(StateDataWithoutSelection stateData, LambdaExpression selectExpression)
            {
                _stateData = stateData;
                _selectExpression = selectExpression;
            }

            /// <summary>Выражение фильтрации записей.</summary>
            public Expression<Func<OneSDataRecord, bool>> FilterExpression
            {
                set { _stateData.FilterExpression = value; }
            }

            /// <summary>Добавление сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                _stateData.AddSort(sortKeyExpression, sortKind);
            }

            /// <summary>Создание объекта запроса.</summary>
            /// <param name="sourceName">Имя источника.</param>
            public SimpleQuery CreateQuery(string sourceName)
            {
                return CreateDataTypeQuery(sourceName, _stateData.FilterExpression,
                                           _stateData.CreateSortExpressionList(), _selectExpression);
            }

            /// <summary>Создание запроса коллекции элементов кастомного типа.</summary>
            /// <param name="source">Имя источника.</param>
            /// <param name="filterExpression">Выражение фильтрации.</param>
            /// <param name="sortExpressionList">Список выражений сортировки.</param>
            /// <param name="selectExpression">Выражение выборки.</param>
            private static SimpleQuery CreateDataTypeQuery(
                string source, Expression<Func<OneSDataRecord, bool>> filterExpression,
                ReadOnlyCollection<SortExpression> sortExpressionList, LambdaExpression selectExpression)
            {
                var queryType = typeof(CustomDataTypeQuery<>).MakeGenericType(selectExpression.ReturnType);
                return (SimpleQuery)Activator.CreateInstance(queryType, source, filterExpression, sortExpressionList, selectExpression);
            }
        }

        /// <summary>Базовый класс состояний.</summary>
        private abstract class BuilderState
        {
            /// <summary>Создание исключения недопустимой операции.</summary>
            /// <param name="method">Метод вызываемой операции.</param>
            private Exception CreateException(MethodBase method)
            {
                throw new InvalidOperationException(string.Format(
                    "Недопустимо вызывать метод \"{0}\" в состоянии \"{1}\".",
                    method, GetType()));
            }

            /// <summary>Построенный запрос, после обработки выражения.</summary>
            public virtual ISimpleQuery BuiltQuery
            {
                get
                {
                    throw CreateException(MethodBase.GetCurrentMethod());
                }
            }

            /// <summary>Обработка начала парсинга.</summary>
            public virtual BuilderState HandleStart()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка завершения парсинга.</summary>
            public virtual BuilderState HandleEnd()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Получение перечислителя.</summary>
            /// <param name="itemType">Имя элемента.</param>
            public virtual BuilderState HandleGettingEnumerator(Type itemType)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка выборки.</summary>
            /// <param name="selectExpression">Выражение выборки.</param>
            public virtual BuilderState HandleSelect(LambdaExpression selectExpression)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка фильтрации.</summary>
            /// <param name="filterExpression">Выражение фильтрации.</param>
            public virtual void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка старта сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            public BuilderState HandleOrderBy(LambdaExpression sortKeyExpression)
            {
                return HandleOrderBy(sortKeyExpression, SortKind.Ascending);
            }

            /// <summary>Обработка старта сортировки, начиная с сортировки по убыванию..</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            public BuilderState HandleOrderByDescending(LambdaExpression sortKeyExpression)
            {
                return HandleOrderBy(sortKeyExpression, SortKind.Descending);
            }

            /// <summary>Обработка продолжения сортировки, по вторичным ключам.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            public BuilderState HandleThenBy(LambdaExpression sortKeyExpression)
            {
                return HandleThenBy(sortKeyExpression, SortKind.Ascending);
            }

            /// <summary>Обработка продолжения сортировки по убыванию, по вторичным ключам.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            public BuilderState HandleThenByDescending(LambdaExpression sortKeyExpression)
            {
                return HandleThenBy(sortKeyExpression, SortKind.Descending);
            }

            /// <summary>Получение всех записей.</summary>
            /// <param name="sourceName">Имя источника.</param>
            public virtual BuilderState HandleGettingRecords(string sourceName)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Получение всех типизированных записей.</summary>
            /// <param name="dataType">Тип запрашиваемых записей.</param>
            public virtual BuilderState HandleGettingTypedRecords(Type dataType)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка старта сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Порядок сортировки.</param>
            protected virtual BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>Обработка продолжения сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Порядок сортировки.</param>
            protected virtual BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }
        }

        /// <summary>Начальное состояние.</summary>
        private sealed class StartingState : BuilderState
        {
            public override BuilderState HandleStart()
            {
                return new StartedState();
            }
        }

        /// <summary>Состояние после старта.</summary>
        private sealed class StartedState : BuilderState
        {
            public override BuilderState HandleGettingEnumerator(Type itemType)
            {
                return new EnumeratorState(itemType);
            }
        }

        /// <summary>Завершенное состояние.</summary>
        private sealed class EndedState : BuilderState
        {
            public EndedState(ISimpleQuery builtQuery)
            {
                Contract.Requires<ArgumentNullException>(builtQuery != null);

                _builtQuery = builtQuery;
            }

            /// <summary>Построенный запрос, после обработки выражения.</summary>
            public override ISimpleQuery BuiltQuery
            {
                get { return _builtQuery; }
            }
            private readonly ISimpleQuery _builtQuery;
        }

        /// <summary>Состояние после получение записей из источника.</summary>
        private sealed class GettingRecordsState : BuilderState
        {
            private readonly ISimpleQuery _builtQuery;

            public GettingRecordsState(ISimpleQuery builtQuery)
            {
                _builtQuery = builtQuery;
            }

            /// <summary>Обработка завершения парсинга.</summary>
            public override BuilderState HandleEnd()
            {
                return new EndedState(_builtQuery);
            }
        }

        /// <summary>Базовый класс состояния с данными.</summary>
        private abstract class StateWithDataBase : BuilderState
        {
            protected StateWithDataBase(IStateData data)
            {
                _data = data;
            }
            
            /// <summary>
            /// Данные состояния.
            /// </summary>
            protected IStateData Data
            {
                get { return _data; }
            }
            private readonly IStateData _data;

            /// <summary>Обработка сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение выборки ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            private void HandleSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                Data.AddSort(sortKeyExpression, sortKind);
            }

            /// <summary>Обработка старта сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Порядок сортировки.</param>
            protected override BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                HandleSort(sortKeyExpression, sortKind);
                return new SortedState(Data);
            }

            /// <summary>Обработка продолжения сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Порядок сортировки.</param>
            protected override BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                HandleSort(sortKeyExpression, sortKind);
                return new SortingState(Data);
            }
        }

        /// <summary>
        /// Базовый класс промежуточных состояний.
        /// </summary>
        private abstract class IntermediateStateBase : StateWithDataBase
        {
             protected IntermediateStateBase(IStateData data)
                 : base(data)
             {}

            /// <summary>Получение всех записей.</summary>
            /// <param name="sourceName">Имя источника.</param>
            public sealed override BuilderState HandleGettingRecords(string sourceName)
             {
                 return new GettingRecordsState(
                     Data.CreateQuery(sourceName));
             }

            /// <summary>Обработка фильтрации.</summary>
            /// <param name="filterExpression">Выражение фильтрации.</param>
            public sealed override void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
             {
                 Data.FilterExpression = filterExpression;
             }
        }

        /// <summary>
        /// Состояние после получения перечислителя элементов данных.
        /// </summary>
        private sealed class EnumeratorState : IntermediateStateBase
        {
            private readonly StateDataWithoutSelection _stateDataWithoutSelection;
            
            public EnumeratorState(Type itemType) 
                : this(new StateDataWithoutSelection(itemType))
            {}

            private EnumeratorState(StateDataWithoutSelection stateDataWithoutSelection) 
                : base(stateDataWithoutSelection)
            {
                _stateDataWithoutSelection = stateDataWithoutSelection;
            }

            public override BuilderState HandleSelect(LambdaExpression selectExpression)
            {
                if (_stateDataWithoutSelection.ItemType != selectExpression.ReturnType)
                {
                    throw new InvalidOperationException(string.Format(
                        "Тип \"{0}\" результата выборки не приемлем. Ожидался тип \"{1}\".",
                        selectExpression.ReturnType, _stateDataWithoutSelection.ItemType));
                }

                // Для тривиального выражения ничего не делается.
                if (IsTrivial(selectExpression))
                    return this;

                return new SelectedState(
                    new SelectionStateData(_stateDataWithoutSelection, selectExpression));
            }

            private static bool IsTrivial(LambdaExpression lambda)
            {
                Contract.Assert(lambda.Parameters.Count == 1);

                var parameter = lambda.Parameters[0];

                return lambda.Body == parameter;
            }

            public override BuilderState HandleGettingTypedRecords(Type dataType)
            {
                if (_stateDataWithoutSelection.ItemType != dataType)
                {
                    throw new InvalidOperationException(string.Format(
                        "Тип \"{0}\" для получения записей неприемлем. Ожидался тип \"{1}\".",
                        dataType, _stateDataWithoutSelection.ItemType));
                }

                return new GettingRecordsState(CreateTupleQuery(dataType));
            }
        }

        /// <summary>Состояние после выборки данных.</summary>
        private sealed class SelectedState : IntermediateStateBase
        {
            public SelectedState(SelectionStateData data) : base(data)
            {}
        }

        /// <summary>
        /// Промежуточное состояние описания сортировки.
        /// </summary>
        private sealed class SortingState : StateWithDataBase
        {
            public SortingState(IStateData data)
                : base(data)
            {}
        }

        /// <summary>
        /// Состояние после описания сортировки.
        /// </summary>
        private sealed class SortedState : IntermediateStateBase
        {
            public SortedState(IStateData data) : base(data)
            {}

            protected override BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                // Другие сортировки игнорируются.
                return this;
            }

            protected override BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                // Другие сортировки игнорируются.
                return this;
            }
        }

        #endregion
    }
}