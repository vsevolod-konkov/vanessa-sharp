using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal
{
    /// <summary>
    /// Тестирование <see cref="OneSDataRecordReaderFactory"/>.
    /// </summary>
    [TestFixture]
    public sealed class OneSDataRecordReaderFactoryTests
    {
        private static readonly IValueConverter _valueConverter = new Mock<IValueConverter>(MockBehavior.Strict).Object;
        
        private static Mock<ISqlResultReader> CreateISqlResultReaderMock(IList<string> fields)
        {
            var sqlReaderMock = new Mock<ISqlResultReader>(MockBehavior.Strict);
            sqlReaderMock
                .SetupGet(r => r.ValueConverter)
                .Returns(_valueConverter);
            sqlReaderMock
                .SetupGet(r => r.FieldCount)
                .Returns(fields.Count);
            sqlReaderMock
                .Setup(r => r.GetFieldName(It.IsAny<int>()))
                .Returns<int>(i => fields[i]);

            return sqlReaderMock;
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecordReaderFactory.CreateItemReader"/>.
        /// </summary>
        [Test]
        public void TestCreateItemReader()
        {
            // Arrange
            var expectedFields = new[]
                {
                    "ID", "Name", "Value"
                };

            var sqlReaderMock = CreateISqlResultReaderMock(expectedFields);

            var rawValues = Enumerable
                .Range(0, expectedFields.Length)
                .Select(i => new object())
                .ToArray();

            // Act
            var result = OneSDataRecordReaderFactory.Default.CreateItemReader(sqlReaderMock.Object);
            var record = result(rawValues);

            // Assert
            CollectionAssert.AreEqual(expectedFields, record.Fields);

            for (var index = 0; index < expectedFields.Length; index++)
            {
                var oneSValue = record.GetValue(index);
                Assert.AreSame(_valueConverter, oneSValue.ValueConverter);
                Assert.AreSame(rawValues[index], oneSValue.RawValue);
            }

            sqlReaderMock
                .VerifyGet(r => r.ValueConverter, Times.Once());
            sqlReaderMock
                .VerifyGet(r => r.FieldCount, Times.AtLeastOnce());
            sqlReaderMock
                .Verify(r => r.GetFieldName(It.IsAny<int>()), Times.Exactly(expectedFields.Length));
        }

        /// <summary>
        /// Тестирование <see cref="OneSDataRecordReaderFactory.CreateItemReader"/>
        /// в случае получения табличной части.
        /// </summary>
        [Test]
        public void TestCreateItemReaderWhenValueIsTablePartReader()
        {
            // Arrange
            var expectedFields = new[]
                {
                    "TablePart"
                };

            var sqlReaderMock = CreateISqlResultReaderMock(expectedFields);
            var tablePartReader = new Mock<ISqlResultReader>(MockBehavior.Strict).Object;

            // Act
            var result = OneSDataRecordReaderFactory.Default.CreateItemReader(sqlReaderMock.Object);
            var record = result(new object[] { tablePartReader });

            // Assert
            CollectionAssert.AreEqual(expectedFields, record.Fields);

            var recordsEnumerable = AssertEx.IsInstanceAndCastOf<ItemEnumerable<OneSDataRecord>>(record.GetTablePartRecords(0));

            Assert.AreSame(tablePartReader, recordsEnumerable.SqlResultReader);
            Assert.AreSame(OneSDataRecordReaderFactory.Default, recordsEnumerable.ItemReaderFactory);
        }
    }
}
