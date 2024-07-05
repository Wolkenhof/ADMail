using System.Diagnostics;
using System.Reflection;
using System.Windows;
using ADMail.Pages;

namespace ADMail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        internal static MainWindow? ContentWindow;
        private static UserList? _userList;

        public MainWindow()
        {
            InitializeComponent();
            VersionLbl.Content = $"Version {Assembly.GetExecutingAssembly().GetName().Version!}";
            _userList = new UserList();
            FrameWindow.Content = _userList;
            ContentWindow = this;

#if DEBUG
            DebuggerBtn.Visibility = Visibility.Visible;
#endif
        }

        private void Debugger_OnClick(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
        }
    }
}