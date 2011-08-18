using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс соединителя с информационной базой 1С.</summary>
    public interface IOneSConnector : IDisposable
    {
        /// <summary>Соединение с информационной базой.</summary>
        /// <param name="connectString">Строка соединения.</param>
        /// <returns>Возвращает объект глобального контекста.</returns>
        IGlobalContext Connect(string connectString);
        
        /// <summary>Время ожидания подключения.</summary>
        uint PoolTimeout { get; set; }

        /// <summary>Мощность подключения.</summary>
        uint PoolCapacity { get; set; }
    }
}
