using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Moq;
using NUnit.Framework;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// Тестирование <see cref="OrderByExpressionTransformer"/>.
    /// </summary>
    [TestFixture]
    public sealed class OrderByExpressionTransformerTests
    {
        private readonly Mock<IOneSMappingProvider> _mappingProviderMock = new Mock<IOneSMappingProvider>(MockBehavior.Strict);
        
        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля записи методом <see cref="OneSDataRecord.GetInt32(string)"/>.
        /// </summary>
        [Test]
        public void TestTransformDataRecordGetInt32()
        {
            const string FIELD_NAME = "sort_field";

            // Arrange
            Expression<Func<OneSDataRecord, int>> sortKeyExpression = r => r.GetInt32(FIELD_NAME);

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            Assert.AreEqual(FIELD_NAME, result.FieldName);
        }

        /// <summary>
        /// Тестирование <see cref="OrderByExpressionTransformer.Transform"/>
        /// в случае если передано выражение получения значения поля типизированного кортежа.
        /// </summary>
        [Test]
        public void TestTransfromTypedTupleField()
        {
            // Arrange
            const string FIELD_NAME = "id_field";
            _mappingProviderMock
                .Setup(p => p.GetTypeMapping(typeof (AnyData)))
                .Returns(new OneSTypeMapping("X", new ReadOnlyCollection<OneSFieldMapping>(new[]
                    {
                        CreateFieldMapping(d => d.Id, FIELD_NAME)
                    })));
            
            Expression<Func<AnyData, int>> sortKeyExpression = d => d.Id;

            // Act
            var result = OrderByExpressionTransformer.Transform(_mappingProviderMock.Object, new QueryParseContext(), sortKeyExpression);

            // Assert
            Assert.AreEqual(FIELD_NAME, result.FieldName);
        }

        public sealed class AnyData
        {
            public int Id;
        }

        // TODO Копипаста
        private static OneSFieldMapping CreateFieldMapping<T>(Expression<Func<AnyData, T>> accessor, string fieldName)
        {
            var memberInfo = ((MemberExpression)accessor.Body).Member;

            return new OneSFieldMapping(memberInfo, fieldName);
        }
    }
}
