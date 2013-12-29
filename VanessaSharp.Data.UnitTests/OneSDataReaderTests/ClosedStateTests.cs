using System;
using Moq;
using NUnit.Framework;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.OneSDataReaderTests
{
    /// <summary>
    /// Тестирование <see cref="OneSDataReader"/>
    /// в случае когда экземпляр находится в закрытом состоянии.
    /// </summary>
    [TestFixture(Case.AfterBof)]
    [TestFixture(Case.AfterRow)]
    [TestFixture(Case.AfterEof)]
    public sealed class ClosedStateTests
    {
        public enum Case
        {
            AfterBof,
            AfterRow,
            AfterEof
        }

        /// <summary>Тестируемый экземпляр.</summary>
        private OneSDataReader _testedInstance;

        /// <summary>Сценарий закрытия.</summary>
        private readonly Case _case;

        public ClosedStateTests(Case @case)
        {
            _case = @case;
        }

        /// <summary>
        /// Установка реализации <see cref="IDisposable.Dispose"/>
        /// для мока.
        /// </summary>
        private static void SetupDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock
                .Setup(o => o.Dispose())
                .Verifiable();
        }

        /// <summary>Создание мока реализующего <see cref="IDisposable"/>.</summary>
        private static Mock<T> CreateDisposableMock<T>()
            where T : class, IDisposable
        {
            var mock = new Mock<T>(MockBehavior.Strict);
            SetupDispose(mock);

            return mock;
        }

        /// <summary>
        /// Проверка вызова <see cref="IDisposable.Dispose"/> 
        /// у мока.
        /// </summary>
        private static void VerifyDispose<T>(Mock<T> mock)
            where T : class, IDisposable
        {
            mock.Verify(o => o.Dispose(), Times.AtLeastOnce());
        }

        /// <summary>Инициализация теста.</summary>
        [SetUp]
        public void SetUp()
        {
            var queryResultMock = new Mock<IQueryResult>(MockBehavior.Strict);
            SetupDispose(queryResultMock);

            if (_case != Case.AfterBof)
            {
                var rowIndex = -1;
                const int ROWS_COUNT = 1;

                var queryResultSelectionMock = new Mock<IQueryResultSelection>(MockBehavior.Strict);
                queryResultSelectionMock
                    .Setup(s => s.Next())
                    .Returns(() =>
                    {
                        ++rowIndex;
                        return (rowIndex < ROWS_COUNT);
                    })
                    .Verifiable();
                SetupDispose(queryResultSelectionMock);

                queryResultMock
                    .Setup(r => r.IsEmpty())
                    .Returns(false);
                queryResultMock
                    .Setup(r => r.Choose())
                    .Returns(queryResultSelectionMock.Object);
            }

            _testedInstance = new OneSDataReader(queryResultMock.Object, new Mock<IValueTypeConverter>(MockBehavior.Strict).Object);

            if (_case != Case.AfterBof)
            {
                Assert.IsTrue(_testedInstance.Read());
            }

            if (_case == Case.AfterEof)
            {
                Assert.IsFalse(_testedInstance.Read());
            }

            _testedInstance.Close();
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.IsClosed"/>.
        /// </summary>
        [Test]
        public void TestIsClosed()
        {
            Assert.IsTrue(_testedInstance.IsClosed);
        }

        /// <summary>Тестирование метода <see cref="OneSDataReader.FieldCount"/>.</summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestFieldCount()
        {
            var result = _testedInstance.FieldCount;
        }

        /// <summary>Тестирование <see cref="OneSDataReader.GetName"/>.</summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetName()
        {
            var result = _testedInstance.GetName(3);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetFieldType"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetFieldType()
        {
            var actualType = _testedInstance.GetFieldType(3);
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.Read"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestRead()
        {
            var actualType = _testedInstance.Read();
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataReader.GetValues"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestGetValues()
        {
            var actualResult = _testedInstance.GetValues(new object[10]);
        }

        /// <summary>Тестирование свойства <see cref="OneSDataReader.Depth"/>.</summary>
        [Test]
        public void TestDepth()
        {
            Assert.AreEqual(0, _testedInstance.Depth);
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.HasRows"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestHasRows()
        {
            var result = _testedInstance.HasRows;
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(int)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByIndex()
        {
            var result = _testedInstance[5];
        }

        /// <summary>
        /// Тестирование свойства <see cref="OneSDataReader.Item(string)"/>.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestItemByName()
        {
            var result = _testedInstance["TEST_FIELD"];
        }

        /// <summary>Тестирование <see cref="OneSDataReader.RecordsAffected"/>.</summary>
        [Test]
        public void TestRecordsAffected()
        {
            Assert.AreEqual(-1, _testedInstance.RecordsAffected);
        }
    }
}
