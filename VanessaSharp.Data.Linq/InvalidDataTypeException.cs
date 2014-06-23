using System;

namespace VanessaSharp.Data.Linq
{
    /// <summary>
    /// Класс исключения в случае если данный тип не подходит
    /// для LINQ-запросов к 1С.
    /// </summary>
    public sealed class InvalidDataTypeException : Exception
    {
        /// <summary>Конструктор.</summary>
        /// <param name="invalidDataType">Некорректный тип.</param>
        /// <param name="reason">Причина по которой тип данных не является корректным.</param>
        public InvalidDataTypeException(Type invalidDataType, string reason)
        {
            _invalidDataType = invalidDataType;
            _reason = reason;
        }

        /// <summary>
        /// Некорректный тип.
        /// </summary>
        public Type InvalidDataType
        {
            get { return _invalidDataType; }
        }
        private readonly Type _invalidDataType;

        /// <summary>
        /// Причина по которой тип данных не является корректным.
        /// </summary>
        public string Reason
        {
            get { return _reason; }
        }
        private readonly string _reason;

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
                    "Тип \"{0}\" является некорректным в качестве типа данных linq-запроса к 1С. Причина: \"{1}\".",
                    InvalidDataType, Reason);
            }
        }
    }
}
