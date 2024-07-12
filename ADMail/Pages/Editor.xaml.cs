using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ADMail.Pages
{
    /// <summary>
    /// Interaktionslogik für Editor.xaml
    /// </summary>
    public partial class Editor 
    {
        public Editor(UserList.User selectedUser)
        {
            InitializeComponent();

            DisplayNameLabel.Text = selectedUser.DisplayName;
            UsernameLabel.Text = selectedUser.SAMAccountName;
        }

        private void Back_OnClicked(object sender, RoutedEventArgs e)
        {
            ContentPage.ContentWindow!.FrameWindow.Content = ContentPage.UserListPage;
        }
    }
}
