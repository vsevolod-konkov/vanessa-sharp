using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>Тесты на <see cref="ItemEnumerator{T}"/>.</summary>
    [TestFixture]
    public sealed class ItemEnumeratorTests
    {
        private Mock<ISqlResultReader> _sqlResultReaderMock;
        private RecordReaderMock _recordReaderMock;
        private Func<ISqlResultReader, OneSDataRecord> _itemReader;
        private ItemEnumerator<OneSDataRecord> _testedInstance;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            _recordReaderMock = new RecordReaderMock();
            _itemReader = _recordReaderMock.ReadRecord;
            _testedInstance = new ItemEnumerator<OneSDataRecord>(_sqlResultReaderMock.Object, _itemReader);
        }

        /// <summary>Тестирование инициализации.</summary>
        [Test]
        public void TestInit()
        {
            Assert.AreSame(_sqlResultReaderMock.Object, _testedInstance.SqlReader);
            Assert.AreSame(_itemReader, _testedInstance.ItemReader);

            Assert.Throws<InvalidOperationException>(() => { var item = _testedInstance.Current; });
        }

        /// <summary>
        /// Тестирование <see cref="ItemEnumerator{T}.Dispose"/>.
        /// </summary>
        [Test]
        public void TestDispose()
        {
            // Arrange
            _sqlResultReaderMock
                .Setup(r => r.Dispose())
                .Verifiable();

            // Act
            _testedInstance.Dispose();

            // Assert
            _sqlResultReaderMock.Verify(r => r.Dispose(), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="ItemEnumerator{T}.MoveNext"/>
        /// в случае если нет записей.
        /// </summary>
        [Test]
        public void TestMoveNextWhenEmpty()
        {
            // Arrange
            _sqlResultReaderMock
                .Setup(r => r.Read())
                .Returns(false)
                .Verifiable();

            // Act
            var result = _testedInstance.MoveNext();
            Assert.Throws<InvalidOperationException>(() => { var item = _testedInstance.Current; });
            
            // Assert
            Assert.IsFalse(result);
            _sqlResultReaderMock.Verify(r => r.Read(), Times.Once());
        }

        /// <summary>
        /// Тестирование <see cref="ItemEnumerator{T}.MoveNext"/>
        /// в случае когда записи имеются.
        /// </summary>
        /// <param name="recordsCount">Количество записей.</param>
        [Test]
        public void TestMoveNextWhenNotEmpty([Values(1, 5)] int recordsCount)
        {
            // Arrange
            _sqlResultReaderMock
                .Setup(r => r.Read())
                .Returns(true)
                .Verifiable();

            for (var counter = 1; counter <= recordsCount; counter++)
            {
                // Arrange reader
                _recordReaderMock.Reset();
                _recordReaderMock.ExpectedRecord = new OneSDataRecord();

                // Act
                var result = _testedInstance.MoveNext();

                // Assert
                Assert.IsTrue(result);
                Assert.AreSame(_recordReaderMock.ExpectedRecord, _testedInstance.Current);

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(counter));
                Assert.AreEqual(1, _recordReaderMock.CallsCount);
                Assert.AreSame(_sqlResultReaderMock.Object, _recordReaderMock.ActualSqlResultReader);    
            }
        }

        /// <summary>
        /// Тестирование <see cref="ItemEnumerator{T}.MoveNext"/>
        /// в случае достижения конца записей.
        /// </summary>
        /// <param name="recordsCount">Количество записей.</param>
        [Test]
        public void TestMoveNextWhenEof([Values(0, 1, 5)] int recordsCount)
        {
            // Счетчик записей
            int recordsCounter = 0;

            // Arrange
            _sqlResultReaderMock
                .Setup(r => r.Read())
                .Returns(() => recordsCounter++ < recordsCount)
                .Verifiable();

            _recordReaderMock.ExpectedRecord = new OneSDataRecord();

            for (var counter = 1; counter <= recordsCount; counter++)
            {
                // Act
                var result = _testedInstance.MoveNext();

                // Assert
                Assert.IsTrue(result);

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(counter));
                Assert.AreEqual(counter, _recordReaderMock.CallsCount);
            }

            for (var readRepeating = 0; readRepeating < 2; readRepeating++)
            {
                Assert.IsFalse(_testedInstance.MoveNext());
                Assert.Throws<InvalidOperationException>(() => { var item = _testedInstance.Current; });

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(recordsCount + 1));
                Assert.AreEqual(recordsCount, _recordReaderMock.CallsCount);
            }
        }

        /// <summary>Мок для делегата чтения записей.</summary>
        private class RecordReaderMock
        {
            public ISqlResultReader ActualSqlResultReader { get; private set; }

            public OneSDataRecord ExpectedRecord { get; set; }

            public int CallsCount { get; private set; }

            public void Reset()
            {
                CallsCount = 0;
                ActualSqlResultReader = null;
                ExpectedRecord = null;
            }

            public OneSDataRecord ReadRecord(ISqlResultReader resultReader)
            {
                CallsCount++;

                ActualSqlResultReader = resultReader;

                return ExpectedRecord;
            }
        }
    }
}
