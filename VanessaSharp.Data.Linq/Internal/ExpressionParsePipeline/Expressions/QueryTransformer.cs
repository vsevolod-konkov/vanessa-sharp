using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>Преобразователь запроса в объект <see cref="ExpressionParseProduct"/>.</summary>
    internal sealed class QueryTransformer : IQueryTransformer
    {
        /// <summary>Часть запроса, отвечающая за выборку записи.</summary>
        private static readonly SelectPartInfo<OneSDataRecord>
            _recordSelectPart = new SelectPartInfo<OneSDataRecord>(
                SqlAllColumnsExpression.Instance, OneSDataRecordReaderFactory.Default);
        
        /// <summary>Контекст разбора запроса.</summary>
        private readonly QueryParseContext _context = new QueryParseContext();

        /// <summary>Конструктор использования.</summary>
        /// <param name="mappingProvider">Поставщик соответствий типам источников данных 1С.</param>
        public QueryTransformer(IOneSMappingProvider mappingProvider)
            : this(new ExpressionTransformMethods(mappingProvider))
        {}

        /// <summary>Конструктор для unit-тестирования.</summary>
        /// <param name="expressionTransformMethods">Преобразователь выражения выборки.</param>
        internal QueryTransformer(IExpressionTransformMethods expressionTransformMethods)
        {
            Contract.Requires<ArgumentNullException>(expressionTransformMethods != null);

            _expressionTransformMethods = expressionTransformMethods;
        }

        /// <summary>
        /// Методы преобразования выборки.
        /// </summary>
        internal IExpressionTransformMethods ExpressionTransformMethods
        {
            get { return _expressionTransformMethods; }
        }
        private readonly IExpressionTransformMethods _expressionTransformMethods;

        /// <summary>
        /// Получение модели инструкции SQL-запроса.
        /// </summary>
        /// <param name="query">Исходный запрос.</param>
        /// <param name="selectStatement">SQL-Инструкция выборки.</param>
        private SqlQueryStatement GetQueryStatement<TInput, TOutput>(IQuery<TInput, TOutput> query,
                                                                     SqlSelectStatement selectStatement)
        {
            var source = query.Source.GetSourceName(_expressionTransformMethods);
            var whereStatement = ParseFilterExpression(query.Filter);
            var orderByStatement = ParseSortExpressions(query.Sorters);

            return new SqlQueryStatement(
                selectStatement, new SqlFromStatement(source), whereStatement, orderByStatement);
        }

        /// <summary>
        /// Преобразование запроса в результат 
        /// разбора выражения.
        /// </summary>
        /// <param name="query">Запрос.</param>
        public CollectionReadExpressionParseProduct<TOutput> Transform<TInput, TOutput>(IQuery<TInput, TOutput> query)
        {
            var selectPart = GetSelectPart(query);
            var queryStatement = GetQueryStatement(query, selectPart.GetStatement(query.IsDistinct));

            return GetExpressionParseProduct(
                queryStatement, selectPart.ItemReaderFactory);
        }

        /// <summary>Преобразовывает скалярные запросы в результат парсинга LINQ-выражения готового в выполнению.</summary>
        /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
        /// <typeparam name="TResult">Тип скалярного значения.</typeparam>
        /// <param name="query">Объект скалярного запроса.</param>
        public ScalarReadExpressionParseProduct<TResult> 
            TransformScalar<TInput, TOutput, TResult>(IScalarQuery<TInput, TOutput, TResult> query)
        {
            var selectStatement = GetSelectStatementForAggregate(query.Selector, query.AggregateFunction, query.IsDistinct);
            var queryStatement = GetQueryStatement(query, selectStatement);

            return new ScalarReadExpressionParseProduct<TResult>(
                BuildSqlCommand(queryStatement),
                OneSQueryExpressionHelper.GetConverter<TResult>()
                );
        }

        /// <summary>Получение инструкции выборки для агрегирования данных.</summary>
        /// <param name="selector">LINQ-выражение выборки.</param>
        /// <param name="aggregateFunction">Агрегируемая функция.</param>
        /// <param name="isDistinct">Выбирать различные.</param>
        private SqlSelectStatement GetSelectStatementForAggregate(LambdaExpression selector,
                                                                   AggregateFunction aggregateFunction,
                                                                   bool isDistinct)
        {
            return new SqlSelectStatement(
                new SqlColumnListExpression(new[]
                    {
                        GetCallAggregateFunctionExpression(aggregateFunction, selector, isDistinct)
                    }),
                false);
        }

        /// <summary>Получение вызова агрегируемой функции.</summary>
        private SqlExpression GetCallAggregateFunctionExpression(AggregateFunction aggregateFunction,
                                                                 LambdaExpression selector,
                                                                 bool isDistinct)
        {
            if (aggregateFunction == AggregateFunction.Count && selector == null)
            {
                Contract.Assert(!isDistinct);

                return SqlCountExpression.All;
            }

            SqlExpression selectExpression;
            try
            {
                selectExpression = _expressionTransformMethods.TransformExpression(_context, selector);
            }
            catch (NotSupportedException e)
            {
                throw new NotSupportedException(string.Format(
                    "Неподдерживается вызов агрегируемой функции \"{0}\" с выражением \"{1}\".",
                    aggregateFunction,
                    selector), e);
            }
            
            return GetCallAggregateFunctionExpression(aggregateFunction, selectExpression, isDistinct);
        }

        /// <summary>Получение вызова агрегируемой функции.</summary>
        private static SqlExpression GetCallAggregateFunctionExpression(AggregateFunction aggregateFunction,
                                                                        SqlExpression expression,
                                                                        bool isDistinct)
        {
            if (aggregateFunction == AggregateFunction.Count)
            {
                return SqlCountExpression.CreateForExpression(expression, isDistinct);
            }

            return new SqlAggregateFunctionExpression(GetSqlAggregateFunction(aggregateFunction), expression);
        }

        private static SqlAggregateFunction GetSqlAggregateFunction(AggregateFunction aggregateFunction)
        {
            switch (aggregateFunction)
            {
                case AggregateFunction.Summa:
                    return SqlAggregateFunction.Sum;
                case AggregateFunction.Average:
                    return SqlAggregateFunction.Avg;
                case AggregateFunction.Minimum:
                    return SqlAggregateFunction.Min;
                case AggregateFunction.Maximum:
                    return SqlAggregateFunction.Max;
                default:
                    throw new ArgumentOutOfRangeException("aggregateFunction");
            }
        }

        /// <summary>
        /// Получение информации по выборке.
        /// </summary>
        /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
        /// <typeparam name="TOutput">Тип элементов выходной последовательности запроса.</typeparam>
        /// <param name="query">Запрос.</param>
        private SelectPartInfo<TOutput> GetSelectPart<TInput, TOutput>(IQuery<TInput, TOutput> query)
        {
            SelectionPartParseProduct<TOutput> selectionPartParseProduct;
            
            if (query.Selector == null)
            {
                if (typeof(TOutput) == typeof(OneSDataRecord))
                    return (SelectPartInfo<TOutput>)(object)_recordSelectPart;

                selectionPartParseProduct = _expressionTransformMethods
                    .TransformSelectTypedRecord<TOutput>();
            }
            else
            {
                selectionPartParseProduct = _expressionTransformMethods
                    .TransformSelectExpression(_context, query.Selector);
            }

            return new SelectPartInfo<TOutput>(
                new SqlColumnListExpression(selectionPartParseProduct.Columns),
                new NoSideEffectItemReaderFactory<TOutput>(selectionPartParseProduct.SelectionFunc)
                );
        }

        /// <summary>
        /// Построение SQL-команды.
        /// </summary>
        /// <param name="queryStatement">Объект SQL-инструкции запроса.</param>
        private SqlCommand BuildSqlCommand(SqlQueryStatement queryStatement)
        {
            return new SqlCommand(
                queryStatement.BuildSql(),
                _context.Parameters.GetSqlParameters());
        }

        /// <summary>
        /// Конструирование результата парсинга для запроса последовательности элементов.
        /// </summary>
        /// <typeparam name="T">Тип элементов выходной последовательности.</typeparam>
        /// <param name="queryStatement">Объект SQL-инструкции запроса.</param>
        /// <param name="itemReaderFactory">Фабрика читателей элементов.</param>
        private CollectionReadExpressionParseProduct<T> GetExpressionParseProduct<T>(
            SqlQueryStatement queryStatement,
            IItemReaderFactory<T> itemReaderFactory)
        {
            return new CollectionReadExpressionParseProduct<T>(
                BuildSqlCommand(queryStatement),
                itemReaderFactory);
        }

        /// <summary>
        /// Парсинг и преобразование выражения фильтрации
        /// в часть where инструкции sql-запроса.
        /// </summary>
        /// <typeparam name="T">Тип элементов входной последовательности.</typeparam>
        /// <param name="filterExpression">Выражение фильтрации элементов.</param>
        private SqlWhereStatement ParseFilterExpression<T>(Expression<Func<T, bool>> filterExpression)
        {
            if (filterExpression == null)
                return null;

            return new SqlWhereStatement(
                _expressionTransformMethods.TransformCondition(_context, filterExpression));
        }

        /// <summary>
        /// Парсинг и преобразование списка выражений соритровки
        /// в часть order by инструкции sql-запроса.
        /// </summary>
        /// <param name="sortExpressions">Список выражений сортировки.</param>
        private SqlOrderByStatement ParseSortExpressions(ReadOnlyCollection<SortExpression> sortExpressions)
        {
            Contract.Requires<ArgumentNullException>(sortExpressions != null);

            if (sortExpressions.Count == 0)
                return null;

            return new SqlOrderByStatement(
                new ReadOnlyCollection<SqlSortFieldExpression>(
                    sortExpressions
                        .Select(ParseSortExpression)
                        .ToArray()
                    ));
        }

        /// <summary>
        /// Парсинг и преобразование выражения сортировки
        /// в подвыражение order by инструкции sql-запроса.
        /// </summary>
        /// <param name="sortExpression">Выражение сортировки.</param>
        private SqlSortFieldExpression ParseSortExpression(SortExpression sortExpression)
        {
            Contract.Requires<ArgumentNullException>(sortExpression != null);

            return new SqlSortFieldExpression(
                _expressionTransformMethods.TransformExpression(_context, sortExpression.KeyExpression),
                sortExpression.Kind);
        }

        #region Вспомогательные типы

        /// <summary>Струтура с информацией о выборке данных.</summary>
        /// <typeparam name="T">Тип читаемых данных.</typeparam>
        private struct SelectPartInfo<T>
        {
            private readonly SqlColumnSetExpression _columns;

            public SelectPartInfo(SqlColumnSetExpression columns, IItemReaderFactory<T> itemReaderFactory) : this()
            {
                _columns = columns;
                _itemReaderFactory = itemReaderFactory;
            }

            /// <summary>
            /// SELECT часть инструкции SQL-запроса.
            /// </summary>
            public SqlSelectStatement GetStatement(bool isDistinct)
            {
                return new SqlSelectStatement(_columns, isDistinct);
            }

            /// <summary>
            /// Фабрика читателей элементов из последовательности.
            /// </summary>
            public IItemReaderFactory<T> ItemReaderFactory
            {
                get { return _itemReaderFactory; }
            }
            private readonly IItemReaderFactory<T> _itemReaderFactory;
        }

        #endregion
    }
}
