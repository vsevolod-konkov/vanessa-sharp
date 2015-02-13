using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.Data.DataReading
{
    /// <summary>
    /// Интерфейс курсора данных 
    /// - инкапсулирует кэширование.
    /// </summary>
    [ContractClass(typeof(DataCursorContract))]
    internal interface IDataCursor : IDisposable
    {
        /// <summary>Переход на следующую запись.</summary>
        /// <returns>
        /// Возвращает <c>true</c>, если следующая запись была и переход состоялся.
        /// В ином случае возвращается <c>false</c>.
        /// </returns>
        bool Next();

        /// <summary>
        /// Сброс курсора к начальной позиции.
        /// </summary>
        void Reset();

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="ordinal">
        /// Порядковый номер поля.
        /// </param>
        object GetValue(int ordinal);

        /// <summary>
        /// Получение значения поля записи.
        /// </summary>
        /// <param name="name">
        /// Имя поля.
        /// </param>
        object GetValue(string name);

        /// <summary>
        /// Уровень текущей записи.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Имя группировки текущей записи.
        /// </summary>
        string GroupName { get; }

        /// <summary>
        /// Тип текущей записи.
        /// </summary>
        SelectRecordType RecordType { get; }

        /// <summary>
        /// Получение поствщика записей-потомков
        /// текущей записи.
        /// </summary>
        /// <param name="queryResultIteration">Стратегия обхода записей.</param>
        /// <param name="groupNames">Имена группировок.</param>
        /// <param name="groupValues">Значения группировок.</param>
        IDataRecordsProvider GetDescendantRecordsProvider(
            QueryResultIteration queryResultIteration,
            IEnumerable<string> groupNames,
            IEnumerable<string> groupValues);
    }

    [ContractClassFor(typeof(IDataCursor))]
    internal abstract class DataCursorContract : IDataCursor
    {
        bool IDataCursor.Next()
        {
            return false;
        }

        object IDataCursor.GetValue(int ordinal)
        {
            return null;
        }

        object IDataCursor.GetValue(string name)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name));

            return null;
        }

        int IDataCursor.Level
        {
            get { return 1; }
        }

        string IDataCursor.GroupName
        {
            get { return null; }
        }

        SelectRecordType IDataCursor.RecordType
        {
            get { return SelectRecordType.DetailRecord; }
        }

        IDataRecordsProvider IDataCursor.GetDescendantRecordsProvider(
            QueryResultIteration queryResultIteration,
            IEnumerable<string> groupNames,
            IEnumerable<string> groupValues)
        {
            Contract.Requires<ArgumentNullException>(
                (groupNames == null)
                || (groupNames != null && Contract.ForAll(groupNames, i => !string.IsNullOrWhiteSpace(i)))
                );

            Contract.Requires<ArgumentNullException>(
                (groupValues == null)
                || (groupValues != null && Contract.ForAll(groupValues, i => !string.IsNullOrWhiteSpace(i)))
                );

            Contract.Ensures(Contract.Result<IDataRecordsProvider>() != null);

            return null;
        }

        public abstract void Dispose();

        public abstract void Reset();
    }
}
