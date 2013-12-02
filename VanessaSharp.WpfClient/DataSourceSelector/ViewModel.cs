using System;
using System.Data;
using System.Data.Common;
using System.Windows;

namespace VanessaSharp.WpfClient.DataSourceSelector
{
    /// <summary>ViewModel для выбора источника данных.</summary>
    internal sealed class ViewModel : DependencyObject
    {
        /// <summary>Список провайдеров ADO.Net источников данных.</summary>
        public DataTable Providers
        {
            get { return DbProviderFactories.GetFactoryClasses(); }
        }

        #region SelectedProvider

        /// <summary>Выбранный провайдер ADO.Net источника данных.</summary>
        public static readonly DependencyProperty SelectedProviderProperty 
            = DependencyProperty.Register("SelectedProvider", typeof(DataRowView), typeof(ViewModel), new PropertyMetadata(SelectedProviderChangeCallback));

        /// <summary>Выбранный провайдер ADO.Net источника данных.</summary>
        public DataRowView SelectedProvider
        {
            get { return (DataRowView)GetValue(SelectedProviderProperty); }
            set { SetValue(SelectedProviderProperty, value); }
        }

        /// <summary>
        /// Обработка выбора нового провайдера ADO.Net источника данных.
        /// </summary>
        private static void SelectedProviderChangeCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ViewModel)d;
            viewModel.OnSelectedProviderChanged();
        }

        /// <summary>
        /// Обработка выбора нового провайдера ADO.Net источника данных.
        /// </summary>
        private void OnSelectedProviderChanged()
        {
            DataSourceConnectionBuilder dataSourceConnectionBuilder;
            try
            {
                dataSourceConnectionBuilder
                    = DataSourceConnectionBuilder.Create(SelectedProvider.Row);
            }
            catch (ApplicationException e)
            {
                App.ShowErrorWithStack("Ошибка при выборе источника данных", "Ошибка при выборе источника данных", e);
                SetDataSourceConnectionBuilder(null);
                
                return;
            }

            SetDataSourceConnectionBuilder(dataSourceConnectionBuilder);
        }

        #endregion

        #region ConnectionProperties

        /// <summary>Свойства подключения.</summary>
        private static readonly DependencyPropertyKey _connectionPropertiesPropertyKey
            = DependencyProperty.RegisterReadOnly("ConnectionProperties",
                                                  typeof(DbConnectionPropertiesCollectionView), 
                                                  typeof(ViewModel),
                                                  new PropertyMetadata(default(DbConnectionPropertiesCollectionView)));

        /// <summary>Свойства подключения.</summary>
        public static readonly DependencyProperty ConnectionPropertiesProperty 
            = _connectionPropertiesPropertyKey.DependencyProperty;

        /// <summary>Свойства подключения.</summary>
        public DbConnectionPropertiesCollectionView ConnectionProperties
        {
            get { return (DbConnectionPropertiesCollectionView)GetValue(ConnectionPropertiesProperty); }
            private set { SetValue(_connectionPropertiesPropertyKey, value); }
        }

        #endregion

        #region DataSourceEnabled

        /// <summary>Доступность выбранного источника данных.</summary>
        public static readonly DependencyProperty IsDataSourceEnabledProperty 
            = DependencyProperty.Register("IsDataSourceEnabled", typeof(bool), typeof(ViewModel), new PropertyMetadata(false));

        /// <summary>Доступность выбранного источника данных.</summary>
        public bool IsDataSourceEnabled
        {
            get { return (bool)GetValue(IsDataSourceEnabledProperty); }
            set { SetValue(IsDataSourceEnabledProperty, value); }
        }

        #endregion

        /// <summary>Построитель подключения к источнику данных.</summary>
        private DataSourceConnectionBuilder _dataSourceConnectionBuilder;

        /// <summary>Установка нового источника данных.</summary>
        private void SetDataSourceConnectionBuilder(DataSourceConnectionBuilder dataSourceConnectionBuilder)
        {
            _dataSourceConnectionBuilder = dataSourceConnectionBuilder;
            
            if (_dataSourceConnectionBuilder == null)
            {
                ConnectionProperties = null;
                IsDataSourceEnabled = false;
            }
            else
            {
                ConnectionProperties
                    = new DbConnectionPropertiesCollectionView(_dataSourceConnectionBuilder.DbConnectionStringBuilder);
                IsDataSourceEnabled = true;
            }
        }

        /// <summary>Получение выбранного источника данных.</summary>
        public DataSource GetDataSource()
        {
            if (!IsDataSourceEnabled)
            {
                throw new InvalidOperationException("Источник данных не выбран.");
            }

            return _dataSourceConnectionBuilder.CreateDataSource();
        }
    }
}