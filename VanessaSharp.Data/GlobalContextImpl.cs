namespace VanessaSharp.Data
{
    /// <summary>Реализация интерфейса <see cref="IGlobalContext"/>.</summary>
    internal sealed class GlobalContextImpl : IGlobalContext
    {
        /// <summary>Возвращение контекста подключению.</summary>
        public void Unlock()
        {
        }

        /// <summary>Создание объекта запроса.</summary>
        public IQuery CreateQuery()
        {
            throw new System.NotImplementedException();
        }
    }
}
