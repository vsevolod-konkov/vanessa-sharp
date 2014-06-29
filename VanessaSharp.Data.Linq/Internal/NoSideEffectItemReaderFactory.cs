using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Фабрика создания читателя элементов.</summary>
    /// <typeparam name="T">Тип элементов.</typeparam>
    internal sealed class NoSideEffectItemReaderFactory<T> : IItemReaderFactory<T>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="itemReader">Читатель элементов - без side-эффектов.</param>
        public NoSideEffectItemReaderFactory(Func<IValueConverter, object[], T> itemReader)
        {
            Contract.Requires<ArgumentNullException>(itemReader != null);
            
            _itemReader = itemReader;
        }

        /// <summary>Читатель элементов.</summary>
        internal Func<IValueConverter, object[], T> ItemReader
        {
            get
            {
                Contract.Ensures(Contract.Result<Func<IValueConverter, object[], T>>() != null);
                
                return _itemReader;
            }
        }
        private readonly Func<IValueConverter, object[], T> _itemReader;

        /// <summary>
        /// Создание читателя элементов.
        /// </summary>
        public Func<object[], T> CreateItemReader(ISqlResultReader sqlResultReader)
        {
            var valueConverter = sqlResultReader.ValueConverter;

            return values => _itemReader(valueConverter, values);
        }
    }
}
