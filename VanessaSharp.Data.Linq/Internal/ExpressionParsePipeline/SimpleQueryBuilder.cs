using System;
using System.Linq;
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
            Records,
            Ended,
        }

        /// <summary>������� ���������.</summary>
        private HandlerState _currentState = HandlerState.Starting;

        /// <summary>��� ��������� � ������������������.</summary>
        private Type _itemType;

        /// <summary>�������� ������.</summary>
        private string _sourceName;
        

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

            if (_itemType != typeof(OneSDataRecord))
            {
                throw new NotSupportedException(string.Format(
                    "�� �������������� ������������������ � ���������� ���� \"{0}\".",
                    _itemType));
            }
            _builtQuery = new SimpleQuery(_sourceName);
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

        /// <summary>��������� ���� �������.</summary>
        /// <param name="sourceName">��� ���������.</param>
        public void HandleGettingRecords(string sourceName)
        {
            if (_currentState != HandlerState.Enumerable)
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