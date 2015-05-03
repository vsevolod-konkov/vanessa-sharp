using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение операции выбора.
    /// </summary>
    internal sealed class SqlCaseExpression : SqlExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="condition">Условие выбора значения.</param>
        /// <param name="value">Значение при выполнении условия.</param>
        /// <param name="elseValue">Значение в случае невыполнения условия.</param>
        public SqlCaseExpression(SqlCondition condition, SqlExpression value, SqlExpression elseValue)
            : this(new [] { new SqlCase(condition, value) }, elseValue)
        {
            Contract.Requires<ArgumentNullException>(condition != null);
            Contract.Requires<ArgumentNullException>(value != null);
            Contract.Requires<ArgumentNullException>(elseValue != null);
        }

        /// <summary>Конструктор.</summary>
        /// <param name="cases">Варианты.</param>
        /// <param name="defaultValue">Значение по умолчанию.</param>
        public SqlCaseExpression(IList<SqlCase> cases, SqlExpression defaultValue)
        {
            Contract.Requires<ArgumentNullException>(
                cases != null && cases.Count > 0 && Contract.ForAll(cases, c => c != null));

            Contract.Requires<ArgumentNullException>(defaultValue != null);

            _cases = new ReadOnlyCollection<SqlCase>(cases.ToArray());
            _defaultValue = defaultValue;
        }

        /// <summary>Варианты.</summary>
        public ReadOnlyCollection<SqlCase> Cases
        {
            get
            {
                Contract.Ensures(
                    Contract.Result<ReadOnlyCollection<SqlCase>>() != null
                    &&
                    Contract.Result<ReadOnlyCollection<SqlCase>>().Count > 0
                    &&
                    Contract.ForAll(Contract.Result<ReadOnlyCollection<SqlCase>>(), c => c != null)
                    );

                return _cases;
            }
        }
        private readonly ReadOnlyCollection<SqlCase> _cases;

        /// <summary>Значение по умолчанию.</summary>
        public SqlExpression DefaultValue
        {
            get
            {
                Contract.Ensures(Contract.Result<SqlExpression>() != null);

                return _defaultValue;
            }
        }
        private readonly SqlExpression _defaultValue;

        protected override bool OverrideEquals(SqlExpression other)
        {
            var otherCase = other as SqlCaseExpression;

            return otherCase != null
                   && Cases.Count == otherCase.Cases.Count
                   && Enumerable.Range(0, Cases.Count).All(i => Cases[i].Equals(otherCase.Cases[i]))
                   && DefaultValue.Equals(otherCase.DefaultValue);
        }

        /// <summary>
        /// Играет роль хэш-функции для определенного типа. 
        /// </summary>
        /// <returns>
        /// Хэш-код для текущего объекта <see cref="T:System.Object"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return 
                typeof(SqlCaseExpression).GetHashCode()
                ^ Cases.Aggregate(0, (c, sqlCase) => c ^ sqlCase.GetHashCode())
                ^ DefaultValue.GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        public override void BuildSql(StringBuilder sqlBuilder)
        {
            sqlBuilder.Append("CASE");

            foreach (var sqlCase in Cases)
            {
                sqlBuilder.Append(" WHEN ");
                sqlCase.Condition.BuildSql(sqlBuilder);
                sqlBuilder.Append(" THEN ");
                sqlCase.Value.BuildSql(sqlBuilder);
            }

            sqlBuilder.Append(" END");
        }
    }
}
