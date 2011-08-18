using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс глобального контекста.</summary>
    public interface IGlobalContext
    {
        /// <summary>Создание объекта.</summary>
        /// <param name="typeName">Имя типа.</param>
        dynamic NewObject(string typeName);
    }
}
