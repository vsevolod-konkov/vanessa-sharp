using System;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data
{
    /// <summary>Сервис перевода типов 1С в типы CLR.</summary>
    internal interface IValueTypeConverter
    {
        /// <summary>
        /// Конвертация типа 1С в тип CLR.
        /// </summary>
        /// <param name="valueType">Описание типа 1С.</param>
        Type ConvertFrom(IValueType valueType);
    }
}
