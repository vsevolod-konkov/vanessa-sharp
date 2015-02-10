using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="DataReaderFieldInfoCollectionLoader"/>.
    /// </summary>
    [TestFixture]
    public sealed class DataReaderFieldInfoCollectionLoaderTests
    {
        private List<IDisposableMock> _disposableMocks;
        private IQueryResult _queryResult;
        private ITypeDescriptionConverter _typeDescriptionConverter;
        private IRawValueConverterProvider _rawValueConverterProvider;
        
        private static object ConvertRawValue(object obj)
        {
            return obj;
        }

        private void InitTest(IList<DataReaderFieldInfo> expectedFields)
        {
            _disposableMocks = new List<IDisposableMock>();

            var rawValueConverterProviderMock = new Mock<IRawValueConverterProvider>(MockBehavior.Strict);
            rawValueConverterProviderMock
                .Setup(c => c.GetRawValueConverter(It.IsAny<Type>()))
                .Returns(ConvertRawValue);
            _rawValueConverterProvider = rawValueConverterProviderMock.Object;

            var typeDescriptionMocks = Enumerable
                .Range(0, expectedFields.Count)
                .Select(i => new DisposableMock<ITypeDescription>())
                .ToArray();

            _disposableMocks.AddRange(typeDescriptionMocks);

            var typeConverterMock = new Mock<ITypeDescriptionConverter>(MockBehavior.Strict);
            for (var index = 0; index < expectedFields.Count; index++)
            {
                var typeDescription = typeDescriptionMocks[index].Object;

                typeConverterMock
                    .Setup(c => c.ConvertFrom(typeDescription))
                    .Returns(expectedFields[index].Type);
            }

            var columnMocks = Enumerable
                .Range(0, expectedFields.Count)
                .Select(i => new DisposableMock<IQueryResultColumn>())
                .ToArray();

            _disposableMocks.AddRange(columnMocks);

            for (var index = 0; index < expectedFields.Count; index++)
            {
                columnMocks[index]
                    .SetupGet(c => c.Name)
                    .Returns(expectedFields[index].Name);

                columnMocks[index]
                    .SetupGet(c => c.ValueType)
                    .Returns(typeDescriptionMocks[index].Object);
            }

            var columnsCollectionMock = new DisposableMock<IQueryResultColumnsCollection>();
            _disposableMocks.Add(columnsCollectionMock);

            columnsCollectionMock
                .SetupGet(cs => cs.Count)
                .Returns(expectedFields.Count);
            columnsCollectionMock
                .Setup(cs => cs.Get(It.IsAny<int>()))
                .Returns<int>(i => columnMocks[i].Object);

            var queryResultMock = new DisposableMock<IQueryResult>();
            queryResultMock
                .SetupGet(q => q.Columns)
                .Returns(columnsCollectionMock.Object);

            _typeDescriptionConverter = typeConverterMock.Object;
            _queryResult = queryResultMock.Object;
        }

        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoCollectionLoader.Load"/>.
        /// </summary>
        [Test]
        public void TestLoad()
        {
            // Arrange
            var expectedFields = new[]
                {
                    new DataReaderFieldInfo("Id", typeof(int), ConvertRawValue),
                    new DataReaderFieldInfo("Date", typeof(DateTime), ConvertRawValue)
                };

            InitTest(expectedFields);

            // Act
            var actualResult = DataReaderFieldInfoCollectionLoader.Load(
                _queryResult, _typeDescriptionConverter, _rawValueConverterProvider);

            // Assert
            Assert.AreEqual(expectedFields.Length, actualResult.Count);

            for (var index = 0; index < expectedFields.Length; index++)
            {
                var field = actualResult[index];
                Assert.AreEqual(expectedFields[index].Name, field.Name);
                Assert.AreEqual(expectedFields[index].Type, field.Type);
                Assert.AreEqual(expectedFields[index].RawValueConverter, (Func<object, object>)ConvertRawValue);
            }

            foreach (var mock in _disposableMocks)
                mock.VerifyDispose();
        }

        /// <summary>
        /// Тестирование <see cref="DataReaderFieldInfoCollectionLoader.Create"/>.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            // Arrange
            var expectedFields = new[]
                {
                    new DataReaderFieldInfo("Id", typeof(int), ConvertRawValue),
                    new DataReaderFieldInfo("Date", typeof(DateTime), ConvertRawValue)
                };

            InitTest(expectedFields);

            // Act
            var actualResult = DataReaderFieldInfoCollectionLoader.Create(
                _queryResult, _typeDescriptionConverter, _rawValueConverterProvider);

            // Assert
            Assert.IsInstanceOf<LazyDataReaderFieldInfoCollection>(actualResult);

            Assert.AreEqual(expectedFields.Length, actualResult.Count);

            for (var index = 0; index < expectedFields.Length; index++)
            {
                var field = actualResult[index];
                Assert.AreEqual(expectedFields[index].Name, field.Name);
                Assert.AreEqual(expectedFields[index].Type, field.Type);
                Assert.AreEqual(expectedFields[index].RawValueConverter, (Func<object, object>)ConvertRawValue);
            }
        }
    }
}
