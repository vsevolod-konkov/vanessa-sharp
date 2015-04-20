using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>Выражение для вызова встроенной функции.</summary>
    internal sealed class SqlEmbeddedFunctionExpression : SqlExpression
    {
        /// <summary>Выражение вызова функции получения подстроки.</summary>
        /// <param name="expression">Выражение исходной строки.</param>
        /// <param name="position">Выражение позиции.</param>
        /// <param name="length">Выражение длины.</param>
        public static SqlEmbeddedFunctionExpression Substring(
            SqlExpression expression,
            SqlExpression position,
            SqlExpression length)
        {
            return Create(SqlEmbeddedFunction.Substring,
                expression, position, length);
        }

        /// <summary>
        /// Выражение вызова функции получения года.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Year(
            SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Year, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения квартала.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Quarter(
            SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Quarter, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения месяца.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Month(
            SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Month, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения дня года.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression DayOfYear(
            SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.DayOfYear, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения дня месяца.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Day(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Day, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения недели года даты.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Week(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Week, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения дня недели даты.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression DayWeek(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.DayWeek, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения часа даты.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Hour(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Hour, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения минуты даты.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Minute(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Minute, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции получения секунды даты.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        public static SqlEmbeddedFunctionExpression Second(SqlExpression dateExpression)
        {
            return Create(SqlEmbeddedFunction.Second, dateExpression);
        }

        /// <summary>
        /// Выражение вызова функции определения начальной даты периода.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        /// <param name="kind">Вид периода.</param>
        public static SqlEmbeddedFunctionExpression BeginOfPeriod(SqlExpression dateExpression, OneSTimePeriodKind kind)
        {
            return CreatePeriod(
                SqlEmbeddedFunction.BeginOfPeriod,
                dateExpression, kind);
        }

        /// <summary>
        /// Выражение вызова функции определения конечной даты периода.
        /// </summary>
        /// <param name="dateExpression">Выражение даты.</param>
        /// <param name="kind">Вид периода.</param>
        public static SqlEmbeddedFunctionExpression EndOfPeriod(SqlExpression dateExpression, OneSTimePeriodKind kind)
        {
            return CreatePeriod(
                SqlEmbeddedFunction.EndOfPeriod,
                dateExpression, kind);
        }

        private static SqlEmbeddedFunctionExpression CreatePeriod(
            SqlEmbeddedFunction function, SqlExpression date, OneSTimePeriodKind kind)
        {
            return Create(function, date, SqlLiteralExpression.Create(kind));
        }

        private static SqlEmbeddedFunctionExpression Create(
            SqlEmbeddedFunction function, params SqlExpression[] args)
        {
            return new SqlEmbeddedFunctionExpression(
                function,
                new ReadOnlyCollection<SqlExpression>(args));
        }

        private SqlEmbeddedFunctionExpression(
            SqlEmbeddedFunction function, 
            ReadOnlyCollection<SqlExpression> arguments)
        {
            Contract.Requires<ArgumentNullException>(arguments != null);
            Contract.Requires<ArgumentNullException>(Contract.ForAll(arguments, a => a != null));
            
            _function = function;
            _arguments = arguments;
        }

        /// <summary>
        /// Встроенная функция SQL.
        /// </summary>
        public SqlEmbeddedFunction Function
        {
            get { return _function; }
        }
        private readonly SqlEmbeddedFunction _function;

        /// <summary>
        /// Аргументы вызова функции.
        /// </summary>
        public ReadOnlyCollection<SqlExpression> Arguments
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<SqlExpression>>() != null);
                Contract.Ensures(Contract.ForAll(Contract.Result<ReadOnlyCollection<SqlExpression>>(), a => a != null));

                return _arguments;
            }
        }
        private readonly ReadOnlyCollection<SqlExpression> _arguments;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherFunction = other as SqlEmbeddedFunctionExpression;

            return (otherFunction != null)
                   && (Function == otherFunction.Function)
                   && Arguments.SequenceEqual(otherFunction.Arguments);
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return typeof(SqlEmbeddedFunctionExpression).GetHashCode()
                   ^ Function.GetHashCode()
                   ^ Arguments.Aggregate(0, (c, a) => c ^ a.GetHashCode());
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append(GetSqlForEmbeddedFunction(Function));
            sqlBuilder.Append("(");

            var separator = string.Empty;
            foreach (var argument in Arguments)
            {
                sqlBuilder.Append(separator);
                argument.BuildSql(sqlBuilder);

                separator = ", ";
            }

            sqlBuilder.Append(")");
        }

        /// <summary>Получение SQL-имени для встроенной функции.</summary>
        /// <param name="function">Функция.</param>
        private static string GetSqlForEmbeddedFunction(SqlEmbeddedFunction function)
        {
            if (!Enum.GetValues(typeof(SqlEmbeddedFunction)).OfType<SqlEmbeddedFunction>().Contains(function))
            {
                throw new ArgumentOutOfRangeException("function", function,
                                                      string.Format("Ошибочное значение встроенной функции \"{0}\".",
                                                                    function));
            }

            return Enum.GetName(typeof(SqlEmbeddedFunction), function).ToUpperInvariant();
        }
    }
}
