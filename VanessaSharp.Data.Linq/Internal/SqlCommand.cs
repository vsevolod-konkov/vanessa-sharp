using System.Collections.ObjectModel;

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
            _sql = sql;
            _parameters = parameters;
        }

        /// <summary>SQL-запрос.</summary>
        public string Sql
        {
            get { return _sql; }
        }
        private readonly string _sql;

        /// <summary>Параметры запроса.</summary>
        public ReadOnlyCollection<SqlParameter> Parameters
        {
            get { return _parameters; }
        }
        private readonly ReadOnlyCollection<SqlParameter> _parameters;
    }
}
