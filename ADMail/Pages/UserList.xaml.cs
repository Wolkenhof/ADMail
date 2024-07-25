using System.Collections.Concurrent;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace ADMail.Pages
{
    public partial class UserList : UserControl
    {
        public class User
        {
            public string Name { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string Picture { get; set; }
            public string SAMAccountName { get; set; }
            public string SID { get; set; }
            public IEnumerable<MailList> MailList { get; set; }
        }

        public class MailList
        {
            public string Mail { get; set; }
            public bool isPrimary { get; set; }
        }

        public static ConcurrentBag<User>? users;
        public List<User> UsersList => users.ToList();

        public UserList()
        {
            InitializeComponent();
            LoadUsers();
            this.DataContext = this;
        }

        private async void LoadUsers()
        {
            users = new ConcurrentBag<User>();
            var names = new List<string>();
            LoadingScreen.Visibility = Visibility.Visible;
            UserListView.Visibility = Visibility.Hidden;
            await Task.Run(() =>
            {
                var deviceDomain = Environment.UserDomainName;
                using (var context = new PrincipalContext(ContextType.Domain, deviceDomain))
                {
                    using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                    var userPrincipal = new UserPrincipal(context)
                    {
                        Enabled = true
                    };
                    searcher.QueryFilter = userPrincipal;
                    var allUsers = searcher.FindAll();

                    foreach (var result in allUsers)
                    {
                        var de = result.GetUnderlyingObject() as DirectoryEntry;
                        if (de == null) continue;

                        // Getting proxyAddresses
                        var proxyAddrString = de.Properties["proxyAddresses"];
                        var mailList = GetMailLists(proxyAddrString);

                        var displayName = $"{de.Properties["displayName"].Value}";
                        if (string.IsNullOrEmpty(displayName))
                            displayName = $"{de.Properties["sAMAccountName"].Value}";

                        var samAccountName = $"{de.Properties["sAMAccountName"].Value}";
                        var sidBytes = (byte[])de.Properties["objectSid"].Value!;
                        var sid = new SecurityIdentifier(sidBytes, 0);
                        var sidString = sid.ToString();

                        var normalizedSamAccountName = samAccountName.Trim().ToLower();

                        // Synchronized block to check and add user to avoid concurrent modifications
                        lock (users)
                        {
                            if (users.All(u => u.SAMAccountName.Trim().ToLower() != normalizedSamAccountName))
                            {
                                users.Add(new User()
                                {
                                    Name = $"{de.Properties["givenName"].Value}",
                                    LastName = $"{de.Properties["sn"].Value}",
                                    DisplayName = displayName,
                                    Picture = "pack://application:,,,/assets/user.png",
                                    SAMAccountName = samAccountName,
                                    MailList = mailList,
                                    SID = sidString
                                });
                            }
                        }
                    }
                }
                var sortedUsers = users.OrderBy(i => i.DisplayName).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    UserListView.ItemsSource = sortedUsers;
                    LoadingScreen.Visibility = Visibility.Hidden;
                    UserListView.Visibility = Visibility.Visible;
                });
            });
        }

        private static IEnumerable<MailList> GetMailLists(PropertyValueCollection proxyAddrString)
        {
            var mailList = new List<MailList>();

            foreach (var t in proxyAddrString)
            {
                var value = t!.ToString();

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

        #endregion

        private void UserListView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (UserListView.SelectedItems.Count > 0)
            {
                var selectedItem = UserListView.SelectedItems[0];
                var selectedUser = (User)selectedItem!;
                var editor = new Editor(selectedUser);

                // Subscribe to the event
                editor.UserUpdated += Editor_UserUpdated;

                ContentPage.ContentWindow!.FrameWindow.Content = editor;
            }
        }

        private void Editor_UserUpdated(object? sender, User? e)
        {
            if (e == null) return;

            var existingUser = users!.FirstOrDefault(u => u.SAMAccountName == e.SAMAccountName);
            if (existingUser != null)
            {
                users = new ConcurrentBag<User>(users.Except(new[] { existingUser }).Append(e));
            }
            UserListView.ItemsSource = users.OrderBy(i => i.DisplayName).ToList();
            LoadUsers();
        }
    }
}
