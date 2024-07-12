using System;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace ADMail.Pages
{
    /// <summary>
    /// Interaktionslogik für UserList.xaml
    /// </summary>
    public partial class UserList : UserControl
    {
        public class User
        {
            public string Name { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string Picture { get; set; }
            public string SAMAccountName { get; set; }
            public IEnumerable<MailList> MailList { get; set; }
        }

        public class MailList
        {
            public string Mail { get; set; }
            public bool isPrimary { get; set; }
        }

        public static List<User> users;
        public List<User> UsersList => users;

        public UserList()
        {
            InitializeComponent();
            LoadUsers();
            this.DataContext = this;
        }

        private async void LoadUsers()
        {
            users = [];
            var names = new List<string>();
            LoadingScreen.Visibility = Visibility.Visible;
            UserListView.Visibility = Visibility.Hidden;
            await Task.Run(() =>
            {
                var deviceDomain = Environment.UserDomainName;
                using (var context = new PrincipalContext(ContextType.Domain, deviceDomain))
                {
                    using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                    var userPrincipal = new UserPrincipal(context);
                    userPrincipal.Enabled = true;
                    searcher.QueryFilter = userPrincipal;
                    var allUsers = searcher.FindAll();

                    foreach (var result in allUsers)
                    {
                        var de = result.GetUnderlyingObject() as DirectoryEntry;

                        // Getting proxyAddresses
                        var proxyAddrString = de.Properties["proxyAddresses"];
                        var mailList = GetMailLists(proxyAddrString);

                        var displayName = $"{de.Properties["displayName"].Value}";
                        if (string.IsNullOrEmpty(displayName))
                            displayName = $"{de.Properties["sAMAccountName"].Value}";

                        var samAccountName = $"{de.Properties["sAMAccountName"].Value}";
                        if (users.All(u => u.SAMAccountName != samAccountName))
                        {
                            users.Add(new User()
                            {
                                Name = $"{de.Properties["givenName"].Value}",
                                LastName = $"{de.Properties["sn"].Value}",
                                DisplayName = displayName,
                                Picture = "pack://application:,,,/assets/user.png",
                                SAMAccountName = samAccountName,
                                MailList = mailList
                            });
                        }
                    }
                }
                users = users.OrderBy(i => i.DisplayName).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserListView.ItemsSource = users;
                    LoadingScreen.Visibility = Visibility.Hidden;
                    UserListView.Visibility = Visibility.Visible;
                });
            });
        }

        private static IEnumerable<MailList> GetMailLists(PropertyValueCollection proxyAddrString)
        {
            var mailList = new List<MailList>();

            for (var i = 0; i < proxyAddrString.Count; i++)
            {
                var value = proxyAddrString[i]!.ToString();

                if (value!.StartsWith("SMTP:"))
                {
                    mailList.Add(new MailList
                    {
                        isPrimary = true,
                        Mail = value[5..]
                    });
                }
                else if (value.StartsWith("smtp:"))
                {
                    mailList.Add(new MailList
                    {
                        isPrimary = false,
                        Mail = value[5..]
                    });
                }
            }
            
            return mailList;
        }

        private void UserListView_Selected(object sender, SelectionChangedEventArgs e)
        {
            // empty
        }

        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => FilterUsers(SearchBox.Text);

        #region Modules

        private void FilterUsers(string searchText)
        {
            var filteredUsers = users.Where(user => user.DisplayName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            UserListView.ItemsSource = filteredUsers;
        }

        private static bool ContainsAny(string s, IReadOnlyCollection<string>? substrings)
        {
            if (string.IsNullOrEmpty(s) || substrings == null)
                return false;

            return substrings.Any(substring => s.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        }

        #endregion


        private void UserListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserListView.SelectedItems.Count > 0)
            {
                var selectedItem = UserListView.SelectedItems[0];
                var selectedUser = (User)selectedItem!;
                var editor = new Editor(selectedUser);
                ContentPage.ContentWindow!.FrameWindow.Content = editor;
            }
        }
    }
}
