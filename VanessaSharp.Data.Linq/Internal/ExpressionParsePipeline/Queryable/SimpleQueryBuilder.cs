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
    /// � ������ ������� <see cref="SimpleQuery"/>.
    /// </remarks>
    internal sealed class SimpleQueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>������ �������� ���������.</summary>
        private BuilderState _currentState;

        /// <summary>�����������.</summary>
        public SimpleQueryBuilder()
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

        /// <summary>��������� �������.</summary>
        /// <param name="selectExpression">��������� �������.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            _currentState = _currentState.HandleSelect(selectExpression);
        }

        /// <summary>��������� ����������.</summary>
        /// <param name="filterExpression">��������� ����������.</param>
        public void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
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
        public ISimpleQuery BuiltQuery
        {
            get { return _currentState.BuiltQuery; }
        }

        /// <summary>�������� <see cref="TupleQuery{T}"/>.</summary>
        /// <param name="dataType">��� ������.</param>
        private static ISimpleQuery CreateTupleQuery(Type dataType)
        {
            var queryType = typeof(TupleQuery<>).MakeGenericType(dataType);
            return (ISimpleQuery)Activator.CreateInstance(queryType);
        }

        #region ������ ���������

        /// <summary>��������� ������ ���������.</summary>
        private interface IStateData
        {
            /// <summary>��������� ���������� �������.</summary>
            Expression<Func<OneSDataRecord, bool>> FilterExpression { set; }

            /// <summary>���������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind);

            /// <summary>�������� ������� �������.</summary>
            /// <param name="sourceName">��� ���������.</param>
            SimpleQuery CreateQuery(string sourceName);
        }

        /// <summary>������ ��������� ����� �� ����������� �������.</summary>
        private sealed class StateDataWithoutSelection : IStateData
        {
            public StateDataWithoutSelection(Type itemType)
            {
                _itemType = itemType;
            }
            
            /// <summary>��� ��������� � ������������������.</summary>
            public Type ItemType
            {
                get { return _itemType; }
            }
            private readonly Type _itemType;

            /// <summary>��������� ���������� �������.</summary>
            public Expression<Func<OneSDataRecord, bool>> FilterExpression { get; set; }

            /// <summary>���������� ����������.</summary>
            /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
            /// <param name="sortKind">����������� ����������.</param>
            public void AddSort(LambdaExpression sortKeyExpression, SortKind sortKind)
            {
                _sortExpressionStack.Push(new SortExpression(sortKeyExpression, sortKind));
            }

            /// <summary>�������� ������ ��������� ����������.</summary>
            public ReadOnlyCollection<SortExpression> CreateSortExpressionList()
            {
                return new ReadOnlyCollection<SortExpression>(
                    _sortExpressionStack.ToArray());
            }
            
            /// <summary>��������� ����������.</summary>
            private readonly Stack<SortExpression> _sortExpressionStack = new Stack<SortExpression>();

            /// <summary>�������� ������� �������.</summary>
            /// <param name="sourceName">��� ���������.</param>
            public SimpleQuery CreateQuery(string sourceName)
            {
                return new DataRecordsQuery(sourceName, FilterExpression, CreateSortExpressionList());
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
            }

            /// <summary>��������� ���������� �������.</summary>
            public Expression<Func<OneSDataRecord, bool>> FilterExpression
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
            public SimpleQuery CreateQuery(string sourceName)
            {
                return CreateDataTypeQuery(sourceName, _stateData.FilterExpression,
                                           _stateData.CreateSortExpressionList(), _selectExpression);
            }

            /// <summary>�������� ������� ��������� ��������� ���������� ����.</summary>
            /// <param name="source">��� ���������.</param>
            /// <param name="filterExpression">��������� ����������.</param>
            /// <param name="sortExpressionList">������ ��������� ����������.</param>
            /// <param name="selectExpression">��������� �������.</param>
            private static SimpleQuery CreateDataTypeQuery(
                string source, Expression<Func<OneSDataRecord, bool>> filterExpression,
                ReadOnlyCollection<SortExpression> sortExpressionList, LambdaExpression selectExpression)
            {
                var queryType = typeof(CustomDataTypeQuery<>).MakeGenericType(selectExpression.ReturnType);
                return (SimpleQuery)Activator.CreateInstance(queryType, source, filterExpression, sortExpressionList, selectExpression);
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
            public virtual ISimpleQuery BuiltQuery
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
            public virtual void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
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
                return new EnumeratorState(itemType);
            }
        }

        /// <summary>����������� ���������.</summary>
        private sealed class EndedState : BuilderState
        {
            public EndedState(ISimpleQuery builtQuery)
            {
                Contract.Requires<ArgumentNullException>(builtQuery != null);

                _builtQuery = builtQuery;
            }

            /// <summary>����������� ������, ����� ��������� ���������.</summary>
            public override ISimpleQuery BuiltQuery
            {
                get { return _builtQuery; }
            }
            private readonly ISimpleQuery _builtQuery;
        }

        /// <summary>��������� ����� ��������� ������� �� ���������.</summary>
        private sealed class GettingRecordsState : BuilderState
        {
            private readonly ISimpleQuery _builtQuery;

            public GettingRecordsState(ISimpleQuery builtQuery)
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

            /// <summary>��������� ����������.</summary>
            /// <param name="filterExpression">��������� ����������.</param>
            public sealed override void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
             {
                 Data.FilterExpression = filterExpression;
             }
        }

        /// <summary>
        /// ��������� ����� ��������� ������������� ��������� ������.
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
                        "��� \"{0}\" ���������� ������� �� ��������. �������� ��� \"{1}\".",
                        selectExpression.ReturnType, _stateDataWithoutSelection.ItemType));
                }

                // ��� ������������ ��������� ������ �� ��������.
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
                        "��� \"{0}\" ��� ��������� ������� ����������. �������� ��� \"{1}\".",
                        dataType, _stateDataWithoutSelection.ItemType));
                }

                return new GettingRecordsState(CreateTupleQuery(dataType));
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