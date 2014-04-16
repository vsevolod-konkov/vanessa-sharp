using System.Linq.Expressions;
using System.Reflection;
using VanessaSharp.Data.Linq.Internal;
using VanessaSharp.Data.Linq.UnitTests.Utility;

namespace VanessaSharp.Data.Linq.UnitTests
{
    /// <summary>
    /// Базовый класс для тестов на методы <see cref="ExpressionParser"/>.
    /// </summary>
    public abstract class ExpressionParserTestBase : TestsBase
    {
        /// <summary>
        /// Получение выражения получения записей из источника данных.
        /// </summary>
        /// <param name="sourceName">Источник.</param>
        protected static Expression GetGetRecordsExpression(string sourceName)
        {
            return Expression.Call(
                OneSQueryExpressionHelper.GetRecordsExpression(sourceName),
                OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<OneSDataRecord>());
        }

        /// <summary>Вспомогательный метод для подстановки анонимного типа.</summary>
        internal static MethodInfo GetGetEnumeratorMethodInfo<T>(Trait<T> trait)
        {
            return OneSQueryExpressionHelper.GetGetEnumeratorMethodInfo<T>();
        }
    }
}