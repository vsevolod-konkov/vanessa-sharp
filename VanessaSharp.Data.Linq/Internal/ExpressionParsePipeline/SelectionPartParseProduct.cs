using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Часть результата парсинга выражения,
    /// отвечающая за выборку полей.
    /// </summary>
    internal sealed class SelectionPartParseProduct<T>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="fields">Коллекция полей.</param>
        /// <param name="selectionFunc">Функция получения элемента.</param>
        public SelectionPartParseProduct(
            ReadOnlyCollection<string> fields, Func<IValueConverter, object[], T> selectionFunc)
        {
            Contract.Requires<ArgumentNullException>(fields != null);
            Contract.Requires<ArgumentNullException>(selectionFunc != null);
            
            _fields = fields;
            _selectionFunc = selectionFunc;
        }

        /// <summary>Коллекция полей.</summary>
        public ReadOnlyCollection<string> Fields
        {
            get { return _fields; }
        }
        private readonly ReadOnlyCollection<string> _fields;

        /// <summary>Функция получения элемента.</summary>
        public Func<IValueConverter, object[], T> SelectionFunc
        {
            get { return _selectionFunc; }
        }
        private readonly Func<IValueConverter, object[], T> _selectionFunc;
    }
}
