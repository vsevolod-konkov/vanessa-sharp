namespace VanessaSharp.Data.AcceptanceTests
{
    /// <summary>Базовый класс для тестов.</summary>
    public abstract class TestsBase
    {
        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        protected string TestConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder
                    {
                        Catalog = Constants.TestCatalog,
                        User = Constants.TEST_USER
                    };

                return builder.ConnectionString;
            }
        }

        /// <summary>Путь к тестовой БД 1С.</summary>
        protected string TestCatalog
        {
            get { return Constants.TestCatalog; }
        }

        /// <summary>Тестовый пользователь.</summary>
        protected string TestUser
        {
            get { return Constants.TEST_USER; }
        }
    }
}
