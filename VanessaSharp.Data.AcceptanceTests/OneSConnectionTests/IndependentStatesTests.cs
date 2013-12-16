using System;
using System.Data.Common;
using NUnit.Framework;

namespace VanessaSharp.Data.AcceptanceTests.OneSConnectionTests
{
    /// <summary>
    /// Тесты на поведение экземпляра <see cref="OneSConnection"/>
    /// независмые от его состояния.
    /// </summary>
    #if REAL_MODE
    [TestFixture(TestMode.Real)]
    #endif
    #if ISOLATED_MODE
    [TestFixture(TestMode.Isolated)]
    #endif
    public sealed class IndependentStatesTests : OneSConnectionTestsBase
    {
        public IndependentStatesTests(TestMode testMode)
                : base(testMode)
        {}
        
        /// <summary>Тестирование метода <see cref="OneSConnection.ChangeDatabase"/>.</summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TestChangeDatabase()
        {
            // Пока не реализовано, 
            // Даже когда фактического изменения нет.
            TestedInstance.ChangeDatabase(TestCatalog);
        }

        /// <summary>Тестирование метода <see cref="OneSConnection.CreateCommand"/>.</summary>
        [Test(Description = "Тестирование типизированного метода создания команды")]
        public void TestTypedCreateCommand()
        {
            var command = TestedInstance.CreateCommand();
            
            Assert.IsNotNull(command);
            Assert.AreSame(TestedInstance, command.Connection);
        }

        /// <summary>Тестирование метода <see cref="DbConnection.CreateCommand"/>.</summary>
        [Test(Description = "Тестирование реализации обобщенного метода создания команды")]
        public void TestUntypedCreateCommand()
        {
            DbConnection dbConnection = TestedInstance;
            var command = dbConnection.CreateCommand();
            
            Assert.IsNotNull(command);
            Assert.AreSame(dbConnection, command.Connection);
        }
    }
}
