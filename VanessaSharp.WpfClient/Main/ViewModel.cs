using System;
using System.Data;
using System.Windows;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>View Model для <see cref="Main.Window"/>.</summary>
    internal sealed class ViewModel : DependencyObject
    {
        #region DataSourceString

        private static readonly DependencyPropertyKey _dataSourceStringPropertyKey 
            = DependencyProperty.RegisterReadOnly("DataSourceString", typeof(string), typeof(ViewModel), new PropertyMetadata("Источник не выбран"));

        /// <summary>Строка соединения к источнику данных.</summary>
        public static readonly DependencyProperty DataSourceStringProperty =
            _dataSourceStringPropertyKey.DependencyProperty;

        /// <summary>Строка соединения к источнику данных.</summary>
        public string DataSourceString
        {
            get { return (string)GetValue(DataSourceStringProperty); }

            private set { SetValue(_dataSourceStringPropertyKey, value); }
        }

        #endregion

        #region QueryText

        /// <summary>Текст запроса к источнику данных.</summary>
        public static readonly DependencyProperty QueryTextProperty 
            = DependencyProperty.Register("QueryText", typeof(string), typeof(ViewModel), new PropertyMetadata(default(string)));

        /// <summary>Текст запроса к источнику данных.</summary>
        public string QueryText
        {
            get { return (string)GetValue(QueryTextProperty); }
            set { SetValue(QueryTextProperty, value); }
        }

        #endregion

        #region QueryResult

        private static readonly DependencyPropertyKey _queryResultPropertyKey
            = DependencyProperty.RegisterReadOnly("QueryResult", typeof(DataTable), typeof(ViewModel), new PropertyMetadata(default(DataTable)));

        /// <summary>Табличный результат запроса.</summary>
        public static readonly DependencyProperty QueryResultProperty = _queryResultPropertyKey.DependencyProperty;

        /// <summary>Табличный результат запроса.</summary>
        public DataTable QueryResult
        {
            get { return (DataTable)GetValue(QueryResultProperty); }
            private set { SetValue(_queryResultPropertyKey, value); }
        }

        #endregion

        #region IsDataSourceEnabled

        /// <summary>Доступность источника данных.</summary>
        public static readonly DependencyProperty IsDataSourceEnabledProperty = DependencyProperty.Register("IsDataSourceEnabled", typeof(bool), typeof(ViewModel), new PropertyMetadata(false));

        /// <summary>Доступность источника данных.</summary>
        public bool IsDataSourceEnabled
        {
            get { return (bool)GetValue(IsDataSourceEnabledProperty); }
            set { SetValue(IsDataSourceEnabledProperty, value); }
        }

        #endregion

        /// <summary>Выбранный источник данных.</summary>
        private DataSource _dataSource;

        /// <summary>Установка источника данных.</summary>
        public void SetDataSource(DataSource dataSource)
        {
            _dataSource = dataSource;

            DataSourceString = dataSource.ToString();
            IsDataSourceEnabled = true;
        }

        /// <summary>Выполнение запроса.</summary>
        public void ExecuteQuery()
        {
            try
            {
                QueryResult = _dataSource.ExecuteQuery(QueryText);
            }
            catch (Exception e)
            {
                App.ShowError(
                    string.Format("Возникла ошибка при выполнении запроса : {0}.", e.Message),
                    "Ошибка при выполнении запроса");
            }
        }
    }
}
