using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Интерфейс объекта синтаксической конструкции SQL.
    /// </summary>
    [ContractClass(typeof(SqlObjectContract))]
    internal interface ISqlObject
    {
        /// <summary>Построение SQL-строки.</summary>
        void BuildSql(StringBuilder sqlBuilder);

        /// <summary>Имеются ли пробелы в SQL.</summary>
        bool HasSpaces { get; }
    }

    [ContractClassFor(typeof(ISqlObject))]
    internal abstract class SqlObjectContract : ISqlObject
    {
        /// <summary>Построение SQL-строки.</summary>
        void ISqlObject.BuildSql(StringBuilder sqlBuilder)
        {
            Contract.Requires<ArgumentNullException>(sqlBuilder != null);
        }

        /// <summary>Имеются ли пробелы в SQL.</summary>
        bool ISqlObject.HasSpaces
        {
            get { return false; }
        }
    }
}
