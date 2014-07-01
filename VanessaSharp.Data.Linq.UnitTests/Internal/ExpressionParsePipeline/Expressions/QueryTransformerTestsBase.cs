using System;
using System.Linq.Expressions;
using Moq;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests.Internal.ExpressionParsePipeline.Expressions
{
    /// <summary>
    /// ������� ����� ��� ������������ <see cref="QueryTransformer"/>.
    /// </summary>
    public abstract class QueryTransformerTestsBase
    {
        /// <summary>�������� ������� �������.</summary>
        /// <typeparam name="TInput">
        /// ��� ��������� ������� ������������������.
        /// </typeparam>
        /// <typeparam name="TOutput">
        /// ��� ��������� �������� ������������������.
        /// </typeparam>
        /// <param name="source">�������� ���������.</param>
        /// <param name="selector">��������� �������.</param>
        /// <param name="filter">��������� ����������.</param>
        /// <param name="sorters">��������� ����������.</param>
        internal static IQuery<TInput, TOutput> CreateQuery<TInput, TOutput>(
            ISourceDescription source,
            Expression<Func<TInput, TOutput>> selector = null,
            Expression<Func<TInput, bool>> filter = null,
            params SortExpression[] sorters)
        {
            var queryMock = new Mock<IQuery<TInput, TOutput>>(MockBehavior.Strict);
            queryMock
                .SetupGet(q => q.Source)
                .Returns(source);
            queryMock
                .SetupGet(q => q.Selector)
                .Returns(selector);
            queryMock
                .SetupGet(q => q.Filter)
                .Returns(filter);
            queryMock
                .SetupGet(q => q.Sorters)
                .Returns(sorters.ToReadOnly());

            return queryMock.Object;
        }

        /// <summary>
        /// �������� �������������� ������.
        /// </summary>
        public sealed class SomeData
        {
            public int Id;

            public string Name;

            public decimal Price;
        }
    }
}