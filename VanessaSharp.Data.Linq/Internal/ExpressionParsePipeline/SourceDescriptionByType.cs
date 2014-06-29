using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.Expressions;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>Описание источника данных через тип.</summary>
    /// <typeparam name="T">Тип данных.</typeparam>
    internal sealed class SourceDescriptionByType<T> : ISourceDescription
    {
        /// <summary>Закрытие конструктора.</summary>
        private SourceDescriptionByType()
        {
            Contract.Requires<ArgumentException>(typeof(T) != typeof(OneSDataRecord));
        }

        /// <summary>
        /// Единственный экземпляр.
        /// </summary>
        public static SourceDescriptionByType<T> Instance
        {
            get { return _instance; }
        }
        private static readonly SourceDescriptionByType<T> _instance = new SourceDescriptionByType<T>(); 

        /// <summary>Получение имени источника данных.</summary>
        /// <param name="sourceResolver">Ресолвер имен источников данных.</param>
        public string GetSourceName(ISourceResolver sourceResolver)
        {
            return sourceResolver.ResolveSourceNameForTypedRecord<T>();
        }
    }
}
