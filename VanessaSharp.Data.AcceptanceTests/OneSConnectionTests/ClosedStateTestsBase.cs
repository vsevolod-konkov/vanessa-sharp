using System;
using System.Data;
using NUnit.Framework;
using VanessaSharp.AcceptanceTests.Utility;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Базовый класс для
    /// тестов на случаи когда экземпляр 
    /// <see cref="OneSConnection"/> имеет 
    /// состояние <see cref="OneSConnection.State"/>
    /// в значении <see cref="ConnectionState.Closed"/>.
    /// </summary>
    public abstract class ClosedStateTestsBase : OneSConnectionTestsBase
    {
        /// <summary>Соединение было открыто, а потом закрыто.</summary>
        private readonly bool _hadOpened;

        /// <summary>Параметрический конструктор.</summary>
        /// <param name="testMode">Редим тестирования.</param>
        /// <param name="hadOpened">Было ли открыто соединение.</param>
        protected ClosedStateTestsBase(TestMode testMode, bool hadOpened)
            : base(testMode)
        {
            _hadOpened = hadOpened;
        }

        /// <summary>Действия выполняемые перед тестом.</summary>
        [SetUp]
        public void OnAfterInitConnection()
        {
            if (_hadOpened)
            {
                TestedInstance.Open();
                TestedInstance.Close();
            }
        }

        /// <summary>
        /// Тестирование действия,
        /// в случае передачи невалидной строки соединения.
        /// </summary>
        /// <param name="action">Действие.</param>
        protected void TestActionWhenInvalidConnectionString(
            TestDelegate action)
        {
            TestedInstance.ConnectionString = "белеберда";
            Assert.Throws<InvalidOperationException>(action);
        }
    }
}
