using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Коллекция с информацией по полям читателя данных <see cref="OneSDataReader"/>.
    /// </summary>
    [ContractClass(typeof(DataReaderFieldInfoCollectionContract))]
    internal interface IDataReaderFieldInfoCollection
    {
        /// <summary>Индексатор поля по индексу в коллекции.</summary>
        /// <param name="ordinal">Индекс поля.</param>
        DataReaderFieldInfo this[int ordinal] { get; }

        /// <summary>
        /// Индекс поля с заданным именем <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        int IndexOf(string name);

        /// <summary>Количество полей в коллекции.</summary>
        int Count { get; }
    }

    [ContractClassFor(typeof(IDataReaderFieldInfoCollection))]
    internal abstract class DataReaderFieldInfoCollectionContract : IDataReaderFieldInfoCollection
    {
        /// <summary>Индексатор поля по индексу в коллекции.</summary>
        /// <param name="ordinal">Индекс поля.</param>
        DataReaderFieldInfo IDataReaderFieldInfoCollection.this[int ordinal]
        {
            get
            {
                Contract.Requires<ArgumentOutOfRangeException>(ordinal >= 0 && ordinal < ((IDataReaderFieldInfoCollection)this).Count);
                Contract.Ensures(Contract.Result<DataReaderFieldInfo>() != null);

                return null;
            }
        }

        /// <summary>
        /// Индекс поля с заданным именем <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Имя поля.</param>
        int IDataReaderFieldInfoCollection.IndexOf(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));
            Contract.Ensures(
                (Contract.Result<int>() == -1) 
                || 
                (Contract.Result<int>() >= 0 && Contract.Result<int>() < ((IDataReaderFieldInfoCollection)this).Count));

            return 0;
        }

        /// <summary>Количество полей в коллекции.</summary>
        int IDataReaderFieldInfoCollection.Count
        {
            get { return 0; }
        }
    }
}
