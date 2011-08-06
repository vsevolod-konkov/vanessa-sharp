namespace VanessaSharp.Data.Tests
{
    /// <summary>Базовый класс для тестов.</summary>
    public abstract class TestsBase
    {
        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        internal string TestConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Constants.TestCatalog;
                builder.User = Constants.TestUser;

                return builder.ConnectionString;
            }
        }
    }
}
