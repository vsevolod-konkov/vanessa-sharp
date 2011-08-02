namespace VsevolodKonkov.OneSSharp.Data.Tests
{
    /// <summary>Базовый класс для тестов.</summary>
    public abstract class TestsBase
    {
        /// <summary>Настройки.</summary>
        internal Properties.Settings Settings
        {
            get { return Properties.Settings.Default; }
        }

        /// <summary>Строка соединения с тестовой информационной базой 1С.</summary>
        internal string TestConnectionString
        {
            get
            {
                var builder = new OneSConnectionStringBuilder();
                builder.Catalog = Settings.TestCatalog;
                builder.User = Settings.TestUser;

                return builder.ConnectionString;
            }
        }
    }
}
