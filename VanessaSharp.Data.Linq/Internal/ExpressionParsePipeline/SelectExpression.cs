using System;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Выражение выборки.</summary>
    /// <typeparam name="TInput">Тип элементов входной последовательности.</typeparam>
    /// <typeparam name="TOutput">Тип элементов выходной последовательности.</typeparam>
    internal sealed class SelectExpression<TInput, TOutput>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="expression">Выражение выборки.</param>
        /// <param name="isDistinct">Выборка различных.</param>
        public SelectExpression(Expression<Func<TInput, TOutput>> expression, bool isDistinct)
        {
            Contract.Requires<ArgumentException>(
                    (typeof(TInput) == typeof(TOutput) && expression == null)
                    ||
                    (typeof(TInput) == typeof(TOutput) || expression != null));
            
            _expression = expression;
            _isDistinct = isDistinct;
        }

        /// <summary>Выражение выборки.</summary>
        public Expression<Func<TInput, TOutput>> Expression
        {
            get { return _expression; }
        }
        private readonly Expression<Func<TInput, TOutput>> _expression;

        /// <summary>Выборка различных.</summary>
        public bool IsDistinct
        {
            get { return _isDistinct; }
        }
        private readonly bool _isDistinct;
    }
}
