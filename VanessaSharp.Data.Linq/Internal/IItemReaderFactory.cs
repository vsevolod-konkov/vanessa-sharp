using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Фабрика создания читателя элемента.</summary>
    [ContractClass(typeof(IItemReaderFactoryContract<>))]
    internal interface IItemReaderFactory<out T>
    {
        /// <summary>
        /// Создание читателя элементов.
        /// </summary>
        Func<object[], T> CreateItemReader(ISqlResultReader sqlResultReader);
    }

    [ContractClassFor(typeof(IItemReaderFactory<>))]
    internal abstract class IItemReaderFactoryContract<T> : IItemReaderFactory<T>
    {
        Func<object[], T> IItemReaderFactory<T>.CreateItemReader(ISqlResultReader sqlResultReader)
        {
            Contract.Requires<ArgumentNullException>(sqlResultReader != null);
            Contract.Ensures(Contract.Result<Func<object[], T>>() != null);

            return null;
        }
    }
}