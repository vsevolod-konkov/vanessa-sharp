using System.Data.Common;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Реализация <see cref="DbProviderFactory"/> для доступа к данным 1С.
    /// </summary>
    public sealed class OneSProviderFactory : DbProviderFactory
    {
        /// <summary>Единичный экземпляр.</summary>
        public static readonly OneSProviderFactory Instance = new OneSProviderFactory();

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnectionStringBuilder"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbConnectionStringBuilder"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new OneSConnectionStringBuilder();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbConnection"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbConnection"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override DbConnection CreateConnection()
        {
            return new OneSConnection();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbCommand"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbCommand"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override DbCommand CreateCommand()
        {
            return new OneSCommand();
        }

        /// <summary>
        /// Returns a new instance of the provider's class that implements the <see cref="T:System.Data.Common.DbDataAdapter"/> class.
        /// </summary>
        /// <returns>
        /// A new instance of <see cref="T:System.Data.Common.DbDataAdapter"/>.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override DbDataAdapter CreateDataAdapter()
        {
            return new OneSDataAdapter();
        }
    }
}
