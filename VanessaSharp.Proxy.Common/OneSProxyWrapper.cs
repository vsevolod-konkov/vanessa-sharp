using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Стандартная реализация <see cref="IOneSProxyWrapper"/>.
    /// </summary>
    internal sealed class OneSProxyWrapper : IOneSProxyWrapper
    {
        /// <summary>Создание обертки над объектом.</summary>
        /// <param name="obj">Обертываемый объект.</param>
        public object Wrap(object obj)
        {
            if (obj == null)
                return null;

            return (OneSProxyHelper.IsOneSObject(obj))
                  ? new OneSObject(obj, this)
                  : obj;  
        }
    }
}
