namespace VanessaSharp.Proxy.Common
{
    /// <summary>
    /// Стандартная реализация <see cref="IOneSObjectDefiner"/>.
    /// </summary>
    internal sealed class OneSObjectDefiner : IOneSObjectDefiner
    {
        /// <summary>
        /// Закрытие конструктора от соблазна создавать новые экземпляры.
        /// </summary>
        private OneSObjectDefiner()
        {}

        /// <summary>Экземпляр по умолчанию.</summary>
        public static OneSObjectDefiner Default
        {
            get { return _default; }
        }
        private static readonly OneSObjectDefiner _default = new OneSObjectDefiner();

        /// <summary>Проверка является ли объект объектом 1С.</summary>
        /// <param name="obj">Тестируемый объект.</param>
        public bool IsOneSObject(object obj)
        {
            return OneSProxyHelper.IsOneSObject(obj);
        }
    }
}