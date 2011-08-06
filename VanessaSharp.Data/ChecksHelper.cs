using System;
using System.Diagnostics;
using NUnit.Framework;

namespace VanessaSharp.Data
{
    /// <summary>Вспомогательные методы проверки.</summary>
    public static class ChecksHelper
    {
        /// <summary>Тестирование ожидания выброса исключения типа <typeparamref name="TException"/>.</summary>
        /// <typeparam name="TException">Тип ожидаемого исключения.</typeparam>
        /// <param name="action">Дейстие, в котором ожидается выброс исключения.</param>
        /// <returns>Перехваченное исключение.</returns>
        public static TException AssertException<TException>(Action action)
            where TException : Exception
        {
            CheckArgumentNotNull(action, "action");
            
            try
            {
                action();
            }
            catch (TException expectedException)
            {
                return expectedException;
            }
            catch (Exception e)
            {
                Assert.IsAssignableFrom(typeof(TException), e);
            }

            throw new AssertionException(
                string.Format(
                    "Ожидалось исключение типа {0}.",
                    typeof(TException).FullName));
        }

        /// <summary>Тестирование ожидания выброса исключения типа <paramref name="expectedExceptionType"/>.</summary>
        /// <param name="expectedExceptionType">Тип ожидаемого исключения.</param>
        /// <param name="action">Дейстие, в котором ожидается выброс исключения.</param>
        /// <returns>Перехваченное исключение.</returns>
        public static Exception AssertException(Type expectedExceptionType, Action action)
        {
            CheckArgumentNotNull(expectedExceptionType, "expectedExceptionType");
            CheckArgumentNotNull(action, "action");
            
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsAssignableFrom(expectedExceptionType, e);
                return e;
            }

            throw new AssertionException(
                string.Format(
                    "Ожидалось исключение типа {0}.",
                    expectedExceptionType.FullName));
        }

        /// <summary>Тестирование ожидания выброса исключения <paramref name="expectedExceptionType"/>.</summary>
        /// <param name="expectedException">Тип ожидаемого исключения.</param>
        /// <param name="action">Дейстие, в котором ожидается выброс исключения.</param>
        /// <returns>Перехваченное исключение.</returns>
        public static Exception AssertException(Exception expectedException, Action action)
        {
            CheckArgumentNotNull(expectedException, "expectedException");
            CheckArgumentNotNull(action, "action");
            
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.AreEqual(expectedException, e);
                return e;
            }

            throw new AssertionException(
                string.Format(
                    "Ожидалось исключение {0}.",
                    expectedException));
        }

        /// <summary>Проверка, того что параметер имеет значение отличное от <c>null</c>.</summary>
        /// <param name="value">Значение.</param>
        /// <param name="paramName">Имя параметра.</param>
        public static void CheckArgumentNotNull(object value, string paramName)
        {
            if (value == null)
                throw new ArgumentNullException(paramName);
        }

        /// <summary>Проверка, того что параметер имеет значение отличное от <c>null</c>.</summary>
        /// <param name="value">Значение.</param>
        /// <param name="paramName">Имя параметра.</param>
        public static void CheckArgumentNotEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(
                    string.Format(
                        "Параметр \"{0}\" равен null или пустой строке.", 
                        paramName), 
                    paramName);
            }
        }
    }
}
