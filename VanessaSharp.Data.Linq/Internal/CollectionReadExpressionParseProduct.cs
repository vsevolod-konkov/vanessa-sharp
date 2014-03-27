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
        /// <param name="itemReaderFactory">Фабрика создания читателя элемента.</param>
        public CollectionReadExpressionParseProduct(SqlCommand command, IItemReaderFactory<T> itemReaderFactory) 
            : base(command)
        {
            Contract.Requires<ArgumentNullException>(itemReaderFactory != null);

            _itemReaderFactory = itemReaderFactory;
        }

        /// <summary>Фабрика создания читателя элемента.</summary>
        public IItemReaderFactory<T> ItemReaderFactory
        {
            get { return _itemReaderFactory; }
        }
        private readonly IItemReaderFactory<T> _itemReaderFactory;

        /// <summary>Выполнение запроса.</summary>
        /// <param name="executer">Выполнитель запроса.</param>
        public override object Execute(ISqlCommandExecuter executer)
        {
            var sqlReader = executer.ExecuteReader(Command);
            try
            {
                var itemReader = ItemReaderFactory.CreateItemReader(sqlReader);
                return new ItemEnumerator<T>(sqlReader, itemReader);
            }
            catch
            {
                sqlReader.Dispose();
                throw;
            }
        }
    }
}
