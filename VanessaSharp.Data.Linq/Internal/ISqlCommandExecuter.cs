using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Исполнитель SQL-запроса.</summary>
    [ContractClass(typeof(ISqlCommandExecuterContract))]
    internal interface ISqlCommandExecuter : IDisposable
    {
        /// <summary>Выполнение SQL-запроса для получения табличных данных.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        ISqlResultReader ExecuteReader(SqlCommand command);

        /// <summary>Выполенение SQL-запроса для получения скалярного значения.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        object ExecuteScalar(SqlCommand command);
    }

    [ContractClassFor(typeof(ISqlCommandExecuter))]
    internal abstract class ISqlCommandExecuterContract : ISqlCommandExecuter
    {
        ISqlResultReader ISqlCommandExecuter.ExecuteReader(SqlCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Ensures(Contract.Result<ISqlResultReader>() != null);

            return null;
        }

        object ISqlCommandExecuter.ExecuteScalar(SqlCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return null;
        }

        public abstract void Dispose();
    }
}
