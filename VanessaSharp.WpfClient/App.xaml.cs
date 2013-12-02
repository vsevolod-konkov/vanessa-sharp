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
            ShowErrorWithStack("Непредвиденная ошибка", "Возникла непредвиденная ошибка", e.Exception);
        }

        /// <summary>Показывает ошибку пользователю.</summary>
        public static void ShowErrorWithStack(string caption, string message, Exception exception)
        {
            ShowError(caption, "{0}: {1}{2}{3}", 
                message, exception.Message, Environment.NewLine, exception.StackTrace);
        }

        /// <summary>Показывает ошибку пользователю.</summary>
        public static void ShowError(string caption, string messageFormat, params object[] args)
        {
            var message = string.Format(messageFormat, args);

            Clipboard.SetText(message);
            ShowErrorDialog(Current.MainWindow, caption, message);
        }

        /// <summary>Показывает диалоговое окно с ошибкой.</summary>
        private static void ShowErrorDialog(Window owner, string caption, string message)
        {
            if (owner == null)
                MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
                MessageBox.Show(owner, message, caption, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }
}
