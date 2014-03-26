using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    internal sealed class FromSqlExpression : SqlExpression
    {
        public FromSqlExpression(SqlExpression source)
        {
            _source = source;
        }

        public SqlExpression Source
        {
            get { return _source; }
        }
        private readonly SqlExpression _source;

        public override void BuildSql(ISqlBuilder sqlBuilder)
        {
            sqlBuilder.WriteBeginFrom();
            Source.BuildSql(sqlBuilder);
            sqlBuilder.WriteEndFrom();
        }
    }
}
