using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Data
{
    /// <summary>Интерфейс к глобальному контексту информационной базы 1С.</summary>
    internal interface IGlobalContext
    {
        /// <summary>Возвращение контекста подключению.</summary>
        void Unlock();

        /// <summary>Создание объекта запроса.</summary>
        IQuery CreateQuery();
    }
}
