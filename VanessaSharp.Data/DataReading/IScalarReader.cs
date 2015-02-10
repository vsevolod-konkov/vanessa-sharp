using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Читатель скалярного значения
    /// из результата запроса.
    /// </summary>
    [ContractClass(typeof(ScalarReaderContract))]
    internal interface IScalarReader
    {
        /// <summary>Чтение скалярного значения.</summary>
        /// <param name="queryResult">Результат запроса.</param>
        object ReadScalar(IQueryResult queryResult);
    }

    [ContractClassFor(typeof(IScalarReader))]
    internal abstract class ScalarReaderContract : IScalarReader
    {
        object IScalarReader.ReadScalar(IQueryResult queryResult)
        {
            Contract.Requires<ArgumentNullException>(queryResult != null);

            return null;
        }
    }
}
