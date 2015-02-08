using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Интерфейс специального конвертера для 
    /// объектов 1С, с которыми требуется работать через
    /// стандартные API .Net.
    /// </summary>
    [ContractClass(typeof(OneSObjectSpecialConverterContract))]
    internal interface IOneSObjectSpecialConverter
    {
        /// <summary>
        /// Преобразование объекта 1С к читателю данных.
        /// </summary>
        /// <param name="oneSObject">Объект 1С.</param>
        OneSDataReader ToDataReader(object oneSObject);

        /// <summary>
        /// Преобразование объекта 1С к <see cref="Guid"/>.
        /// </summary>
        /// <param name="oneSObject">Объект 1С.</param>
        Guid ToGuid(object oneSObject);
    }

    [ContractClassFor(typeof(IOneSObjectSpecialConverter))]
    internal abstract class OneSObjectSpecialConverterContract : IOneSObjectSpecialConverter
    {
        OneSDataReader IOneSObjectSpecialConverter.ToDataReader(object oneSObject)
        {
            Contract.Requires<ArgumentNullException>(oneSObject != null);
            Contract.Ensures(Contract.Result<OneSDataReader>() != null);

            throw new NotImplementedException();
        }


        Guid IOneSObjectSpecialConverter.ToGuid(object oneSObject)
        {
            Contract.Requires<ArgumentNullException>(oneSObject != null);
            
            throw new NotImplementedException();
        }
    }
}
