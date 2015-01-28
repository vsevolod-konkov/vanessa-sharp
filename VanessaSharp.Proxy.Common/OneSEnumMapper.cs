using System;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Реализация <see cref="IOneSEnumMapper"/>
    /// для конвертиации перечислений.
    /// </summary>
    internal sealed class OneSEnumMapper : IOneSEnumMapper
    {
        /// <summary>
        /// Конвертация RCW-обертки 1С в перечисление.
        /// </summary>
        /// <param name="comObj">Конвертируемая RCW-обертка 1С.</param>
        /// <param name="enumType">Тип перечисления.</param>
        public object ConvertComObjectToEnum(object comObj, Type enumType)
        {
            throw new NotImplementedException();
        }
    }
}
