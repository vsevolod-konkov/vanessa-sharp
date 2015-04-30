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
    /// в объект запроса <see cref="IQuery"/>.
    /// </remarks>
    internal sealed class QueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>Объект текущего состояния.</summary>
        private BuilderState _currentState;

        /// <summary>Конструктор.</summary>
        public QueryBuilder()
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

        /// <summary>Обработка агрегации данных.</summary>
        /// <param name="outputItemType">Тип элементов выходной последовательности.</param>
        /// <param name="function">Функция агрегации.</param>
        /// <param name="scalarType">Тип результата.</param>
        public void HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
        {
            _currentState = _currentState.HandleAggregate(outputItemType, function, scalarType);
        }

        /// <summary>Обработка выборки различных записей.</summary>
        public void HandleDistinct()
        {
            _currentState = _currentState.HandleDistinct();
        }

        /// <summary>Обработка выборки.</summary>
        /// <param name="selectExpression">Выражение выборки.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            _currentState = _currentState.HandleSelect(selectExpression);
        }

        /// <summary>Обработка фильтрации.</summary>
        /// <param name="filterExpression">Выражение фильтрации.</param>
        public void HandleFilter(LambdaExpression filterExpression)
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
        public IQuery BuiltQuery
        {
            get { return _currentState.BuiltQuery; }
        }

        #region Классы состояний

        /// <summary>Интерфейс данных состояния.</summary>
        private interface IStateData
        {
            /// <summary>Выражение фильтрации записей.</summary>
            LambdaExpression FilterExpression { set; }

            /// <summary>Добавление сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind);

            /// <summary>Создание объекта запроса записей <see cref="OneSDataRecord"/>.</summary>
            /// <param name="sourceName">Имя источника.</param>
            IQuery CreateQuery(string sourceName);

            /// <summary>Создание объекта запроса типизированных записей.</summary>
            /// <param name="itemType">Тип элементов входной последовательности.</param>
            IQuery CreateQuery(Type itemType);
        }

        /// <summary>Данные состояний когда не происходило выборки.</summary>
        private class StateDataWithoutSelection : IStateData
        {
            public StateDataWithoutSelection(Type outputItemType)
            {
                _outputItemType = outputItemType;
                InputItemType = outputItemType;
            }
            
            /// <summary>Тип элементов в выходной последовательности.</summary>
            public Type OutputItemType
            {
                get { return _outputItemType; }
            }
            private readonly Type _outputItemType;

            /// <summary>Тип элементов во входной последовательности.</summary>
            public Type InputItemType { get; set; }

            /// <summary>Выборка различных.</summary>
            public bool IsDistinct { get; set; }

            /// <summary>Выражение фильтрации записей.</summary>
            public LambdaExpression FilterExpression
            {
                protected get { return _filterExpression; }
                
                set
                {
                    if (_filterExpression != null)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Попытка установить выражение фильтрации \"{0}\" вызвало исключение, так как выражение фильтрации уже установлено \"{1}\".",
                            value, _filterExpression));
                    }
                    
                    if (value != null)
                        CheckFilterExpression(value);
                    
                    _filterExpression = value;
                }
            }
            private LambdaExpression _filterExpression;

            /// <summary>Добавление сортировки.</summary>
            /// <param name="sortKeyExpression">Выражение получения ключа сортировки.</param>
            /// <param name="sortKind">Направление сортировки.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                CheckLambdaExpression(sortKeyExpression);
                _sortExpressionStack.Push(new SortExpression(sortKeyExpression, sortKind));
            }

            /// <summary>Создание списка выражений сортировки.</summary>
            protected ReadOnlyCollection<SortExpression> CreateSortExpressionList()
            {
                return new ReadOnlyCollection<SortExpression>(
                    _sortExpressionStack.ToArray());
            }
            
            /// <summary>Стек выражений сортировки.</summary>
            /// <remarks>Так как выражения сортировки обрабатываются в обратном направлении, с конца.</remarks>
            private readonly Stack<SortExpression> _sortExpressionStack = new Stack<SortExpression>();

            /// <summary>
            /// Проверка выражения выборки для создания запроса.
            /// </summary>
            /// <param name="selectExpression">Выражение выборки.</param>
            private void CheckSelectExpressionForQuery(LambdaExpression selectExpression)
            {
                if (InputItemType != OutputItemType && selectExpression == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Если нет выражения выборки, то типы элементов входной и выходной последовательности должны совпадать."
                        + " Это не так. Тип элементов входной последовательности \"{0}\". Тип элементов выходной последовательности\"{1}\".",
                        InputItemType, OutputItemType));
                }

                if (selectExpression != null)
                {
                    CheckLambdaExpression(selectExpression);

                    if (selectExpression.ReturnType != OutputItemType)
                    {
                        throw new InvalidOperationException(string.Format(
                        "Тип элементов выходной последовательности должен совпадать с типом результата выражения выборки."
                        + " Это не так. Тип элементов выходной последовательности \"{0}\". Тип результата выражения выборки \"{1}\" равен \"{2}\".",
                        OutputItemType, selectExpression, selectExpression.ReturnType));
                    }
                }
            }

            /// <summary>Создание объекта запроса записей типа <see cref="OneSDataRecord"/>.</summary>
            /// <param name="sourceName">Имя источника данных.</param>
            public IQuery CreateQuery(string sourceName)
            {
                Contract.Assert(InputItemType == typeof(OneSDataRecord));

                return CreateQuery(sourceName, null);
            }

            /// <summary>
            /// Создание объекта запроса записей типа <see cref="OneSDataRecord"/>.
            /// </summary>
            /// <param name="sourceName">Имя источника данных.</param>
            /// <param name="selectExpression">Выражение выборки.</param>
            public virtual IQuery CreateQuery(string sourceName, LambdaExpression selectExpression)
            {
                Contract.Assert(InputItemType == typeof(OneSDataRecord));

                CheckSelectExpressionForQuery(selectExpression);

                return (selectExpression == null)
                    ? QueryFactory.CreateQuery(sourceName, FilterExpression, CreateSortExpressionList(), IsDistinct)
                    : QueryFactory.CreateQuery(sourceName, selectExpression, FilterExpression, CreateSortExpressionList(), IsDistinct);
            }

            /// <summary>Создание объекта запроса.</summary>
            public IQuery CreateQuery(Type itemType)
            {
                return CreateQuery(itemType, null);
            }

            /// <summary>
            /// Создание запроса типизированных записей с указанием проекции элементов.
            /// </summary>
            /// <param name="itemType">Тип элементов входной последовательности.</param>
            /// <param name="selectExpression">Выражение выборки элементов.</param>
            public virtual IQuery CreateQuery(Type itemType, LambdaExpression selectExpression)
            {
                Contract.Assert(itemType != typeof(OneSDataRecord));
                
                if (InputItemType != itemType)
                {
                    throw new InvalidOperationException(string.Format(
                        "Тип \"{0}\" для получения записей неприемлем. Ожидался тип \"{1}\".",
                        itemType, InputItemType));
                }

                CheckSelectExpressionForQuery(selectExpression);

                return (selectExpression == null)
                           ? QueryFactory.CreateQuery(InputItemType, FilterExpression, CreateSortExpressionList(), IsDistinct)
                           : QueryFactory.CreateQuery(selectExpression, FilterExpression, CreateSortExpressionList(), IsDistinct);
            }

            private void CheckLambdaExpression(LambdaExpression lambda)
            {
                if (!VerifyLambdaType(lambda.Type))
                {
                    throw new InvalidOperationException(String.Format(
                        "Ошибочный тип \"{0}\" выражения \"{1}\". Ожидалось выражение типа \"{2}\", в котором первым типом-параметром будет тип элементов входной последовательности \"{3}\".",
                        lambda, lambda.Type,
                        typeof(Func<,>), InputItemType));
                }
            }

            private void CheckFilterExpression(LambdaExpression lambda)
            {
                if (!VerifyFilterType(lambda.Type))
                {
                    throw new InvalidOperationException(String.Format(
                        "Ошибочный тип \"{0}\" выражения фильтрации \"{1}\". Ожидалось выражение типа \"{2}\", со следующими типами-параметрами \"{3}\", \"{4}\".",
                        lambda, lambda.Type,
                        typeof(Func<,>), InputItemType, typeof(bool)));
                }
            }

            private bool VerifyLambdaType(Type lambdaType)
            {
                return lambdaType.IsGenericType 
                    && lambdaType.GetGenericTypeDefinition() == typeof(Func<,>)
                    && lambdaType.GetGenericArguments()[0] == InputItemType;
            }

            private bool VerifyFilterType(Type lambdaType)
            {
                return VerifyLambdaType(lambdaType)
                       && lambdaType.GetGenericArguments()[1] == typeof(bool);
            }
        }

        private sealed class AggregateStateData : StateDataWithoutSelection
        {
            private readonly AggregateFunction _aggregateFunction;
            private readonly Type _scalarType;

            public AggregateStateData(Type outputItemType, AggregateFunction aggregateFunction, Type scalarType)
                : base(outputItemType)
            {
                Contract.Requires<ArgumentNullException>(scalarType != null);

                _aggregateFunction = aggregateFunction;
                _scalarType = scalarType;
            }

            public override IQuery CreateQuery(string sourceName, LambdaExpression selectExpression)
            {
                if (selectExpression == null)
                {
                    Contract.Assert(_aggregateFunction == AggregateFunction.Count);

                    return QueryFactory.CreateCountQuery(
                        sourceName, FilterExpression, CreateSortExpressionList(), _scalarType);
                }
                
                return QueryFactory.CreateScalarQuery(sourceName, selectExpression, FilterExpression, CreateSortExpressionList(),
                                                      IsDistinct, _aggregateFunction, _scalarType);
            }

            public override IQuery CreateQuery(Type itemType, LambdaExpression selectExpression)
            {
                if (selectExpression == null)
                {
                    Contract.Assert(_aggregateFunction == AggregateFunction.Count);

                    return QueryFactory.CreateCountQuery(
                        itemType, FilterExpression, CreateSortExpressionList(), _scalarType);
                }
                
                return QueryFactory.CreateScalarQuery(selectExpression, FilterExpression, CreateSortExpressionList(),
                                                      IsDistinct, _aggregateFunction, _scalarType);
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

                _stateData.InputItemType = GetInputType(_selectExpression);
            }

            /// <summary>Выражение фильтрации записей.</summary>
            public LambdaExpression FilterExpression
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
            public IQuery CreateQuery(string sourceName)
            {
                Contract.Assert(_stateData.InputItemType == typeof(OneSDataRecord));

                return _stateData.CreateQuery(sourceName, _selectExpression);
            }

            /// <summary>Создание объекта запроса.</summary>
            public IQuery CreateQuery(Type itemType)
            {
                Contract.Assert(_stateData.InputItemType != typeof(OneSDataRecord));
                
                return _stateData.CreateQuery(itemType, _selectExpression);
            }

            /// <summary>Получение типа элементов входной последовательности.</summary>
            /// <param name="selectExpression">Выражение выборки элементов.</param>
            private static Type GetInputType(LambdaExpression selectExpression)
            {
                var lambdaType = selectExpression.Type;
                var isValid = lambdaType.IsGenericType && lambdaType.GetGenericTypeDefinition() == typeof(Func<,>);
                if (!isValid)
                {
                    throw new InvalidOperationException(string.Format(
                        "Неверный тип \"{0}\" выражения выборки \"{1}\". Для выражения выборки ожидался тип \"{2}\".",
                        lambdaType, selectExpression, typeof(Func<,>)));
                }

                return lambdaType.GetGenericArguments()[0];
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
            public virtual IQuery BuiltQuery
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
            public virtual void HandleFilter(LambdaExpression filterExpression)
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

            /// <summary>Обработка маркировки получения различных записей.</summary>
            public virtual BuilderState HandleDistinct()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>
            /// Обработка вызова агрегатной функции.
            /// </summary>
            /// <param name="outputItemType">Тип элементов выходной последовательности.</param>
            /// <param name="function">Агрегатная функция.</param>
            /// <param name="scalarType">Тип скалярного значения как результат функции.</param>
            public virtual BuilderState HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
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
                return new ResultState(new StateDataWithoutSelection(itemType));
            }

            public override BuilderState HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
            {
                return new ResultState(
                    new AggregateStateData(outputItemType, function, scalarType));
            }
        }

        /// <summary>Завершенное состояние.</summary>
        private sealed class EndedState : BuilderState
        {
            public EndedState(IQuery builtQuery)
            {
                Contract.Requires<ArgumentNullException>(builtQuery != null);

                _builtQuery = builtQuery;
            }

            /// <summary>Построенный запрос, после обработки выражения.</summary>
            public override IQuery BuiltQuery
            {
                get { return _builtQuery; }
            }
            private readonly IQuery _builtQuery;
        }

        /// <summary>Состояние после получение записей из источника.</summary>
        private sealed class GettingRecordsState : BuilderState
        {
            private readonly IQuery _builtQuery;

            public GettingRecordsState(IQuery builtQuery)
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

            /// <summary>Получение всех типизированных записей.</summary>
            /// <param name="dataType">Тип запрашиваемых записей.</param>
            public override BuilderState HandleGettingTypedRecords(Type dataType)
            {
                return new GettingRecordsState(Data.CreateQuery(dataType));
            }

            /// <summary>Обработка фильтрации.</summary>
            /// <param name="filterExpression">Выражение фильтрации.</param>
            public sealed override void HandleFilter(LambdaExpression filterExpression)
            {
                 Data.FilterExpression = filterExpression;
            }
        }

        /// <summary>
        /// Состояние после получения перечислителя элементов данных.
        /// </summary>
        private sealed class ResultState : IntermediateStateBase
        {
            private readonly StateDataWithoutSelection _stateDataWithoutSelection;
            
            public ResultState(StateDataWithoutSelection stateDataWithoutSelection) 
                : base(stateDataWithoutSelection)
            {
                _stateDataWithoutSelection = stateDataWithoutSelection;
            }

            public override BuilderState HandleSelect(LambdaExpression selectExpression)
            {
                if (_stateDataWithoutSelection.OutputItemType != selectExpression.ReturnType)
                {
                    throw new InvalidOperationException(string.Format(
                        "Тип \"{0}\" результата выборки не приемлем. Ожидался тип \"{1}\".",
                        selectExpression.ReturnType, _stateDataWithoutSelection.OutputItemType));
                }

                // Для тривиального выражения ничего не делается.
                if (IsTrivial(selectExpression))
                    return this;

                return new SelectedState(
                    new SelectionStateData(_stateDataWithoutSelection, selectExpression));
            }

            /// <summary>Определение тривиальной выборки.</summary>
            /// <param name="lambda">Проверяемое выражение.</param>
            private static bool IsTrivial(LambdaExpression lambda)
            {
                Contract.Assert(lambda.Parameters.Count == 1);

                var parameter = lambda.Parameters[0];

                return lambda.Body == parameter;
            }

            /// <summary>Обработка маркировки получения различных записей.</summary>
            public override BuilderState HandleDistinct()
            {
                _stateDataWithoutSelection.IsDistinct = true;
                return this;
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