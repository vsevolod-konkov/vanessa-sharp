namespace VanessaSharp.Proxy.Common
{
    /// <summary>Реализация объекта запроса данных к 1С.</summary>
    public sealed class OneSQuery : OneSContextBoundObject, IQuery
    {
        /// <summary>Конструктор.</summary>
        /// <param name="comObject">RCW-обертка над 1С-объектом.</param>
        /// <param name="proxyWrapper">Обертыватель 1С-объектов.</param>
        /// <param name="globalContext">Глобальный контекст.</param>
        public OneSQuery(object comObject, IOneSProxyWrapper proxyWrapper, OneSGlobalContext globalContext)
            : base(comObject, proxyWrapper, globalContext)
        {}

        /// <summary>Текст запроса.</summary>
        public string Text
        {
            get { return (string)DynamicProxy.Text; }
            set { DynamicProxy.Text = value; }
        }

        /// <summary>Установка значения параметра.</summary>
        /// <param name="name">Имя параметра.</param>
        /// <param name="value">Значение парметра.</param>
        public void SetParameter(string name, object value)
        {
            DynamicProxy.SetParameter(name, value);
        }

        /// <summary>Выполнение запроса.</summary>
        public IQueryResult Execute()
        {
            return DynamicProxy.Execute();
        }
    }
}
