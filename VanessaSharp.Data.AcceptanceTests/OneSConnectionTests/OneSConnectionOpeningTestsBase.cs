using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра <see cref="OneSConnection"/>
    /// с возможностью открытия соединения с тестовой БД 1С.
    /// </summary>
    public class OneSConnectionOpeningTestsBase : OneSConnectionTestsBase
    {
        /// <summary>Инициализация тестового окружения.</summary>
        [SetUp]
        public void SetUp()
        {
            TestedInstance = new OneSConnection(TestConnectionString);
        }

        /// <summary>Тестируемый экземпляр.</summary>
        protected new OneSConnection TestedInstance
        {
            get { return base.TestedInstance; }

            private set { base.TestedInstance = value; }
        }
    }
}
