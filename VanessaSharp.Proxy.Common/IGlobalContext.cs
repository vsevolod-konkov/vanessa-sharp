using System;
using System.Diagnostics.Contracts;

namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс глобального контекста.</summary>
    [ContractClass(typeof(IGlobalContextContract))]
    public interface IGlobalContext : IDisposable
    {
        /// <summary>Создание объекта.</summary>
        /// <param name="typeName">Имя типа.</param>
        dynamic NewObject(string typeName);

        /// <summary>
        /// Получение признака - монопольный ли режим.
        /// </summary>
        bool ExclusiveMode();

        /// <summary>
        /// Установка монопольного режима.
        /// </summary>
        /// <param name="value">
        /// Если передать <c>true</c>, то установится монопольный режим в ином случае нет.
        /// </param>
        void SetExclusiveMode(bool value);

        /// <summary>
        /// Открывает транзакцию.
        /// Транзакция предназначена для записи в информационную базу согласованных изменений.
        /// Все изменения, внесенные в информационную базу после начала транзакции, будут затем либо целиком записаны, либо целиком отменены.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Завершает успешную транзакцию.
        /// Все изменения, внесенные в информационную базу в процессе транзакции, будут записаны.
        /// </summary>
        void CommitTransaction();

        /// <summary>
        /// Отменяет открытую ранее транзакцию.
        /// Все изменения, внесенные в информационную базу в процессе транзакции, будут отменены.
        /// </summary>
        void RollbackTransaction();
    }
}
