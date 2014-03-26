using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.SqlExpressions
{
    internal sealed class FinalSqlExpression : SqlExpression
    {
        public FinalSqlExpression(SelectSqlExpression selectPart, FromSqlExpression fromPart = null)
        {
            _selectPart = selectPart;
            _fromPart = fromPart;
        }

        public SelectSqlExpression SelectPart
        {
            get { return _selectPart; }
        }
        private readonly SelectSqlExpression _selectPart;

        public FromSqlExpression FromPart
        {
            get { return _fromPart; }
        }
        private readonly FromSqlExpression _fromPart;

        public override void BuildSql(ISqlBuilder sqlBuilder)
        {
            SelectPart.BuildSql(sqlBuilder);
            if (FromPart != null)
                FromPart.BuildSql(sqlBuilder);
        }
    }
}
