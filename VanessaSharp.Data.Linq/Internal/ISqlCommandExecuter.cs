using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Исполнитель SQL-запроса.</summary>
    [ContractClass(typeof(SqlCommandExecuterContract))]
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
    internal abstract class SqlCommandExecuterContract : ISqlCommandExecuter
    {
        /// <summary>Выполнение SQL-запроса для получения табличных данных.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        ISqlResultReader ISqlCommandExecuter.ExecuteReader(SqlCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);
            Contract.Ensures(Contract.Result<ISqlResultReader>() != null);

            return null;
        }

        /// <summary>Выполенение SQL-запроса для получения скалярного значения.</summary>
        /// <param name="command">Команда SQL-запроса.</param>
        object ISqlCommandExecuter.ExecuteScalar(SqlCommand command)
        {
            Contract.Requires<ArgumentNullException>(command != null);

            return null;
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public abstract void Dispose();
    }
}
