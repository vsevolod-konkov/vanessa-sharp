using System;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Реализация <see cref="IDataReaderFieldInfoCollection"/>
    /// с ленивой загрузкой.
    /// </summary>
    internal sealed class LazyDataReaderFieldInfoCollection : IDataReaderFieldInfoCollection
    {
        /// <summary>Ленивый загрузчик коллекции.</summary>
        private readonly Lazy<IDataReaderFieldInfoCollection> _lazyCollection; 

        /// <summary>Конструктор.</summary>
        /// <param name="loadAction">Делегат ленивого конструирования коллекции.</param>
        public LazyDataReaderFieldInfoCollection(Func<IDataReaderFieldInfoCollection> loadAction)
        {
            _lazyCollection = new Lazy<IDataReaderFieldInfoCollection>(loadAction);
        }

        /// <summary>Индексатор поля по индексу в коллекции.</summary>
        /// <param name="ordinal">Индекс поля.</param>
        public DataReaderFieldInfo this[int ordinal]
        {
            get { return _lazyCollection.Value[ordinal]; }
        }

        /// <summary>
        /// Индекс поля с заданным именем <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        public int IndexOf(string name)
        {
            return _lazyCollection.Value.IndexOf(name);
        }

        /// <summary>Количество полей в коллекции.</summary>
        public int Count
        {
            get { return _lazyCollection.Value.Count; }
        }
    }
}
