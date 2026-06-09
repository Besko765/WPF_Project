using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace LanguageLearningApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("DispatcherUnhandledException", e.Exception);
            MessageBox.Show($"Wystąpił nieobsłużony wyjątek: {e.Exception.Message}\nSprawdź plik logu aplikacji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("CurrentDomain_UnhandledException", ex);
            }
            else
            {
                LogText($"CurrentDomain_UnhandledException: non-exception object: {e.ExceptionObject}");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException("TaskScheduler_UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private static string GetLogPath()
        {
            var dataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LanguageLearningApp");
            try { Directory.CreateDirectory(dataDir); } catch { }
            return Path.Combine(dataDir, "error.log");
        }

        private static void LogException(string source, Exception ex)
        {
            try
            {
                var path = GetLogPath();
                var text = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {source}: {ex}\n";
                File.AppendAllText(path, text);
            }
            catch { }
        }

        private static void LogText(string text)
        {
            try
            {
                var path = GetLogPath();
                var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {text}\n";
                File.AppendAllText(path, line);
            }
            catch { }
        }
    }

}
