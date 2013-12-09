using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для тестов экземпляра <see cref="OneSConnection"/>.
    /// </summary>
    public abstract class OneSConnectionTestsBase : TestsBase
    {
        /// <summary>Тестируемый экземпляр.</summary>
        protected OneSConnection TestedInstance
        {
            get { return _testedInstance; }

            set { _testedInstance = value; }
        }
        private OneSConnection _testedInstance;

        /// <summary>Очистка ресурсов теста.</summary>
        [TearDown]
        public void TearDown()
        {
            if (_testedInstance != null)
            {
                _testedInstance.Dispose();
                _testedInstance = null;
            }
        }
    }
}
