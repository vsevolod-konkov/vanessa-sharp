using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="DataCursor"/>.
    /// </summary>
    [TestFixture]
    public sealed class DataCursorTests
    {
        private Mock<IDataReaderFieldInfoCollection> _fieldInfoCollectionMock;

        private Mock<IQueryResultSelection> _queryResultSelectionMock;

        private Mock<IOneSObjectSpecialConverter> _oneSObjectSpecialConverterMock;

        private DataCursor _testedInstance;

        private void InitTestedInstance(
            int fieldsCount = 1,
            Action<Mock<IDataReaderFieldInfoCollection>> dataReaderFieldInfoSetup = null,
            Mock<IQueryResultSelection> queryResultSelectionMock = null,
            Action<Mock<IOneSObjectSpecialConverter>> oneSObjectSpecialConverterSetup = null)
        {
            _fieldInfoCollectionMock = new Mock<IDataReaderFieldInfoCollection>(MockBehavior.Strict);
            _fieldInfoCollectionMock
                .SetupGet(c => c.Count)
                .Returns(fieldsCount);

            for (var index = 0; index < fieldsCount; index++)
            {
                var ordinal = index;
                _fieldInfoCollectionMock
                    .Setup(c => c[ordinal])
                    .Returns(
                        new DataReaderFieldInfo("Field_" + ordinal, typeof(object))
                    );
            }

            if (dataReaderFieldInfoSetup != null)
                dataReaderFieldInfoSetup(_fieldInfoCollectionMock);

            _queryResultSelectionMock = queryResultSelectionMock ?? new Mock<IQueryResultSelection>(MockBehavior.Strict);

            _oneSObjectSpecialConverterMock = new Mock<IOneSObjectSpecialConverter>(MockBehavior.Strict);

            if (oneSObjectSpecialConverterSetup != null)
                oneSObjectSpecialConverterSetup(_oneSObjectSpecialConverterMock);

            _testedInstance = new DataCursor(
                _fieldInfoCollectionMock.Object,
                _queryResultSelectionMock.Object,
                _oneSObjectSpecialConverterMock.Object);
        }
        
        /// <summary>Тестирование метода <see cref="DataCursor.Next"/>.</summary>
        /// <param name="expectedResult">Ожидаемый результат.</param>
        [Test]
        public void TestNext([Values(true, false)] bool expectedResult)
        {
            // Arrange
            InitTestedInstance();
            
            _queryResultSelectionMock
                .Setup(qrs => qrs.Next())
                .Returns(expectedResult);

            // Act
            var actualResult = _testedInstance.Next();

            // Assert
            Assert.AreEqual(expectedResult, actualResult);

            _queryResultSelectionMock
                .Verify(qrs => qrs.Next(), Times.Once());
        }

        /// <summary>
        /// Тестирование метода <see cref="DataCursor.Dispose"/>
        /// </summary>
        [Test]
        public void TestDispose()
        {
            // Arrange
            var queryResultSelectionMock = new DisposableMock<IQueryResultSelection>();

            InitTestedInstance(queryResultSelectionMock: queryResultSelectionMock);

            // Act
            _testedInstance.Dispose();

            // Assert
            queryResultSelectionMock.VerifyDispose();
        }

        /// <summary>
        /// Тестирование метода <see cref="DataCursor.GetValue(int)"/>
        /// </summary>
        [Test]
        public void TestGetValueByIndex([Values(1, 4)] int callsCount, [Values(1, 3)] int rowsCount)
        {
            // Arrange
            const int TEST_ORDINAL = 5;
            var expectedResults = Enumerable
                .Range(0, rowsCount)
                .Select(i => "Data" + i)
                .ToArray();

            InitTestedInstance(TEST_ORDINAL + 3);

            _queryResultSelectionMock
                .Setup(qrs => qrs.Next())
                .Returns(true);

            for (var rowsCounter = 0; rowsCounter < rowsCount; rowsCounter++)
            {
                var expectedResult = expectedResults[rowsCounter];

                _queryResultSelectionMock
                    .Setup(qrs => qrs.Get(TEST_ORDINAL))
                    .Returns(expectedResult);

                // Act
                var actualResults = new object[callsCount];
                for (var counter = 0; counter < callsCount; counter++)
                    actualResults[counter] = _testedInstance.GetValue(TEST_ORDINAL);

                // Assert
                foreach (var actualResult in actualResults)
                    Assert.AreEqual(expectedResult, actualResult);

                // Act Next Row
                _testedInstance.Next();
            }

            _queryResultSelectionMock
                .Verify(qrs => qrs.Get(TEST_ORDINAL), Times.Exactly(rowsCount));
        }

        /// <summary>
        /// Тестирование в случае если, индекс колонки выходит за допустимые значения.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestGetValueByIndexWhenOrdinalOutOfRange([Values(-3, 2)] int ordinal)
        {
            // Arrange
            var fieldsCount = ordinal - 1;
            fieldsCount = (fieldsCount > 0) ? fieldsCount : 1; 

            InitTestedInstance(fieldsCount);

            // Act
            _testedInstance.GetValue(ordinal);
        }

        /// <summary>
        /// Тестирование метода <see cref="DataCursor.GetValue(string)"/>
        /// </summary>
        [Test]
        public void TestGetValueByName([Values(1, 4)] int callsCount, [Values(1, 3)] int rowsCount)
        {
            // Arrange
            const string TEST_FIELD_NAME = "TestField";
            const int TEST_ORDINAL = 2;
            var expectedResults = Enumerable
                .Range(0, rowsCount)
                .Select(i => "Data" + i)
                .ToArray();

            InitTestedInstance(TEST_ORDINAL + 3);

            _fieldInfoCollectionMock
                .Setup(c => c.IndexOf(TEST_FIELD_NAME))
                .Returns(TEST_ORDINAL);

            _queryResultSelectionMock
                .Setup(qrs => qrs.Next())
                .Returns(true);

            for (var rowsCounter = 0; rowsCounter < rowsCount; rowsCounter++)
            {
                var expectedResult = expectedResults[rowsCounter];

                _queryResultSelectionMock
                    .Setup(qrs => qrs.Get(TEST_ORDINAL))
                    .Returns(expectedResult);

                // Act
                var actualResults = new object[callsCount];
                for (var counter = 0; counter < callsCount; counter++)
                    actualResults[counter] = _testedInstance.GetValue(TEST_FIELD_NAME);

                // Assert
                foreach (var actualResult in actualResults)
                    Assert.AreEqual(expectedResult, actualResult);

                // Act Next Row
                _testedInstance.Next();
            }

            _queryResultSelectionMock
                .Verify(qrs => qrs.Get(TEST_ORDINAL), Times.Exactly(rowsCount));
        }

        /// <summary>
        /// Тестирование в случае если, индекс колонки выходит за допустимые значения.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void TestGetValueByNameWhenFieldIsNotExist()
        {
            // Arrange
            const string FIELD_NAME = "NotExisting";

            InitTestedInstance();

            _fieldInfoCollectionMock
                .Setup(c => c.IndexOf(FIELD_NAME))
                .Returns(-1);

            // Act
            _testedInstance.GetValue(FIELD_NAME);
        }

        /// <summary>
        /// Тестирование получения экземпляра <see cref="OneSDataReader"/>,
        /// в случае если типом поля является читатель.
        /// </summary>
        [Test]
        public void TestGetOneSDataReaderWhenFieldTypeIsOneSDataReader()
        {
            const int FIELD_INDEX = 0;

            var value = new object();

            var dataReader = new OneSDataReader(
                new Mock<IDataRecordsProvider>(MockBehavior.Strict).Object,
                new Mock<IValueConverter>(MockBehavior.Strict).Object,
                true);

            InitTestedInstance(
                dataReaderFieldInfoSetup: 
                    m => m.Setup(c => c[FIELD_INDEX])
                          .Returns(new DataReaderFieldInfo("TablePart", typeof(OneSDataReader))),
                oneSObjectSpecialConverterSetup:
                    m => m.Setup(c => c.ToDataReader(value))
                          .Returns(dataReader)
                );

            _queryResultSelectionMock
                .Setup(qrs => qrs.Next())
                .Returns(true);

            _queryResultSelectionMock
                .Setup(qrs => qrs.Get(FIELD_INDEX))
                .Returns(value);

            _testedInstance.Next();

            var result = _testedInstance.GetValue(FIELD_INDEX);

            Assert.AreSame(dataReader, result);
        }

        /// <summary>
        /// Тестирование получения значения свойства,
        /// которое имеет прямое соответствие свойству <see cref="IQueryResultSelection"/>.
        /// </summary>
        private void TestDirectProperty<T>(Func<DataCursor, T> testedProperty,
                                           Expression<Func<IQueryResultSelection, T>> propertyAccessor,
                                           T expectedValue)
        {
            // Arrange
            InitTestedInstance();

            _queryResultSelectionMock
                .Setup(propertyAccessor)
                .Returns(expectedValue);

            // Act
            var actualValue = testedProperty(_testedInstance);

            // Assert
            Assert.AreEqual(expectedValue, actualValue);

            _queryResultSelectionMock
                .Verify(propertyAccessor, Times.Once());
        }

        /// <summary>
        /// Тестирование получения значения <see cref="DataCursor.Level"/>.
        /// </summary>
        [Test]
        public void TestLevel()
        {
            const int EXPECTED_LEVEL = 3;

            TestDirectProperty(
                i => i.Level,
                s => s.Level,
                EXPECTED_LEVEL);
        }

        /// <summary>
        /// Тестирование получения значения <see cref="DataCursor.GroupName"/>.
        /// </summary>
        [Test]
        public void TestGroupName()
        {
            const string EXPECTED_GROUP_NAME = "TestGroup";

            TestDirectProperty(
                i => i.GroupName,
                s => s.Group,
                EXPECTED_GROUP_NAME);
        }

        /// <summary>
        /// Тестирование получения значения <see cref="DataCursor.RecordType"/>.
        /// </summary>
        [Test]
        public void TestRecordType()
        {
            TestDirectProperty(
                i => i.RecordType,
                s => s.RecordType,
                SelectRecordType.GroupTotal);
        }
    }
}
