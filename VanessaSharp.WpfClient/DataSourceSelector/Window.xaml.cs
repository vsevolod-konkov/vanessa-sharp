using System.Windows;

namespace VanessaSharp.WpfClient.DataSourceSelector
{
    /// <summary>Окно выборщика источника данных.</summary>
    public partial class Window : System.Windows.Window
    {
        /// <summary>Конструктор.</summary>
        public Window()
        {
            InitializeComponent();
        }

        internal ViewModel ViewModel 
        {
            get { return (ViewModel)DataContext; }
        }

        private void Close(bool dialogResult)
        {
            DialogResult = dialogResult;
            Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
