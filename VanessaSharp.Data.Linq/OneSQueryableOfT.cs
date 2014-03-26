using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Реализация <see cref="IQueryable{T}"/> для доступа к данным 1С.
    /// </summary>
    /// <typeparam name="T">Тип возвращаемых данных.</typeparam>
    internal sealed class OneSQueryable<T> : OneSQueryable, IQueryable<T>
    {
        /// <summary>
        /// Конструктор, принимающий поставщика запросов и выражение.
        /// </summary>
        /// <param name="provider">Поставщик запросов к 1С.</param>
        /// <param name="expression">LINQ-выражение запроса.</param>
        public OneSQueryable(IOneSQueryProvider provider, Expression expression)
            : base(provider, expression)
        {
            // TODO: Сделать проверку согласованности типа элементов и типа результата выражения
        }

        /// <summary>
        /// Получает тип элементов, которые возвращаются при выполнении дерева выражения, связанного с данным экземпляром класса <see cref="T:System.Linq.IQueryable"/>.
        /// </summary>
        /// <returns>
        /// Тип <see cref="T:System.Type"/>, представляющий тип элементов, которые возвращаются при выполнении дерева выражения, связанного с данным объектом.
        /// </returns>
        public override Type ElementType
        {
            get { return typeof(T); }
        }

        /// <summary>
        /// Возвращает перечислитель, который осуществляет перебор элементов коллекции.
        /// </summary>
        /// <remarks>
        /// Для реализации н<see cref="OneSQueryable.GetEnumerator"/>, должен быть переопределен.
        /// </remarks>
        /// <returns>
        /// Объект <see cref="T:System.Collections.IEnumerator"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        protected override IEnumerator InternalGetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Возвращает перечислитель, выполняющий итерацию в коллекции.
        /// </summary>
        /// <returns>
        /// Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1"/>, который может использоваться для перебора элементов коллекции.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public new IEnumerator<T> GetEnumerator()
        {
            return Provider.Execute<IEnumerator<T>>(
                Expression.Call(Expression, 
                    OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<T>()));
        }
    }
}
