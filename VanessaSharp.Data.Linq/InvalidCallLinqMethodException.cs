using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Исключение, которое выбрасывается если по каким-то
    /// причинам во время выполнения был вызыван linq-метод
    /// предназначенный только для использования в деревьях выражений.
    /// </summary>
    internal sealed class InvalidCallLinqMethodException : Exception
    {
        public InvalidCallLinqMethodException(MethodBase method)
        {
            Contract.Requires<ArgumentNullException>(method != null);
            
            _method = method;
        }

        /// <summary>
        /// Метод, который был вызыван во время выполнения.
        /// </summary>
        public MethodBase Method
        {
            get { return _method; }
        }
        private readonly MethodBase _method;

        /// <summary>
        /// Возвращает сообщение, которое описывает текущее исключение.
        /// </summary>
        /// <returns>
        /// Сообщение об ошибке с объяснением причин исключения или пустая строка ("").
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public override string Message
        {
            get
            {
                return string.Format(
                    "Нельзя вызывать метод \"{0}\" во время выполенения, он предназначен только для использования в linq-выражениях.",
                    Method);
            }
        }
    }
}
