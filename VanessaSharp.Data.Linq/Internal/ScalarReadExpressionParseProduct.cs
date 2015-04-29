using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Продукт парсинга выражения чтения скалярного значения.
    /// </summary>
    /// <typeparam name="T">Тип скалярного значения.</typeparam>
    internal sealed class ScalarReadExpressionParseProduct<T> : ExpressionParseProduct
    {
        /// <summary>Конструктор.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        /// <param name="converter">Преобразователь значений.</param>
        public ScalarReadExpressionParseProduct(SqlCommand command, Func<IValueConverter, object, T> converter)
            : base(command)
        {
            Contract.Requires<ArgumentNullException>(converter != null);

            _converter = converter;
        }

        /// <summary>Преобразователь значений.</summary>
        public Func<IValueConverter, object, T> Converter
        {
            get { return _converter; }
        }
        private readonly Func<IValueConverter, object, T> _converter;

        /// <summary>Выполнение запроса.</summary>
        /// <param name="executer">Выполнитель запроса.</param>
        public override object Execute(ISqlCommandExecuter executer)
        {
            var rawResult = executer.ExecuteScalar(Command);

            return Converter(rawResult.Item1, rawResult.Item2);
        }
    }
}
