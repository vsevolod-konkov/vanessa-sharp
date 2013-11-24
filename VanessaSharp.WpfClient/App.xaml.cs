using System;
using System.Windows;
using System.Windows.Threading;

namespace VanessaSharp.WpfClient
{
    /// <summary>
    /// Класс приложения.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>Обработка необработанного исключения.</summary>
        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // TODO Перейти на log4net
            var message = string.Format("Возникла непредвиденная ошибка: {0}{1}{2}", 
                e.Exception.Message, Environment.NewLine, e.Exception.StackTrace);

            ShowError(message, "Непредвиденная ошибка");
        }

        /// <summary>Показывает ошибку пользователю.</summary>
        public static void ShowError(string message, string caption)
        {
            Clipboard.SetText(message);
            ShowErrorDialog(Current.MainWindow, message, caption);
        }

        /// <summary>Показывает диалоговое окно с ошибкой.</summary>
        private static void ShowErrorDialog(Window owner, string message, string caption)
        {
            if (owner == null)
                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
                MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
