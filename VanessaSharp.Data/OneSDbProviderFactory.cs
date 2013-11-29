using System.Data.Common;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Реализация <see cref="DbProviderFactory"/> для доступа к данным 1С.
    /// </summary>
    public sealed class OneSDbProviderFactory : DbProviderFactory
    {
        /// <summary>Единичный экземпляр.</summary>
        public static readonly OneSDbProviderFactory Instance = new OneSDbProviderFactory();

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
    }
}
