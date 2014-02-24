using System;
using NUnit.Framework;
using System.Data;

namespace VanessaSharp.Data.AcceptanceTests.OneSCommandTests
{
    /// <summary>Тесты проверяющие правильность поведения выполнения запросов команды.</summary>
    public abstract class GeneralExecuteTestsBase : CommandTestsBase
    {
        /// <summary>Тестовый запрос.</summary>
        protected const string TEST_SQL = "ВЫБРАТЬ * ИЗ Справочник.ТестовыйСправочник";

        /// <summary>Параметрический конструктор.</summary>
        /// <param name="testMode">Режим тестирования.</param>
        protected GeneralExecuteTestsBase(TestMode testMode)
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
        public void TestCommandSimpleExecute()
        {
            using (var reader = TestedCommand.ExecuteReader())
            {
                Assert.IsNotNull(reader);
            }
            AssertIsExecuteReaderIsSuccess();
        }

        /// <summary>
        /// Проверка того что поведение команды не по умолчанию не поддерживается.
        /// </summary>
        /// <param name="behavior"></param>
        [Test(Description="Проверка того что поведение команды не по умолчанию не поддерживается")]
        public void TestNotSupportedNonDefaultCommandBehavior(
            [Values(
                    CommandBehavior.SingleRow, 
                    CommandBehavior.SchemaOnly, 
                    CommandBehavior.KeyInfo, 
                    CommandBehavior.CloseConnection
                    )]
            CommandBehavior behavior)
        {
            Assert.Throws<NotSupportedException>(
                () => TestedCommand.ExecuteReader(behavior));
            AssertIfExecuteReaderIsInvalid();
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> равно <c>null</c>.
        /// </summary>
        [Test(Description="Тестирование того, что при не заданном подключении, будет исключение InvalidOperationException")]
        public void TestInvalidOperationIfNullConnection()
        {
            TestedCommand.Connection = null;
            TestInvalidOperationExecuteReader();
        }

        /// <summary>
        /// Тестирование факта выброса исключения <see cref="InvalidOperationException"/>
        /// в случае если <see cref="OneSCommand.Connection"/> находится в закрытом состоянии,
        /// то есть <see cref="OneSConnection.State"/> равно <see cref="ConnectionState.Closed"/>.
        /// </summary>
        [Test(Description = "Тестирование факта выброса исключения InvalidOperationException в случае если соединение находится в закрытом состоянии")]
        public void TestInvalidOperationIfClosedConnection()
        {
            TestedCommand.Connection.Close();
            TestInvalidOperationExecuteReader();
        }

        private void TestInvalidOperationExecuteReader()
        {
            Assert.Throws<InvalidOperationException>(
                () => TestedCommand.ExecuteReader());
            AssertIfExecuteReaderIsInvalid();
        }

        /// <summary>
        /// Дополнительная проверка в случае если выполнение 
        /// метода <see cref="OneSCommand.ExecuteReader()"/>
        /// вызвало исключение.
        /// </summary>
        protected virtual void AssertIfExecuteReaderIsInvalid()
        {}

        /// <summary>
        /// Дополнительная проверка в случае если выполнение 
        /// метода <see cref="OneSCommand.ExecuteReader()"/>
        /// было успешным.
        /// </summary>
        protected virtual void AssertIsExecuteReaderIsSuccess()
        {}
    }
}
