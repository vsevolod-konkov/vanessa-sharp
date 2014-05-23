using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Инструкция описания источника данных SQL-запроса.
    /// </summary>
    internal sealed class SqlFromStatement
    {
        /// <summary>Конструктор.</summary>
        /// <param name="source">Наименование источника.</param>
        public SqlFromStatement(string source)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(source));

            _source = source;
        }

        /// <summary>Наменование источника.</summary>
        public string Source
        {
            get { return _source; }
        }
        private readonly string _source;

        /// <summary>Генерация SQL-запроса.</summary>
        public void BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);

            sqlBuilder.Append("FROM ");
            sqlBuilder.Append(Source);
        }
    }
}