using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        // TODO ����������� ���������
        /// <summary>��������� �����������-�����������.</summary>
        private enum HandlerState
        {
            Starting,
            Started,
            Enumerable,
            Sorting,
            Sorted,
            Selected,
            Records,
            Ended,
        }

        /// <summary>������� ���������.</summary>
        private HandlerState _currentState = HandlerState.Starting;

        /// <summary>��� ��������� � ������������������.</summary>
        private Type _itemType;

        /// <summary>�������� ������.</summary>
        private string _sourceName;

        /// <summary>��������� ���������� �������.</summary>
        private Expression<Func<OneSDataRecord, bool>> _filterExpression;

        /// <summary>��������� ����������.</summary>
        private readonly Stack<SortExpression> _sortExpressionStack = new Stack<SortExpression>();

        /// <summary>������� �������� �������.</summary>
        private Func<string, SimpleQuery> _queryFactory;

        /// <summary>�����������.</summary>
        public SimpleQueryBuilder()
        {
            _queryFactory = source => new DataRecordsQuery(
                source, _filterExpression, CreateSortExpressionList());
        }

        /// <summary>�������� ������ ��������� ����������.</summary>
        private ReadOnlyCollection<SortExpression> CreateSortExpressionList()
        {
            return new ReadOnlyCollection<SortExpression>(
                _sortExpressionStack.ToArray());
        }

        /// <summary>��������� ������ ��������.</summary>
        public void HandleStart()
        {
            if (_currentState != HandlerState.Starting)
               ThrowException(MethodBase.GetCurrentMethod());

            _currentState = HandlerState.Started;
        }

        /// <summary>��������� ���������� ��������.</summary>
        public void HandleEnd()
        {
            if (_currentState != HandlerState.Records)
                ThrowException(MethodBase.GetCurrentMethod());

            _builtQuery = _queryFactory(_sourceName);
            _currentState = HandlerState.Ended;
        }

        /// <summary>��������� �������������.</summary>
        /// <param name="itemType">��� ��������.</param>
        public void HandleGettingEnumerator(Type itemType)
        {
            if (_currentState != HandlerState.Started)
                ThrowException(MethodBase.GetCurrentMethod());

            _itemType = itemType;
            _currentState = HandlerState.Enumerable;
        }

        /// <summary>��������� �������.</summary>
        /// <param name="selectExpression">��������� �������.</param>
        public void HandleSelect(LambdaExpression selectExpression)
        {
            if (_currentState != HandlerState.Enumerable)
                ThrowException(MethodBase.GetCurrentMethod());

            if (_itemType != selectExpression.ReturnType)
            {
                throw new InvalidOperationException(string.Format(
                    "��� \"{0}\" ���������� ������� �� ��������. �������� ��� \"{1}\".",
                    selectExpression.ReturnType, _itemType));
            }

            _queryFactory = source => CreateDataTypeQuery(_sourceName, _filterExpression, CreateSortExpressionList(), selectExpression);
            _currentState = HandlerState.Selected;
        }

        /// <summary>��������� ����������.</summary>
        /// <param name="filterExpression">��������� ����������.</param>
        public void HandleFilter(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            if (_currentState == HandlerState.Enumerable
                ||
                _currentState == HandlerState.Selected
                ||
                _currentState == HandlerState.Sorted)
            {
                _filterExpression = filterExpression;
                return;
            }
            
            ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>��������� ������ ����������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleOrderBy(LambdaExpression sortKeyExpression)
        {
            if (!HandleOrderBy(sortKeyExpression, SortKind.Ascending))
                ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>��������� ������ ����������, ������� � ���������� �� ��������..</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleOrderByDescending(LambdaExpression sortKeyExpression)
        {
            if (!HandleOrderBy(sortKeyExpression, SortKind.Descending))
                ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>��������� ����������� ����������, �� ��������� ������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleThenBy(LambdaExpression sortKeyExpression)
        {
            if (!HandleThenBy(sortKeyExpression, SortKind.Ascending))
                ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>��������� ����������� ���������� �� ��������, �� ��������� ������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        public void HandleThenByDescending(LambdaExpression sortKeyExpression)
        {
            if (!HandleThenBy(sortKeyExpression, SortKind.Descending))
                ThrowException(MethodBase.GetCurrentMethod());
        }

        /// <summary>��������� �������� ����������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        /// <param name="sortKind">������� ����������.</param>
        /// <param name="newState">����� ��������� ����������� ����� ������ ������</param>
        /// <returns>
        /// ���������� <c>true</c>, ���� ���������� ���� ���������, � ���� ������ ������������ <c>false</c>.
        /// </returns>
        private bool HandleSort(LambdaExpression sortKeyExpression, SortKind sortKind, HandlerState newState)
        {
            if (_currentState == HandlerState.Sorted)
                return true; // ���������� ������������

            if (_currentState == HandlerState.Enumerable
                ||
                _currentState == HandlerState.Selected
                ||
                _currentState == HandlerState.Sorting)
            {
                _sortExpressionStack.Push(new SortExpression(sortKeyExpression, sortKind));
                _currentState = newState;

                return true;
            }

            return false;
        }

        /// <summary>��������� ������ ����������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        /// <param name="sortKind">������� ����������.</param>
        private bool HandleOrderBy(LambdaExpression sortKeyExpression, SortKind sortKind)
        {
            return HandleSort(sortKeyExpression, sortKind, HandlerState.Sorted);
        }

        /// <summary>��������� ����������� ����������.</summary>
        /// <param name="sortKeyExpression">��������� ��������� ����� ����������.</param>
        /// <param name="sortKind">������� ����������.</param>
        private bool HandleThenBy(LambdaExpression sortKeyExpression, SortKind sortKind)
        {
            return HandleSort(sortKeyExpression, sortKind, HandlerState.Sorting);
        }

        /// <summary>�������� ������� ��������� ��������� ���������� ����.</summary>
        /// <param name="source">��� ���������.</param>
        /// <param name="filterExpression">��������� ����������.</param>
        /// <param name="sortExpressionList">������ ��������� ����������.</param>
        /// <param name="selectExpression">��������� �������.</param>
        private static SimpleQuery CreateDataTypeQuery(string source, Expression<Func<OneSDataRecord, bool>> filterExpression, ReadOnlyCollection<SortExpression> sortExpressionList, LambdaExpression selectExpression)
        {
            var queryType = typeof(CustomDataTypeQuery<>).MakeGenericType(selectExpression.ReturnType);
            return (SimpleQuery)Activator.CreateInstance(queryType, source, filterExpression, sortExpressionList, selectExpression);
        }

        /// <summary>��������� ���� �������.</summary>
        /// <param name="sourceName">��� ���������.</param>
        public void HandleGettingRecords(string sourceName)
        {
            if (_currentState != HandlerState.Enumerable && _currentState != HandlerState.Selected && _currentState != HandlerState.Sorted)
                ThrowException(MethodBase.GetCurrentMethod());

            _sourceName = sourceName;
            _currentState = HandlerState.Records;
        }

        /// <summary>����������� ������, ����� ��������� ���������.</summary>
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

        /// <summary>�������� ���������� ������������ ��������.</summary>
        /// <param name="method">����� ���������� ��������.</param>
        private void ThrowException(MethodBase method)
        {
            throw new InvalidOperationException(string.Format(
                "����������� �������� ����� \"{0}\" � ��������� \"{1}\".",
                method, _currentState));
        }
    }
}