using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using ADMail.Common;
using ADMail.Pages;

namespace ADMail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static UserList? _userList;

        public MainWindow()
        {
            InitializeComponent();
            VersionLbl.Content = $"Version {Assembly.GetExecutingAssembly().GetName().Version!}";
            _userList = new UserList();
#if DEBUG
            DebuggerBtn.Visibility = Visibility.Visible;
            TestWriteBtn.Visibility = Visibility.Visible;
#endif
        }

        private void Debugger_OnClick(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
        }

        private void TestWriteBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var a = new List<UserList.MailList>()
            {
                new UserList.MailList()
                {
                    isPrimary = true,
                    Mail = "primary@example.com"
                },
                new UserList.MailList()
                {
                    isPrimary = false,
                    Mail = "secondary@example.com"
                },
                new UserList.MailList()
                {
                    isPrimary = false,
                    Mail = "test@example.com"
                }
            };
            ADManager.UpdateProxyAddresses("test123", a);
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            RootNavigation.Navigate(typeof(Pages.ContentPage));
        }
    }
}