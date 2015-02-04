using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>Тестирование <see cref="OneSDataReader"/>.</summary>
    [TestFixture(false, Description = "Когда не было ни одной строки")]
    [TestFixture(true, Description = "Когда была хотя бы одна строка")]
    public sealed class EofStateTests : OpenStateTestBase
    {
        /// <summary>Мок курсора данных.</summary>
        private DisposableMock<IDataCursor> _dataCursorMock; 

        /// <summary>Были ли строки.</summary>
        private readonly bool _hasRows;

        /// <summary>Количество строк.</summary>
        private int RowsCount
        {
            get { return _hasRows ? 1 : 0; }
        }

        /// <summary>Конструктор.</summary>
        /// <param name="hasRows">Наличие строк в результате.</param>
        public EofStateTests(bool hasRows)
        {
            _hasRows = hasRows;
        }

        /// <summary>Инициализация данных.</summary>
        protected override void SetUpData()
        {
            _dataCursorMock = CreateDataCursorMock(RowsCount);
        }

        /// <summary>Сценарий для приведения тестового экземпляра в нужное состояние.</summary>
        protected override void ScenarioAfterInitTestedInstance()
        {
            for (var counter = 0; counter < RowsCount; counter++)
                Assert.IsTrue(TestedInstance.Read());

            Assert.IsFalse(TestedInstance.Read());
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.Close"/>.</summary>
        [Test]
        public override void TestClose()
        {
            // Arrange - Act - Assert
            base.TestClose();
            
            // Assert
            if (_hasRows)
                _dataCursorMock.VerifyDispose();
        }

        /// <summary>
        /// Тестирование метода <see cref="OneSDataReader.Read"/>.
        /// </summary>
        [Test]
        public void TestRead()
        {
            Assert.IsFalse(TestedInstance.Read());
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            // Act
            var result = TestedInstance.GetValues(new object[10]);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>.
        /// </summary>
        [Test]
        public void TestHasRows()
        {
            Assert.AreEqual(_hasRows, TestedInstance.HasRows);
            DataRecordsProviderMock.Verify(d => d.HasRecords, Times.AtLeastOnce());
        }
    }
}
