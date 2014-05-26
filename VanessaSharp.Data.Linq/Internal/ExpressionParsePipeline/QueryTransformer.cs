using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Преобразователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal sealed class QueryTransformer
    {
        /// <summary>Часть запроса, отвечающая за выборку записи.</summary>
        private static readonly SelectPartInfo<OneSDataRecord>
            _recordSelectPart = new SelectPartInfo<OneSDataRecord>(
                SqlAllColumnsExpression.Instance, OneSDataRecordReaderFactory.Default);
        
        /// <summary>Контекст разбора запроса.</summary>
        private readonly QueryParseContext _context = new QueryParseContext();

        /// <summary>
        /// Преобразователь выражения выборки.
        /// </summary>
        private readonly IExpressionTransformMethods _expressionTransformMethods;

        /// <summary>Конструктор использования.</summary>
        public QueryTransformer()
            : this(ExpressionTransformMethods.Default)
        {}

        /// <summary>Конструктор для unit-тестирования.</summary>
        /// <param name="expressionTransformMethods">Преобразователь выражения выборки.</param>
        internal QueryTransformer(IExpressionTransformMethods expressionTransformMethods)
        {
            Contract.Requires<ArgumentNullException>(expressionTransformMethods != null);

            _expressionTransformMethods = expressionTransformMethods;
        }

        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <typeparam name="T">Тип элементов результата.</typeparam>
        /// <param name="query">Запрос.</param>
        public CollectionReadExpressionParseProduct<T> Transform<T>(CustomDataTypeQuery<T> query)
        {
            Contract.Requires<ArgumentNullException>(query != null);
            Contract.Ensures(Contract.Result<CollectionReadExpressionParseProduct<T>>() != null);

            var selectPart = ParseSelectExpression(query.SelectExpression);

            return GetExpressionParseProduct(query, selectPart);
        }

        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <param name="query">Запрос.</param>
        public CollectionReadExpressionParseProduct<OneSDataRecord> Transform(DataRecordsQuery query)
        {
            Contract.Requires<ArgumentNullException>(query != null);
            Contract.Ensures(Contract.Result<CollectionReadExpressionParseProduct<OneSDataRecord>>() != null);

            return GetExpressionParseProduct(query, _recordSelectPart);
        }

        private CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SimpleQuery query, SelectPartInfo<T> selectPart)
        {
            var whereStatement = ParseFilterExpression(query.Filter);
            
            var queryStatement = new SqlQueryStatement(
                selectPart.Statement, new SqlFromStatement(query.Source), whereStatement);

            return GetExpressionParseProduct(
                queryStatement, selectPart.ItemReaderFactory);
        }

        private CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SqlQueryStatement queryStatement,
            IItemReaderFactory<T> itemReaderFactory)
        {
            return new CollectionReadExpressionParseProduct<T>(
                new SqlCommand(
                        queryStatement.BuildSql(),
                        _context.Parameters.GetSqlParameters()),
                itemReaderFactory);
        }

        private SelectPartInfo<T> ParseSelectExpression<T>(Expression<Func<OneSDataRecord, T>> selectExpression)
        {
            Contract.Requires<ArgumentNullException>(selectExpression != null);

            var part = _expressionTransformMethods.TransformSelectExpression(_context, selectExpression);

            return new SelectPartInfo<T>(
                new SqlColumnListExpression(part.Columns),
                new NoSideEffectItemReaderFactory<T>(part.SelectionFunc));
        }

        private SqlWhereStatement ParseFilterExpression(Expression<Func<OneSDataRecord, bool>> filterExpression)
        {
            if (filterExpression == null)
                return null;

            return new SqlWhereStatement(
                _expressionTransformMethods.TransformWhereExpression(_context, filterExpression));
        }
        
        #region Вспомогательные типы

        /// <summary>Струтура с информацией о выборке данных.</summary>
        /// <typeparam name="T">Тип читаемых данных.</typeparam>
        private struct SelectPartInfo<T>
        {
            public SelectPartInfo(SqlColumnSetExpression columns, IItemReaderFactory<T> itemReaderFactory) : this()
            {
                _statement = new SqlSelectStatement(columns);
                _itemReaderFactory = itemReaderFactory;
            }

            public SqlSelectStatement Statement
            {
                get { return _statement; }
            }
            private readonly SqlSelectStatement _statement;

            public IItemReaderFactory<T> ItemReaderFactory
            {
                get { return _itemReaderFactory; }
            }
            private readonly IItemReaderFactory<T> _itemReaderFactory;
        }

        #endregion
    }
}
