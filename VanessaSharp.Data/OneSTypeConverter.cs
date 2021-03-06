﻿using System;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>
    /// Реализация по умолчанию для <see cref="IOneSTypeConverter"/>.
    /// </summary>
    internal sealed class OneSTypeConverter : IOneSTypeConverter
    {
        public const string NULL_TYPE_NAME = "Null";
        
        /// <summary>
        /// Экземпляр реализации <see cref="IOneSTypeConverter"/> по умолчанию.
        /// </summary>
        public static OneSTypeConverter Default
        {
            get
            {
                Contract.Ensures(Contract.Result<OneSTypeConverter>() != null);

                return _default;
            }
        }
        private static readonly OneSTypeConverter _default = new OneSTypeConverter();

        /// <summary>Конвертация типа 1С в тип CLR.</summary>
        /// <param name="oneSType">Тип 1С.</param>
        public Type TryConvertFrom(IOneSType oneSType)
        {
            var typeString = GetTypeName(oneSType);

            switch (typeString)
            {
                case "Строка":
                    return typeof(string);
                case "Число":
                    return typeof(double);
                case "Булево":
                    return typeof(bool);
                case "Дата":
                    return typeof(DateTime);
                case "Уникальный идентификатор":
                    return typeof(Guid);
                case NULL_TYPE_NAME:
                    return typeof(DBNull);
                case "Результат запроса":
                    return typeof(OneSDataReader);
            }

            return null;
        }

        /// <summary>
        /// Получение имени типа.
        /// </summary>
        /// <param name="oneSType">Тип 1С.</param>
        public string GetTypeName(IOneSType oneSType)
        {
            var globalContext = oneSType.GlobalContext;

            return globalContext.String(oneSType);
        }
    }
}
