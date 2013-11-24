using System;
using System.Data.Common;
using System.Windows;

namespace VanessaSharp.WpfClient.DataSourceSelector
{
    /// <summary>Представление свойства подключения к БД.</summary>
    internal sealed class DbConnectionPropertyView : DependencyObject
    {
        /// <summary>Построитель строки подключения.</summary>
        private DbConnectionStringBuilder _builder;

        /// <summary>
        /// Конструктор, используемый при добавлении свойства через пользовательский интерфейс.
        /// </summary>
        public DbConnectionPropertyView()
        {}

        /// <summary>Конструктор, используемый при инициализации таблицы свойств.</summary>
        /// <param name="builder">Построитель строки подключения.</param>
        /// <param name="key">Ключ свойства.</param>
        /// <param name="value">Значение свойства.</param>
        public DbConnectionPropertyView(DbConnectionStringBuilder builder, string key, string value)
        {
            Key = key;
            Value = value;

            _builder = builder;
        }

        #region Key

        public static readonly DependencyProperty KeyProperty 
            = DependencyProperty.Register("Key", typeof(string), typeof(DbConnectionPropertyView), new PropertyMetadata(KeyChangedCallback));

        /// <summary>Ключ свойства.</summary>
        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        private static void KeyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((DbConnectionPropertyView)d).OnChangeKey(
                (string)args.OldValue,
                (string)args.NewValue);
        }

        private void OnChangeKey(string oldKey, string newKey)
        {
            if (_builder == null)
                return;

            if (!string.IsNullOrEmpty(oldKey))
                _builder.Remove(oldKey);

            if (!string.IsNullOrEmpty(newKey))
                _builder.Add(newKey, Value);
        }

        #endregion

        #region Value

        public static readonly DependencyProperty ValueProperty 
            = DependencyProperty.Register("Value", typeof(string), typeof(DbConnectionPropertyView), new PropertyMetadata(ValueChangedCallback));

        /// <summary>Значение свойства.</summary>
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void ValueChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ((DbConnectionPropertyView)d).OnChangeValue();
        }

        private void OnChangeValue()
        {
            if (_builder == null)
                return;

            if (!string.IsNullOrEmpty(Key))
                _builder[Key] = Value;
        }

        #endregion

        /// <summary>Подключение объекта-представления к построителю строки поключения.</summary>
        public void AttachToBuilder(DbConnectionStringBuilder builder)
        {
            if (_builder != null)
            {
                throw new InvalidOperationException(string.Format(
                    "Объект уже подключен к построителю строки подключения."));
            }

            _builder = builder;

            OnChangeValue();
        }

        /// <summary>
        /// Отключение объекта-представления от построителя строки представления.
        /// </summary>
        public void DetachFromBuilder()
        {
            if (_builder != null && !string.IsNullOrEmpty(Key))
                _builder.Remove(Key);

            _builder = null;
        }
    }
}