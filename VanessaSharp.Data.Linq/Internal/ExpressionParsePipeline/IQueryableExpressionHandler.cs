using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace VanessaSharp.Data.Linq.Internal.ExpressionParsePipeline
{
    /// <summary>
    /// Обработчик парсинга выражений генерируемых <see cref="Queryable"/>.
    /// </summary>
    [ContractClass(typeof(IQueryableExpressionHandlerContract))]
    internal interface IQueryableExpressionHandler
    {
        /// <summary>Обработка начала парсинга.</summary>
        void HandleStart();

        /// <summary>Обработка завершения парсинга.</summary>
        void HandleEnd();
        
        /// <summary>Получение перечислителя.</summary>
        /// <param name="itemType">Тип элемента.</param>
        void HandleGettingEnumerator(Type itemType);

        /// <summary>Получение всех записей.</summary>
        /// <param name="sourceName">Имя источника.</param>
        void HandleGettingRecords(string sourceName);
    }

    [ContractClassFor(typeof(IQueryableExpressionHandler))]
    internal abstract class IQueryableExpressionHandlerContract : IQueryableExpressionHandler
    {
        void IQueryableExpressionHandler.HandleStart()
        {}

        void IQueryableExpressionHandler.HandleEnd()
        {}

        void IQueryableExpressionHandler.HandleGettingEnumerator(Type itemType)
        {
            Contract.Requires<ArgumentNullException>(itemType != null);
        }

        void IQueryableExpressionHandler.HandleGettingRecords(string sourceName)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(sourceName));
        }
    }
}
