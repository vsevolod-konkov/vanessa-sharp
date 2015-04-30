using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Queryable
{
    /// <summary>����������� ���������� <see cref="IQueryableExpressionHandler"/></summary>
    /// <remarks>
    /// ��������������� ��������� ������������ <see cref="Queryable"/>
    /// � ������ ������� <see cref="IQuery"/>.
    /// </remarks>
    internal sealed class QueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>������ �������� ���������.</summary>
        private BuilderState _currentState;

        /// <summary>�����������.</summary>
        public QueryBuilder()
        {
            _currentState = new StartingState();
        }

        /// <summary>��������� ������ ��������.</summary>
        public void HandleStart()
        {
            _currentState = _currentState.HandleStart();
        }

        /// <summary>��������� ���������� ��������.</summary>
        public void HandleEnd()
        {
            _currentState = _currentState.HandleEnd();
        }

        /// <summary>��������� �������������.</summary>
        /// <param name="itemType">��� ��������.</param>
        public void HandleGettingEnumerator(Type itemType)
        {
            _currentState = _currentState.HandleGettingEnumerator(itemType);
        }

        /// <summary>��������� ��������� ������.</summary>
        /// <param name="outputItemType">��� ��������� �������� ������������������.</param>
        /// <param name="function">������� ���������.</param>
        /// <param name="scalarType">��� ����������.</param>
        public void HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
        {
            _currentState = _currentState.HandleAggregate(outputItemType, function, scalarType);
        }

        /// <summary>��������� ������� ��������� �������.</summary>
        public void HandleDistinct()
        {
            _currentState = _currentState.HandleDistinct();
        }

        /// <summary>��������� �������.</summary>
        /// <param name="selectExpression">��������� �������.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            _currentState = _currentState.HandleSelect(selectExpression);
        }

        /// <summary>��������� ����������.</summary>
        /// <param name="filterExpression">��������� ����������.</param>
        public void HandleFilter(LambdaExpression filterExpression)
        {
            _currentState.HandleFilter(filterExpression);
        }

        /// <summary>��������� ������ ����������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleOrderBy(sortKeyExpression);
        }

        /// <summary>��������� ������ ����������, ������� � ���������� �� ��������..</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleOrderByDescending(sortKeyExpression);
        }

        /// <summary>��������� ����������� ����������, �� ��������� ������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleThenBy(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleThenBy(sortKeyExpression);
        }

        /// <summary>��������� ����������� ���������� �� ��������, �� ��������� ������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            _currentState = _currentState.HandleThenByDescending(sortKeyExpression);
        }

        /// <summary>��������� ���� �������.</summary>
        /// <param name="sourceName">��� ���������.</param>
        public void HandleGettingRecords(string sourceName)
        {
            _currentState = _currentState.HandleGettingRecords(sourceName);
        }

        /// <summary>��������� ���� �������������� �������.</summary>
        /// <param name="dataType">��� ������������� �������.</param>
        public void HandleGettingTypedRecords(Type dataType)
        {
            _currentState = _currentState.HandleGettingTypedRecords(dataType);
        }

        /// <summary>����������� ������, ����� ��������� ���������.</summary>
        public IQuery BuiltQuery
        {
            get { return _currentState.BuiltQuery; }
        }

        #region ������ ���������

        /// <summary>��������� ������ ���������.</summary>
        private interface IStateData
        {
            /// <summary>��������� ���������� �������.</summary>
            LambdaExpression FilterExpression { set; }

            /// <summary>���������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind);

            /// <summary>�������� ������� ������� ������� <see cref="OneSDataRecord"/>.</summary>
            /// <param name="sourceName">��� ���������.</param>
            IQuery CreateQuery(string sourceName);

            /// <summary>�������� ������� ������� �������������� �������.</summary>
            /// <param name="itemType">��� ��������� ������� ������������������.</param>
            IQuery CreateQuery(Type itemType);
        }

        /// <summary>������ ��������� ����� �� ����������� �������.</summary>
        private class StateDataWithoutSelection : IStateData
        {
            public StateDataWithoutSelection(Type outputItemType)
            {
                _outputItemType = outputItemType;
                InputItemType = outputItemType;
            }
            
            /// <summary>��� ��������� � �������� ������������������.</summary>
            public Type OutputItemType
            {
                get { return _outputItemType; }
            }
            private readonly Type _outputItemType;

            /// <summary>��� ��������� �� ������� ������������������.</summary>
            public Type InputItemType { get; set; }

            /// <summary>������� ���������.</summary>
            public bool IsDistinct { get; set; }

            /// <summary>��������� ���������� �������.</summary>
            public LambdaExpression FilterExpression
            {
                protected get { return _filterExpression; }
                
                set
                {
                    if (_filterExpression != null)
                    {
                        throw new InvalidOperationException(string.Format(
                            "������� ���������� ��������� ���������� \"{0}\" ������� ����������, ��� ��� ��������� ���������� ��� ����������� \"{1}\".",
                            value, _filterExpression));
                    }
                    
                    if (value != null)
                        CheckFilterExpression(value);
                    
                    _filterExpression = value;
                }
            }
            private LambdaExpression _filterExpression;

            /// <summary>���������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                CheckLambdaExpression(sortKeyExpression);
                _sortExpressionStack.Push(new SortExpression(sortKeyExpression, sortKind));
            }

            /// <summary>�������� ������ ��������� ����������.</summary>
            protected ReadOnlyCollection<SortExpression> CreateSortExpressionList()
            {
                return new ReadOnlyCollection<SortExpression>(
                    _sortExpressionStack.ToArray());
            }
            
            /// <summary>���� ��������� ����������.</summary>
            /// <remarks>��� ��� ��������� ���������� �������������� � �������� �����������, � �����.</remarks>
            private readonly Stack<SortExpression> _sortExpressionStack = new Stack<SortExpression>();

            /// <summary>
            /// �������� ��������� ������� ��� �������� �������.
            /// </summary>
            /// <param name="selectExpression">��������� �������.</param>
            private void CheckSelectExpressionForQuery(LambdaExpression selectExpression)
            {
                if (InputItemType != OutputItemType && selectExpression == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "���� ��� ��������� �������, �� ���� ��������� ������� � �������� ������������������ ������ ���������."
                        + " ��� �� ���. ��� ��������� ������� ������������������ \"{0}\". ��� ��������� �������� ������������������\"{1}\".",
                        InputItemType, OutputItemType));
                }

                if (selectExpression != null)
                {
                    CheckLambdaExpression(selectExpression);

                    if (selectExpression.ReturnType != OutputItemType)
                    {
                        throw new InvalidOperationException(string.Format(
                        "��� ��������� �������� ������������������ ������ ��������� � ����� ���������� ��������� �������."
                        + " ��� �� ���. ��� ��������� �������� ������������������ \"{0}\". ��� ���������� ��������� ������� \"{1}\" ����� \"{2}\".",
                        OutputItemType, selectExpression, selectExpression.ReturnType));
                    }
                }
            }

            /// <summary>�������� ������� ������� ������� ���� <see cref="OneSDataRecord"/>.</summary>
            /// <param name="sourceName">��� ��������� ������.</param>
            public IQuery CreateQuery(string sourceName)
            {
                Contract.Assert(InputItemType == typeof(OneSDataRecord));

                return CreateQuery(sourceName, null);
            }

            /// <summary>
            /// �������� ������� ������� ������� ���� <see cref="OneSDataRecord"/>.
            /// </summary>
            /// <param name="sourceName">��� ��������� ������.</param>
            /// <param name="selectExpression">��������� �������.</param>
            public virtual IQuery CreateQuery(string sourceName, LambdaExpression selectExpression)
            {
                Contract.Assert(InputItemType == typeof(OneSDataRecord));

                CheckSelectExpressionForQuery(selectExpression);

                return (selectExpression == null)
                    ? QueryFactory.CreateQuery(sourceName, FilterExpression, CreateSortExpressionList(), IsDistinct)
                    : QueryFactory.CreateQuery(sourceName, selectExpression, FilterExpression, CreateSortExpressionList(), IsDistinct);
            }

            /// <summary>�������� ������� �������.</summary>
            public IQuery CreateQuery(Type itemType)
            {
                return CreateQuery(itemType, null);
            }

            /// <summary>
            /// �������� ������� �������������� ������� � ��������� �������� ���������.
            /// </summary>
            /// <param name="itemType">��� ��������� ������� ������������������.</param>
            /// <param name="selectExpression">��������� ������� ���������.</param>
            public virtual IQuery CreateQuery(Type itemType, LambdaExpression selectExpression)
            {
                Contract.Assert(itemType != typeof(OneSDataRecord));
                
                if (InputItemType != itemType)
                {
                    throw new InvalidOperationException(string.Format(
                        "��� \"{0}\" ��� ��������� ������� ����������. �������� ��� \"{1}\".",
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
                        "��������� ��� \"{0}\" ��������� \"{1}\". ��������� ��������� ���� \"{2}\", � ������� ������ �����-���������� ����� ��� ��������� ������� ������������������ \"{3}\".",
                        lambda, lambda.Type,
                        typeof(Func<,>), InputItemType));
                }
            }

            private void CheckFilterExpression(LambdaExpression lambda)
            {
                if (!VerifyFilterType(lambda.Type))
                {
                    throw new InvalidOperationException(String.Format(
                        "��������� ��� \"{0}\" ��������� ���������� \"{1}\". ��������� ��������� ���� \"{2}\", �� ���������� ������-����������� \"{3}\", \"{4}\".",
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

        /// <summary>������ ��������� � ���������� �������.</summary>
        private sealed class SelectionStateData : IStateData
        {
            /// <summary>������ ��� �������.</summary>
            private readonly StateDataWithoutSelection _stateData;

            /// <summary>��������� ������� ������.</summary>
            private readonly LambdaExpression _selectExpression;

            /// <summary>�����������.</summary>
            /// <param name="stateData">������ ��� �������.</param>
            /// <param name="selectExpression">��������� ������� ������.</param>
            public SelectionStateData(StateDataWithoutSelection stateData, LambdaExpression selectExpression)
            {
                _stateData = stateData;
                _selectExpression = selectExpression;

                _stateData.InputItemType = GetInputType(_selectExpression);
            }

            /// <summary>��������� ���������� �������.</summary>
            public LambdaExpression FilterExpression
            {
                set { _stateData.FilterExpression = value; }
            }

            /// <summary>���������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                _stateData.AddSort(sortKeyExpression, sortKind);
            }

            /// <summary>�������� ������� �������.</summary>
            /// <param name="sourceName">��� ���������.</param>
            public IQuery CreateQuery(string sourceName)
            {
                Contract.Assert(_stateData.InputItemType == typeof(OneSDataRecord));

                return _stateData.CreateQuery(sourceName, _selectExpression);
            }

            /// <summary>�������� ������� �������.</summary>
            public IQuery CreateQuery(Type itemType)
            {
                Contract.Assert(_stateData.InputItemType != typeof(OneSDataRecord));
                
                return _stateData.CreateQuery(itemType, _selectExpression);
            }

            /// <summary>��������� ���� ��������� ������� ������������������.</summary>
            /// <param name="selectExpression">��������� ������� ���������.</param>
            private static Type GetInputType(LambdaExpression selectExpression)
            {
                var lambdaType = selectExpression.Type;
                var isValid = lambdaType.IsGenericType && lambdaType.GetGenericTypeDefinition() == typeof(Func<,>);
                if (!isValid)
                {
                    throw new InvalidOperationException(string.Format(
                        "�������� ��� \"{0}\" ��������� ������� \"{1}\". ��� ��������� ������� �������� ��� \"{2}\".",
                        lambdaType, selectExpression, typeof(Func<,>)));
                }

                return lambdaType.GetGenericArguments()[0];
            }
        }

        /// <summary>������� ����� ���������.</summary>
        private abstract class BuilderState
        {
            /// <summary>�������� ���������� ������������ ��������.</summary>
            /// <param name="method">����� ���������� ��������.</param>
            private Exception CreateException(MethodBase method)
            {
                throw new InvalidOperationException(string.Format(
                    "����������� �������� ����� \"{0}\" � ��������� \"{1}\".",
                    method, GetType()));
            }

            /// <summary>����������� ������, ����� ��������� ���������.</summary>
            public virtual IQuery BuiltQuery
            {
                get
                {
                    throw CreateException(MethodBase.GetCurrentMethod());
                }
            }

            /// <summary>��������� ������ ��������.</summary>
            public virtual BuilderState HandleStart()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ���������� ��������.</summary>
            public virtual BuilderState HandleEnd()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� �������������.</summary>
            /// <param name="itemType">��� ��������.</param>
            public virtual BuilderState HandleGettingEnumerator(Type itemType)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� �������.</summary>
            /// <param name="selectExpression">��������� �������.</param>
            public virtual BuilderState HandleSelect(LambdaExpression selectExpression)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ����������.</summary>
            /// <param name="filterExpression">��������� ����������.</param>
            public virtual void HandleFilter(LambdaExpression filterExpression)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ������ ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            public BuilderState HandleOrderBy(LambdaExpression sortKeyExpression)
            {
                return HandleOrderBy(sortKeyExpression, SortKind.Ascending);
            }

            /// <summary>��������� ������ ����������, ������� � ���������� �� ��������..</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            public BuilderState HandleOrderByDescending(LambdaExpression sortKeyExpression)
            {
                return HandleOrderBy(sortKeyExpression, SortKind.Descending);
            }

            /// <summary>��������� ����������� ����������, �� ��������� ������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            public BuilderState HandleThenBy(LambdaExpression sortKeyExpression)
            {
                return HandleThenBy(sortKeyExpression, SortKind.Ascending);
            }

            /// <summary>��������� ����������� ���������� �� ��������, �� ��������� ������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            public BuilderState HandleThenByDescending(LambdaExpression sortKeyExpression)
            {
                return HandleThenBy(sortKeyExpression, SortKind.Descending);
            }

            /// <summary>��������� ���� �������.</summary>
            /// <param name="sourceName">��� ���������.</param>
            public virtual BuilderState HandleGettingRecords(string sourceName)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ���� �������������� �������.</summary>
            /// <param name="dataType">��� ������������� �������.</param>
            public virtual BuilderState HandleGettingTypedRecords(Type dataType)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ������ ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">������� ����������.</param>
            protected virtual BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ����������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">������� ����������.</param>
            protected virtual BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>��������� ���������� ��������� ��������� �������.</summary>
            public virtual BuilderState HandleDistinct()
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }

            /// <summary>
            /// ��������� ������ ���������� �������.
            /// </summary>
            /// <param name="outputItemType">��� ��������� �������� ������������������.</param>
            /// <param name="function">���������� �������.</param>
            /// <param name="scalarType">��� ���������� �������� ��� ��������� �������.</param>
            public virtual BuilderState HandleAggregate(Type outputItemType, AggregateFunction function, Type scalarType)
            {
                throw CreateException(MethodBase.GetCurrentMethod());
            }
        }

        /// <summary>��������� ���������.</summary>
        private sealed class StartingState : BuilderState
        {
            public override BuilderState HandleStart()
            {
                return new StartedState();
            }
        }

        /// <summary>��������� ����� ������.</summary>
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

        /// <summary>����������� ���������.</summary>
        private sealed class EndedState : BuilderState
        {
            public EndedState(IQuery builtQuery)
            {
                Contract.Requires<ArgumentNullException>(builtQuery != null);

                _builtQuery = builtQuery;
            }

            /// <summary>����������� ������, ����� ��������� ���������.</summary>
            public override IQuery BuiltQuery
            {
                get { return _builtQuery; }
            }
            private readonly IQuery _builtQuery;
        }

        /// <summary>��������� ����� ��������� ������� �� ���������.</summary>
        private sealed class GettingRecordsState : BuilderState
        {
            private readonly IQuery _builtQuery;

            public GettingRecordsState(IQuery builtQuery)
            {
                _builtQuery = builtQuery;
            }

            /// <summary>��������� ���������� ��������.</summary>
            public override BuilderState HandleEnd()
            {
                return new EndedState(_builtQuery);
            }
        }

        /// <summary>������� ����� ��������� � �������.</summary>
        private abstract class StateWithDataBase : BuilderState
        {
            protected StateWithDataBase(IStateData data)
            {
                _data = data;
            }
            
            /// <summary>
            /// ������ ���������.
            /// </summary>
            protected IStateData Data
            {
                get { return _data; }
            }
            private readonly IStateData _data;

            /// <summary>��������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            private void HandleSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                Data.AddSort(sortKeyExpression, sortKind);
            }

            /// <summary>��������� ������ ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">������� ����������.</param>
            protected override BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                HandleSort(sortKeyExpression, sortKind);
                return new SortedState(Data);
            }

            /// <summary>��������� ����������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">������� ����������.</param>
            protected override BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                HandleSort(sortKeyExpression, sortKind);
                return new SortingState(Data);
            }
        }

        /// <summary>
        /// ������� ����� ������������� ���������.
        /// </summary>
        private abstract class IntermediateStateBase : StateWithDataBase
        {
             protected IntermediateStateBase(IStateData data)
                 : base(data)
             {}

            /// <summary>��������� ���� �������.</summary>
            /// <param name="sourceName">��� ���������.</param>
            public sealed override BuilderState HandleGettingRecords(string sourceName)
            {
                 return new GettingRecordsState(
                     Data.CreateQuery(sourceName));
            }

            /// <summary>��������� ���� �������������� �������.</summary>
            /// <param name="dataType">��� ������������� �������.</param>
            public override BuilderState HandleGettingTypedRecords(Type dataType)
            {
                return new GettingRecordsState(Data.CreateQuery(dataType));
            }

            /// <summary>��������� ����������.</summary>
            /// <param name="filterExpression">��������� ����������.</param>
            public sealed override void HandleFilter(LambdaExpression filterExpression)
            {
                 Data.FilterExpression = filterExpression;
            }
        }

        /// <summary>
        /// ��������� ����� ��������� ������������� ��������� ������.
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
                        "��� \"{0}\" ���������� ������� �� ��������. �������� ��� \"{1}\".",
                        selectExpression.ReturnType, _stateDataWithoutSelection.OutputItemType));
                }

                // ��� ������������ ��������� ������ �� ��������.
                if (IsTrivial(selectExpression))
                    return this;

                return new SelectedState(
                    new SelectionStateData(_stateDataWithoutSelection, selectExpression));
            }

            /// <summary>����������� ����������� �������.</summary>
            /// <param name="lambda">����������� ���������.</param>
            private static bool IsTrivial(LambdaExpression lambda)
            {
                Contract.Assert(lambda.Parameters.Count == 1);

                var parameter = lambda.Parameters[0];

                return lambda.Body == parameter;
            }

            /// <summary>��������� ���������� ��������� ��������� �������.</summary>
            public override BuilderState HandleDistinct()
            {
                _stateDataWithoutSelection.IsDistinct = true;
                return this;
            }
        }

        /// <summary>��������� ����� ������� ������.</summary>
        private sealed class SelectedState : IntermediateStateBase
        {
            public SelectedState(SelectionStateData data) : base(data)
            {}
        }

        /// <summary>
        /// ������������� ��������� �������� ����������.
        /// </summary>
        private sealed class SortingState : StateWithDataBase
        {
            public SortingState(IStateData data)
                : base(data)
            {}
        }

        /// <summary>
        /// ��������� ����� �������� ����������.
        /// </summary>
        private sealed class SortedState : IntermediateStateBase
        {
            public SortedState(IStateData data) : base(data)
            {}

            protected override BuilderState HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                // ������ ���������� ������������.
                return this;
            }

            protected override BuilderState HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                // ������ ���������� ������������.
                return this;
            }
        }

        #endregion
    }
}