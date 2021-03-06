﻿namespace VanessaSharp.Proxy.Common
{
    /// <summary>Интерфейс запроса к данным 1С.</summary>
    [OneSObjectMapping(WrapType = typeof(OneSQuery), OneSTypeName = "Query")]
    public interface IQuery : IGlobalContextBound
    {
        /// <summary>Текст запроса.</summary>
        string Text { get; set; }

        /// <summary>Установка значения параметра.</summary>
        /// <param name="name">Имя параметра.</param>
        /// <param name="value">Значение парметра.</param>
        void SetParameter(string name, object value);

        /// <summary>Выполнение запроса.</summary>
        IQueryResult Execute();
    }
}
