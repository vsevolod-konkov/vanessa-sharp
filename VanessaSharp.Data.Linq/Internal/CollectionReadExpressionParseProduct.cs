using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>
    /// Продукт парсинга выражения чтения коллекции.
    /// </summary>
    /// <typeparam name="T">Тип элемента коллекции.</typeparam>
    internal sealed class CollectionReadExpressionParseProduct<T> : ExpressionParseProduct
    {
        /// <summary>Конструктор.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        /// <param name="itemReader">Читатель элемента из записи результата запроса.</param>
        public CollectionReadExpressionParseProduct(SqlCommand command, Func<ISqlResultReader, T> itemReader) 
            : base(command)
        {
            Contract.Requires<ArgumentNullException>(itemReader != null);

            _itemReader = itemReader;
        }

        /// <summary>Читатель элемента из записи результата запроса.</summary>
        public Func<ISqlResultReader, T> ItemReader
        {
            get { return _itemReader; }
        }
        private readonly Func<ISqlResultReader, T> _itemReader;

        /// <summary>Выполнение запроса.</summary>
        /// <param name="executer">Выполнитель запроса.</param>
        public override object Execute(ISqlCommandExecuter executer)
        {
            return new ItemEnumerator<T>(
                executer.ExecuteReader(Command),
                ItemReader);
        }
    }
}
