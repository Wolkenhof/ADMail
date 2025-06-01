using System.Windows;
using ADMail.Common;

namespace ADMail
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LocalizationManager.LoadLanguage();

            var wnd = new MainWindow();
            wnd.ShowDialog();
            Environment.Exit(0);
        }
    }

}
