using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Linq;

namespace VanessaSharp.WpfClient.DataSourceSelector
{
    /// <summary>
    /// Представление коллекции свойств строки подключения к источнику данных.
    /// </summary>
    internal sealed class DbConnectionPropertiesCollectionView 
        : ObservableCollection<DbConnectionPropertyView>
    {
        /// <summary>
        /// Построитель строки подключения.
        /// </summary>
        private readonly DbConnectionStringBuilder _builder;

        /// <summary>Конструктор коллекции.</summary>
        /// <param name="builder">Построитель строки подключения.</param>
        public DbConnectionPropertiesCollectionView(DbConnectionStringBuilder builder)
            : base(GetItems(builder))
        {
            _builder = builder;
        }

        /// <summary>Получение элементов новой коллекции на основании построителя строки подключения.</summary>
        /// <param name="builder">Построитель.</param>
        private static IEnumerable<DbConnectionPropertyView> GetItems(DbConnectionStringBuilder builder)
        {
            return from string key in builder.Keys 
                   select new DbConnectionPropertyView(builder, key, Convert.ToString(builder[key]));
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            _builder.Clear();
        }

        protected override void InsertItem(int index, DbConnectionPropertyView item)
        {
            base.InsertItem(index, item);

            OnAddItem(item);
        }

        protected override void SetItem(int index, DbConnectionPropertyView item)
        {
            OnRemoveItem(this[index]);

            base.SetItem(index, item);

            OnAddItem(item);
        }

        protected override void RemoveItem(int index)
        {
            var oldItem = this[index];

            base.RemoveItem(index);

            OnRemoveItem(oldItem);
        }

        /// <summary>Обработка добавления нового свойства.</summary>
        private void OnAddItem(DbConnectionPropertyView item)
        {
            item.AttachToBuilder(_builder);
        }

        /// <summary>Обработка удаления свойства.</summary>
        private static void OnRemoveItem(DbConnectionPropertyView item)
        {
            item.DetachFromBuilder();            
        }
    }
}