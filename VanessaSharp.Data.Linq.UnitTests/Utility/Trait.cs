﻿using System;
using System.Linq;
using System.Linq.Expressions;

namespace VanessaSharp.Data.Linq.UnitTests.Utility
{
    /// <summary>
    /// Вспомогательные методы для определения компилятором типов.
    /// Полезен для анонимных типов.
    /// </summary>
    internal static class Trait
    {
        public static Trait<T> Of<T>()
        {
            return new Trait<T>();
        }

        public static Expression<Func<T1, T2>> SelectExpression<T1, T2>(this Trait<T1> trait,
                                                                        Expression<Func<T1, T2>> selectExpression)
        {
            return selectExpression;
        }

        public static Trait<T> GetTrait<T>(this IQueryable<T> query)
        {
            return new Trait<T>();
        }

        public static Trait<T2> GetTraitOfOutputType<T1, T2>(this Expression<Func<T1, T2>> selectExpression)
        {
            return new Trait<T2>();
        }
    }

    internal sealed class Trait<T>
    {
        public Type Type { get { return typeof(T); } }
    }
}
