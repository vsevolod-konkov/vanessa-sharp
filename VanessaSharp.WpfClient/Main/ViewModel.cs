using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>View Model для <see cref="Main.Window"/>.</summary>
    internal sealed class ViewModel : DependencyObject
    {
        /// <summary>Конструктор для инициализации некоторых свойств.</summary>
        public ViewModel()
        {
            DataSources = new ObservableCollection<DataSourceInfo>(DataSourceStorage.Load());
        }
        
        #region DataSources

        public static readonly DependencyPropertyKey DataSourcesPropertyKey
            = DependencyProperty.RegisterReadOnly("DataSources", typeof(ObservableCollection<DataSourceInfo>), typeof(ViewModel), new PropertyMetadata(null));

        public static readonly DependencyProperty DataSourcesProperty = DataSourcesPropertyKey.DependencyProperty;

        /// <summary>Использованные источники данных.</summary>
        public ObservableCollection<DataSourceInfo> DataSources
        {
            get { return (ObservableCollection<DataSourceInfo>)GetValue(DataSourcesProperty); }
            private set { SetValue(DataSourcesPropertyKey, value); }
        }

        #endregion

        #region SelectedDataSource

        public static readonly DependencyProperty SelectedDataSourceProperty
            = DependencyProperty.Register("SelectedDataSource", typeof(DataSourceInfo), typeof(ViewModel), new PropertyMetadata(SelectedDataSourceChangeCallback));

        /// <summary>Выбранный источник данных.</summary>
        public DataSourceInfo SelectedDataSource
        {
            get { return (DataSourceInfo)GetValue(SelectedDataSourceProperty); }
            set { SetValue(SelectedDataSourceProperty, value);}
        }

        private static void SelectedDataSourceChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ViewModel)d).OnSelectedDataSourceChange((DataSourceInfo)e.NewValue);
        }

        private void OnSelectedDataSourceChange(DataSourceInfo newDataSourceInfo)
        {
            if (!IsDataSourceEquals(newDataSourceInfo))
                _dataSource = newDataSourceInfo.CreateDataSource();

            IsDataSourceEnabled = true;
        }

        private bool IsDataSourceEquals(DataSourceInfo newDataSourceInfo)
        {
            return _dataSource != null 
                && DataSourceInfo.From(_dataSource).Equals(newDataSourceInfo);
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

            var dataSourceInfo = DataSourceInfo.From(dataSource);
            if (DataSources.IndexOf(dataSourceInfo) < 0)
            {
                DataSources.Add(dataSourceInfo);
                DataSourceStorage.Save(DataSources);
            }
            SelectedDataSource = dataSourceInfo;
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
                App.ShowErrorWithStack(
                    "Ошибка при выполнении запроса",
                    "Возникла ошибка при выполнении запроса", e);
            }
        }
    }
}
