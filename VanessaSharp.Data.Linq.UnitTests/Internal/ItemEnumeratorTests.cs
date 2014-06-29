using System;
using System.Collections.ObjectModel;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>Тесты на <see cref="ItemEnumerator{T}"/>.</summary>
    [TestFixture]
    public sealed class ItemEnumeratorTests
    {
        private const int FIELDS_COUNT = 3;
        
        private Mock<ISqlResultReader> _sqlResultReaderMock;
        private RecordReaderMock _recordReaderMock;
        private Func<object[], OneSDataRecord> _itemReader;
        private ItemEnumerator<OneSDataRecord> _testedInstance;

        /// <summary>
        /// Инициализация тестов.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _sqlResultReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            _sqlResultReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(FIELDS_COUNT)
                .Verifiable();

            _recordReaderMock = new RecordReaderMock();
            _itemReader = _recordReaderMock.ReadRecord;
            _testedInstance = new ItemEnumerator<OneSDataRecord>(_sqlResultReaderMock.Object, _itemReader);
        }

        /// <summary>Тестирование инициализации.</summary>
        [Test]
        public void TestInit()
        {
            Assert.IsTrue(_testedInstance.IsSameSqlResultReader(_sqlResultReaderMock.Object));
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
            var buffer = new object[FIELDS_COUNT];

            _sqlResultReaderMock
                .Setup(r => r.Read())
                .Returns(true)
                .Verifiable();

            _sqlResultReaderMock
                .Setup(r => r.GetValues(It.IsAny<object[]>()))
                .Callback<object[]>(values =>
                    {
                        Assert.AreEqual(FIELDS_COUNT, values.Length);
                        Array.Copy(buffer, values, FIELDS_COUNT);
                    })
                .Verifiable();

            for (var counter = 1; counter <= recordsCount; counter++)
            {
                // Arrange reader
                _recordReaderMock.Reset();
                _recordReaderMock.ExpectedRecord = new OneSDataRecord(
                    new ReadOnlyCollection<string>(new string[0]), new ReadOnlyCollection<OneSValue>(new OneSValue[0]));

                for (var field = 0; field < FIELDS_COUNT; field++)
                    buffer[field] = new object();

                // Act
                var result = _testedInstance.MoveNext();

                // Assert
                Assert.IsTrue(result);
                Assert.AreSame(_recordReaderMock.ExpectedRecord, _testedInstance.Current);

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(counter));
                _sqlResultReaderMock.Verify(r => r.GetValues(It.IsAny<object[]>()), Times.Exactly(counter));
                Assert.AreEqual(1, _recordReaderMock.CallsCount);
                CollectionAssert.AreEqual(buffer, _recordReaderMock.ActualValues);    
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

            _sqlResultReaderMock
                .Setup(r => r.GetValues(It.IsAny<object[]>()))
                .Verifiable();

            _recordReaderMock.ExpectedRecord = new OneSDataRecord(
                new ReadOnlyCollection<string>(new string[0]),
                new ReadOnlyCollection<OneSValue>(new OneSValue[0]));

            for (var counter = 1; counter <= recordsCount; counter++)
            {
                // Act
                var result = _testedInstance.MoveNext();

                // Assert
                Assert.IsTrue(result);

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(counter));
                _sqlResultReaderMock.Verify(r => r.GetValues(It.IsAny<object[]>()), Times.Exactly(counter));
                Assert.AreEqual(counter, _recordReaderMock.CallsCount);
            }

            for (var readRepeating = 0; readRepeating < 2; readRepeating++)
            {
                Assert.IsFalse(_testedInstance.MoveNext());
                Assert.Throws<InvalidOperationException>(() => { var item = _testedInstance.Current; });

                _sqlResultReaderMock.Verify(r => r.Read(), Times.Exactly(recordsCount + 1));
                _sqlResultReaderMock.Verify(r => r.GetValues(It.IsAny<object[]>()), Times.Exactly(recordsCount));
                Assert.AreEqual(recordsCount, _recordReaderMock.CallsCount);
            }
        }

        /// <summary>Мок для делегата чтения записей.</summary>
        private class RecordReaderMock
        {
            public ReadOnlyCollection<object> ActualValues { get; private set; }

            public OneSDataRecord ExpectedRecord { get; set; }

            public int CallsCount { get; private set; }

            public void Reset()
            {
                CallsCount = 0;
                ActualValues = null;
                ExpectedRecord = null;
            }

            public OneSDataRecord ReadRecord(object[] values)
            {
                CallsCount++;

                ActualValues = new ReadOnlyCollection<object>(
                    (object[])values.Clone());

                return ExpectedRecord;
            }
        }
    }
}
