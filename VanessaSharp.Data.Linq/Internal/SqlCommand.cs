using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal
{
    /// <summary>Команда SQL-запроса.</summary>
    /// <remarks>Немутабельная структура.</remarks>
    internal sealed class SqlCommand
    {
        /// <summary>Конструктор.</summary>
        /// <param name="sql">SQL-запрос.</param>
        /// <param name="parameters">Параметры запроса.</param>
        public SqlCommand(string sql, ReadOnlyCollection<SqlParameter> parameters)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sql));
            Contract.Requires<ArgumentNullException>(parameters != null);

            _sql = sql;
            _parameters = parameters;
        }

        /// <summary>SQL-запрос.</summary>
        public string Sql
        {
            get
            {
                Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<string>()));

                return _sql;
            }
        }
        private readonly string _sql;

        /// <summary>Параметры запроса.</summary>
        public ReadOnlyCollection<SqlParameter> Parameters
        {
            get
            {
                Contract.Ensures(Contract.Result<ReadOnlyCollection<SqlParameter>>() != null);

                return _parameters;
            }
        }
        private readonly ReadOnlyCollection<SqlParameter> _parameters;
    }
}
