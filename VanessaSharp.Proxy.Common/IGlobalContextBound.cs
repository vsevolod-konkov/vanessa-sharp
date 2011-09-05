using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Объект привязанный к глобальному контексту.</summary>
    public interface IGlobalContextBound : IDisposable
    {
        /// <summary>Ссылка на глобальный контекст.</summary>
        IGlobalContext GlobalContext { get;  }
    }
}
