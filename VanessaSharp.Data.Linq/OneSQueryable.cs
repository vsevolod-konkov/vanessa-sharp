using System;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Базовый класс для реализации <see cref="IQueryable"/>
    /// для доступа к данным 1С.
    /// </summary>
    internal abstract class OneSQueryable : IQueryable
    {
        /// <summary>>Поставщик запросов к 1С.</summary>
        private readonly IOneSQueryProvider _provider;

        /// <summary>LINQ-выражение запроса.</summary>
        private readonly Expression _expression;

        /// <summary>
        /// Конструктор, принимающий поставщика запросов и выражение.
        /// </summary>
        /// <param name="provider">Поставщик запросов к 1С.</param>
        /// <param name="expression">LINQ-выражение запроса.</param>
        protected OneSQueryable(IOneSQueryProvider provider, Expression expression)
        {
            Contract.Requires<ArgumentNullException>(provider != null);
            Contract.Requires<ArgumentNullException>(expression != null);
            
            _provider = provider;
            _expression = expression;
        }

        [ContractInvariantMethod]
        private void Invariant()
        {
            Contract.Invariant(_provider != null);
            Contract.Invariant(_expression != null);
        }

        /// <summary>
        /// Получает выражение, связанное с экземпляром класса <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// Выражение <see cref="T:System.Linq.Expressions.Expression"/>, связанное с данным экземпляром класса <see cref="T:System.Linq.IQueryable"/>.
        /// </returns>
        public Expression Expression
        {
            get { return _expression; }
        }

        /// <summary>
        /// Возвращает объект поставщика запросов, связанного с указанным источником данных.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Linq.IQueryProvider"/>, связанный с указанным источником данных.
        /// </returns>
        public IOneSQueryProvider Provider
        {
            get { return _provider; }
        }

        /// <summary>
        /// Возвращает объект поставщика запросов, связанного с указанным источником данных.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Linq.IQueryProvider"/>, связанный с указанным источником данных.
        /// </returns>
        IQueryProvider IQueryable.Provider
        {
            get { return Provider; }
        }

        // TODO: Реализовать вывод из выражения
        /// <summary>
        /// Получает тип элементов, которые возвращаются при выполнении дерева выражения, связанного с данным экземпляром класса <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// Тип <see cref="T:System.Type"/>, представляющий тип элементов, которые возвращаются при выполнении дерева выражения, связанного с данным объектом.
        /// </returns>
        public abstract Type ElementType { get; }

        /// <summary>
        /// Возвращает перечислитель, который осуществляет перебор элементов коллекции.
        /// </summary>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public IEnumerator GetEnumerator()
        {
            return InternalGetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, который осуществляет перебор элементов коллекции.
        /// </summary>
        /// <remarks>
        /// Для реализации н<see cref="GetEnumerator"/>, должен быть переопределен в наследуемом классе.
        /// </remarks>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected abstract IEnumerator InternalGetEnumerator();
    }
}
