using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Реализация <see cref="IQueryProvider"/> для доступа к данным 1С.
    /// </summary>
    internal sealed class OneSQueryProvider : IOneSQueryProvider
    {
        /// <summary>Парсер LINQ-выражений.</summary>
        private readonly IExpressionParser _expressionParser;

        /// <summary>Исполнитель SQL-команд.</summary>
        private readonly ISqlCommandExecuter _sqlCommandExecuter;
        
        /// <summary>Конструктор для модульного тестирования.</summary>
        /// <param name="connection"></param>
        /// <param name="expressionParser"></param>
        /// <param name="sqlCommandExecuter"></param>
        internal OneSQueryProvider(OneSConnection connection, 
                                   IExpressionParser expressionParser,
                                   ISqlCommandExecuter sqlCommandExecuter)
        {
            Contract.Requires<ArgumentNullException>(connection != null);
            Contract.Requires<ArgumentNullException>(expressionParser != null);
            Contract.Requires<ArgumentNullException>(sqlCommandExecuter != null);

            _connection = connection;
            _expressionParser = expressionParser;
            _sqlCommandExecuter = sqlCommandExecuter;
        }
        
        /// <summary>Конструктор принимающий соединение.</summary>
        /// <param name="connection">Соединение к 1С.</param>
        public OneSQueryProvider(OneSConnection connection)
            : this(connection, ExpressionParser.Default, new SqlCommandExecuter(connection))
        {
            Contract.Requires<ArgumentNullException>(connection != null);
        }
        
        /// <summary>
        /// Соединение к 1С.
        /// </summary>
        public OneSConnection Connection
        {
            get { return _connection; }
        }
        private readonly OneSConnection _connection;

        /// <summary>
        /// Создание объекта запроса возвращающего записи из <paramref name="sourceName"/>.
        /// </summary>
        /// <param name="sourceName">Имя источника данных.</param>
        public IQueryable<OneSDataRecord> CreateGetRecordsQuery(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
            Contract.Ensures(Contract.Result<IQueryable<OneSDataRecord>>() != null);

            return CreateQuery<OneSDataRecord>(OneSQueryExpressionHelper.GetRecordsExpression(sourceName));
        }

        // TODO: Реализовать
        /// <summary>
        /// Создает объект <see cref="T:System.Linq.IQueryable"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Linq.IQueryable"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </returns>
        /// <param name="expression">Дерево выражения, представляющее запрос LINQ.</param>
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Создает объект <see cref="T:System.Linq.IQueryable`1"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Linq.IQueryable`1"/>, который позволяет вычислить запрос, представленный заданным деревом выражения.
        /// </returns>
        /// <param name="expression">Дерево выражения, представляющее запрос LINQ.</param>
        /// <typeparam name="TElement">Тип элементов возвращаемого объекта <see cref="T:System.Linq.IQueryable`1"/>.</typeparam>
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new OneSQueryable<TElement>(this, expression);
        }

        /// <summary>
        /// Выполняет запрос, представленный заданным деревом выражения.
        /// </summary>
        /// <returns>
        /// Значение, получаемое в результате выполнения указанного запроса.
        /// </returns>
        /// <param name="expression">Дерево выражения, представляющее запрос LINQ.</param>
        public object Execute(Expression expression)
        {
            return _expressionParser
                .Parse(expression)
                .Execute(_sqlCommandExecuter);
        }

        /// <summary>
        /// Выполняет строго типизированный запрос, представленный заданным деревом выражения.
        /// </summary>
        /// <returns>
        /// Значение, получаемое в результате выполнения указанного запроса.
        /// </returns>
        /// <param name="expression">Дерево выражения, представляющее запрос LINQ.</param>
        /// <typeparam name="TResult">Тип значения, получаемого в результате выполнения запроса.</typeparam>
        public TResult Execute<TResult>(Expression expression)
        {
            var resultType = expression.GetResultType();
            if (!typeof(TResult).IsAssignableFrom(resultType))
            {
                throw new ArgumentException(string.Format(
                    "Тип \"{0}\" выражения \"{1}\" не совместим с типом результата \"{2}\".",
                    resultType, expression, typeof(TResult)));
            }

            return (TResult)Execute(expression);
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
