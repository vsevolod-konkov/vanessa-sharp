using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>����������� ���������� <see cref="IQueryableExpressionHandler"/></summary>
    /// <remarks>
    /// ��������������� ��������� ������������ <see cref="Queryable"/>
    /// � ������ ������� <see cref="SimpleQuery"/>.
    /// </remarks>
    internal sealed class SimpleQueryBuilder : IQueryableExpressionHandler
    {
        /// <summary>��������� �����������-�����������.</summary>
        private enum HandlerState
        {
            Starting,
            Started,
            Enumerable,
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

        /// <summary>������� �������� �������.</summary>
        private Func<string, SimpleQuery> _queryFactory = source => new DataRecordsQuery(source);

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
        /// <typeparam name="T">�������� ���.</typeparam>
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

            _queryFactory = source => CreateDataTypeQuery(_sourceName, selectExpression);
            _currentState = HandlerState.Selected;
        }

        /// <summary>�������� ������� ��������� ��������� ���������� ����.</summary>
        /// <param name="source">��� ���������.</param>
        /// <param name="selectExpression">��������� �������.</param>
        private static SimpleQuery CreateDataTypeQuery(string source, LambdaExpression selectExpression)
        {
            var queryType = typeof(CustomDataTypeQuery<>).MakeGenericType(selectExpression.ReturnType);
            return (SimpleQuery)Activator.CreateInstance(queryType, source, selectExpression);
        }

        /// <summary>��������� ���� �������.</summary>
        /// <param name="sourceName">��� ���������.</param>
        public void HandleGettingRecords(string sourceName)
        {
            if (_currentState != HandlerState.Enumerable && _currentState != HandlerState.Selected)
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