using System.Windows;

namespace VanessaSharp.WpfClient.Main
{
    /// <summary>Главное окно.</summary>
    public partial class Window : System.Windows.Window
    {
        /// <summary>Конструктор.</summary>
        public Window()
        {
            InitializeComponent();
        }

        private ViewModel ViewModel
        {
            get { return (ViewModel)DataContext; }
        }

        private void SelectDataSourceButton_OnClick(object sender, RoutedEventArgs e)
        {
            SelectDataSource();
        }

        private void SelectDataSource()
        {
            // TODO: Переименовать класс с соединения на источник данных
            // TODO: Перейти на статический метод
            var dialog = new DataSourceSelector.Window { Owner = this };
            if (dialog.ShowDialog() ?? false)
            {
                ViewModel.SetDataSource(dialog.ViewModel.GetDataSource());
            }
        }

        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ExecuteQuery();
        }
    }
}
