using System.Text;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel
{
    /// <summary>
    /// Выражение таблицы по умолчанию.
    /// </summary>
    internal sealed class SqlDefaultTableExpression : SqlExpression
    {
        private SqlDefaultTableExpression()
        {}

        /// <summary>Экземпляр по умолчанию.</summary>
        public static SqlDefaultTableExpression Instance
        {
            get { return _instance; }    
        }
        private static readonly SqlDefaultTableExpression _instance = new SqlDefaultTableExpression();

        protected override bool OverrideEquals(SqlExpression other)
        {
            return other is SqlDefaultTableExpression;
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
            return typeof(SqlDefaultTableExpression).GetHashCode();
        }

        /// <summary>Генерация SQL-кода.</summary>
        protected override void BuildSql(StringBuilder sqlBuilder)
        {}
    }
}
