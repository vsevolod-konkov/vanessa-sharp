using System;
using NUnit.Framework;
using System.Data;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тесты проверяющие правильность поведения выполнения запросов команды.</summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real, Description = "Тестирование метода Execute для реального режима.")]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated, Description = "Тестирование метода Execute для изоляционного режима.")]
    #endif
    public sealed class GeneralExecuteTests : CommandTestsBase
    {
        private const string TEST_SQL = "ВЫБРАТЬ Справочник.Валюты.Код КАК Код, Справочник.Валюты.Наименование КАК Наименование ИЗ Справочник.Валюты";

        /// <summary>Параметрический конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        public GeneralExecuteTests(TestMode testMode)
            : base(testMode, true)
        {}

        /// <summary>Установка текста запроса.</summary>
        [SetUp]
        public void SetUpCommandText()
        {
            base.TestedCommand.CommandText = TEST_SQL;
        }
        
        /// <summary>
        /// Тестирование выполнения простого запроса.
        /// </summary>
        [Test(Description="Тестирование выполнения простого запроса")]
        [Ignore("Тест имеет неприятный побочный эффект")]
        public void TestCommandSimpleExecute()
        {
            using (TestedCommand.ExecuteReader())
            {}
        }

        /// <summary>
        /// Проверка того что поведение команды не по умолчанию не поддерживается.
        /// </summary>
        /// <param name="behavior"></param>
        [Test(Description="Проверка того что поведение команды не по умолчанию не поддерживается")]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestNotSupportedNonDefaultCommandBehavior(
            [Values(
                    CommandBehavior.SingleRow, 
                    //CommandBehavior.SingleResult, 
                    //CommandBehavior.SequentialAccess, 
                    CommandBehavior.SchemaOnly, 
                    CommandBehavior.KeyInfo, 
                    CommandBehavior.CloseConnection
                    )]
            CommandBehavior behavior)
        {
            using (TestedCommand.ExecuteReader(behavior))
            {}
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> равно <c>null</c>.
        /// </summary>
        [Test(Description="Тестирование того, что при не заданном подключении, будет исключение InvalidOperationException")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidOperationIfNullConnection()
        {
            TestedCommand.Connection = null;
            TestedCommand.ExecuteReader();
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> находится в закрытом состоянии,
        /// то есть <see cref="OneSConnection.State"/> равно <see cref="ConnectionState.Closed"/>.
        /// </summary>
        [Test(Description = "Тестирование факта выброса исключения InvalidOperationException в случае если соединение находится в закрытом состоянии")]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestInvalidOperationIfClosedConnection()
        {
            TestedCommand.Connection.Close();
            TestedCommand.ExecuteReader();
        }
    }
}
