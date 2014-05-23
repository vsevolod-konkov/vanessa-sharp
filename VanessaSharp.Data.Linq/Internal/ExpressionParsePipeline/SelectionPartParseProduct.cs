using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline.SqlModel;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Часть результата парсинга выражения,
    /// отвечающая за выборку полей.
    /// </summary>
    internal sealed class SelectionPartParseProduct<T>
    {
        /// <summary>Конструктор.</summary>
        /// <param name="columns">Коллекция полей.</param>
        /// <param name="selectionFunc">Функция получения элемента.</param>
        public SelectionPartParseProduct(
            ReadOnlyCollection<SqlExpression> columns, Func<IValueConverter, object[], T> selectionFunc)
        {
            Contract.Requires<ArgumentNullException>(columns != null);
            Contract.Requires<ArgumentNullException>(selectionFunc != null);
            
            _columns = columns;
            _selectionFunc = selectionFunc;
        }

        /// <summary>Коллекция колонок.</summary>
        public ReadOnlyCollection<SqlExpression> Columns
        {
            get { return _columns; }
        }
        private readonly ReadOnlyCollection<SqlExpression> _columns;

        /// <summary>Функция получения элемента.</summary>
        public Func<IValueConverter, object[], T> SelectionFunc
        {
            get { return _selectionFunc; }
        }
        private readonly Func<IValueConverter, object[], T> _selectionFunc;
    }
}
