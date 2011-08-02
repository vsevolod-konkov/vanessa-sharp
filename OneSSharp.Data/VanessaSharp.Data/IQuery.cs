using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VsevolodKonkov.OneSSharp.Data
{
    /// <summary>Интерфейс запроса к информационной базе 1С.</summary>
    internal interface IQuery
    {
        /// <summary>Текст запроса.</summary>
        string Text { get; set; }

        /// <summary>Выполнение запроса.</summary>
        void Execute();
    }
}
