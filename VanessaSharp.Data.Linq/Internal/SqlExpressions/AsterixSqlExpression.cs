using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    internal sealed class AsterixSqlExpression : SqlExpression
    {
        public override void BuildSql(ISqlBuilder sqlBuilder)
        {
            sqlBuilder.WriteAsterix();
        }
    }
}
