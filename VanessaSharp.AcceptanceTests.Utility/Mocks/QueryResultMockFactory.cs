using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using Moq;
using VanessaSharp.Proxy.Common;

namespace VanessaSharp.AcceptanceTests.Utility.Mocks
{
    /// <summary>Фабрика для создания моков объектов 1С табличных данных.</summary>
    public sealed class QueryResultMockFactory
    {
        public static IQueryResult Create(TableData tableData)
        {
            Contract.Requires<ArgumentNullException>(tableData != null);

            var queryResultMock = MockHelper.CreateDisposableMock<IQueryResult>();

            var mapNames = CreateMapByName(tableData.Fields);

            queryResultMock
                .SetupGet(r => r.Columns)
                .Returns(CreateColumnsCollection(tableData.Fields, mapNames));

            queryResultMock
                .Setup(r => r.IsEmpty())
                .Returns(!tableData.Rows.Any());

            queryResultMock
                // TODO: Проверять переданное значение QueryResultIteration
                .Setup(r => r.Choose(It.IsAny<QueryResultIteration>()))
                .Returns<QueryResultIteration>(i =>
                    CreateQueryResultSelection(
                        tableData.Rows.Select(r => r.Select(GetOneSRawValue).ToArray()).ToList().GetEnumerator(),
                        mapNames));

            return queryResultMock.Object;
        }

        private static Dictionary<string, int> CreateMapByName(IEnumerable<FieldDescription> fields)
        {
            return fields
                .Select((f, i) => new {f.Name, Index = i})
                .ToDictionary(t => t.Name, t => t.Index);
        }

        private static IQueryResultColumnsCollection CreateColumnsCollection(ICollection<FieldDescription> fields,
                                                                             IDictionary<string, int> mapNames)
        {
            var collectionMock = MockHelper.CreateDisposableMock<IQueryResultColumnsCollection>();

            collectionMock
                .SetupGet(cs => cs.Count)
                .Returns(fields.Count);

            var columns = new ReadOnlyCollection<IQueryResultColumn>(
                    fields.Select(CreateColumn).ToArray());

            collectionMock
                .Setup(cs => cs.Get(It.IsAny<int>()))
                .Returns<int>(i => columns[i]);

            collectionMock
                .Setup(cs => cs.IndexOf(It.IsAny<IQueryResultColumn>()))
                .Returns<IQueryResultColumn>(c => columns.IndexOf(c));

            collectionMock
                .Setup(cs => cs.Find(It.IsAny<string>()))
                .Returns<string>(name => columns[mapNames[name]]);

            return collectionMock.Object;
        }

        private static IQueryResultColumn CreateColumn(FieldDescription field)
        {
            var columnMock = MockHelper
                .CreateDisposableMock<IQueryResultColumn>();

            columnMock
                .SetupGet(c => c.Name)
                .Returns(field.Name);

            columnMock
                .SetupGet(c => c.ValueType)
                // TODO Refactoring
                .Returns(TypeDescriptionMockFactory.Create(GetFieldType(field)));

            return columnMock.Object;
        }

        private static Type GetFieldType(FieldDescription field)
        {
            switch (field.Kind)
            {
                case FieldKind.Any:
                    return typeof(AnyType);
                case FieldKind.Scalar:
                    return ((ScalarFieldDescription)field).Type;
                case FieldKind.TablePart:
                    return typeof(IQueryResult);
                default:
                    throw new ArgumentOutOfRangeException("field.Kind");
            }
        }

        private static IQueryResultSelection CreateQueryResultSelection(
                IEnumerator<IList<object>> rowsEnumerator, IDictionary<string, int> mapNames)
        {
            var resultSelectionMock = MockHelper.CreateDisposableMock<IQueryResultSelection>();

            resultSelectionMock
                .Setup(s => s.Next())
                .Returns(rowsEnumerator.MoveNext);

            resultSelectionMock
                .Setup(s => s.Reset())
                .Callback(rowsEnumerator.Reset);

            Func<int, object> getValue = i => rowsEnumerator.Current[i];

            resultSelectionMock
                .Setup(s => s.Get(It.IsAny<int>()))
                .Returns(getValue);

            resultSelectionMock
                .Setup(s => s.Get(It.IsAny<string>()))
                .Returns<string>(name => getValue(mapNames[name]));

            resultSelectionMock
                .Setup(s => s.Level)
                .Returns(0);

            resultSelectionMock
                .Setup(s => s.Group)
                .Returns(string.Empty);

            resultSelectionMock
                .Setup(s => s.RecordType)
                .Returns(SelectRecordType.DetailRecord);

            return resultSelectionMock.Object;
        }

        /// <summary>
        /// Имитация получения значения которое возвращает 1С.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        public static object GetOneSRawValue(object value)
        {
            if (value is char)
                return new string((char)value, 1);

            if (value is byte)
                return (double)(byte)value;

            if (value is short)
                return (double)(short)value;

            if (value is int)
                return (double)(int)value;

            if (value is long)
                return (double)(long)value;

            if (value is float)
                return (double)(float)value;

            if (value is decimal)
                return (double)(decimal)value;

            {
                var tableData = value as TableData;
                if (tableData != null)
                    return Create(tableData);
            }

            return value;
        }
    }
}
