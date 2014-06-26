using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Выражение сортировки.</summary>
    internal sealed class SortExpression
    {
        /// <summary>Конструктор.</summary>
        /// <param name="keyExpression">Выражение получения ключа сортировки.</param>
        /// <param name="kind">Тип сортировки.</param>
        public SortExpression(LambdaExpression keyExpression, SortKind kind)
        {
            Contract.Requires<ArgumentNullException>(keyExpression != null);
            Contract.Requires<ArgumentException>(keyExpression.Type.GetGenericTypeDefinition() == typeof(Func<,>));

            _keyExpression = keyExpression;
            _kind = kind;
        }

        /// <summary>Выражение получения ключа сортировки.</summary>
        public LambdaExpression KeyExpression
        {
            get { return _keyExpression; }
        }
        private readonly LambdaExpression _keyExpression;

        /// <summary>Тип сортировки.</summary>
        public SortKind Kind
        {
            get { return _kind; }
        }
        private readonly SortKind _kind;
    }
}
