using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>
    /// Базовый класс для тестов для созданного экземпляра <see cref="OneSCommand"/>.
    /// </summary>
    public abstract class CommandTestsBase : ConnectedTestsBase
    {
        /// <summary>Конструктор.</summary>
        /// <param name="shouldBeOpen">Признак необходимости открытия соединения.</param>
        protected CommandTestsBase(bool shouldBeOpen)
            : base(TestMode.Real, shouldBeOpen)
        {}

        /// <summary>Тестовый экземпляр команды.</summary>
        protected OneSCommand TestedCommand
        {
            get { return _testedCommand; }
        }
        private OneSCommand _testedCommand;

        /// <summary>Установка тестового окружения.</summary>
        /// <remarks>Создание тестовой команды <see cref="_testedCommand"/>.</remarks>
        [SetUp]
        public void SetUpTestedCommand()
        {
            _testedCommand = new OneSCommand(Connection);
        }

        /// <summary>Очистка окружения тестов.</summary>
        /// <remarks>Очистка тестовой команды <see cref="_testedCommand"/>.</remarks>
        public void TearDownTestedCommand()
        {
            if (_testedCommand != null)
                _testedCommand.Dispose();
        }
    }
}
