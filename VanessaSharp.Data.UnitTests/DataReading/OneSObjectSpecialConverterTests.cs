﻿using System;
using NUnit.Framework;
using VanessaSharp.Data.DataReading;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.UnitTests.DataReading
{
    /// <summary>
    /// Тестирование <see cref="OneSObjectSpecialConverter"/>
    /// </summary>
    [TestFixture]
    public sealed class OneSObjectSpecialConverterTests
    {
        /// <summary>
        /// Тестирование <see cref="OneSObjectSpecialConverter.ToDataReader"/>
        /// в случае, если входной объект не поддерживает интерфейс <see cref="IQueryResult"/>
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestToDataReaderWhenObjectIsNotIQueryResult()
        {
            var obj = new object();

            var result = OneSObjectSpecialConverter.Default.ToDataReader(obj);
        }

        /// <summary>
        /// Тестирование <see cref="OneSObjectSpecialConverter.ToDataReader"/>
        /// в случае, если входной объект поддерживает интерфейс <see cref="IQueryResult"/>
        /// </summary>
        [Test]
        public void TestToDataReaderWhenObjectIsIQueryResult()
        {
            // Arrange
            var queryResult = new DisposableMock<IQueryResult>().Object;

            // Act
            var result = OneSObjectSpecialConverter.Default.ToDataReader(queryResult);

            // Assert
            Assert.IsTrue(result.IsTablePart);

            Assert.IsInstanceOf<QueryResultDataRecordsProvider>(result.DataRecordsProvider);
            var dataRecordsProvider = (QueryResultDataRecordsProvider)result.DataRecordsProvider;

            Assert.AreSame(queryResult, dataRecordsProvider.QueryResult);
            Assert.AreEqual(QueryResultIteration.Default, dataRecordsProvider.QueryResultIteration);
        }
    }
}
