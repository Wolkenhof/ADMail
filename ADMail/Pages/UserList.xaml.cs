using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Windows;
using System.Windows.Controls;
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
            Loaded += OnLoaded;
            this.DataContext = this;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) => LoadUsers();

        private void LoadUsers()
        {
            users = new List<User>();
            var names = new List<string>();

            var deviceDomain = Environment.UserDomainName;
            using (var context = new PrincipalContext(ContextType.Domain, deviceDomain))
            {
                using var searcher = new PrincipalSearcher(new UserPrincipal(context));
                var userPrincipal = new UserPrincipal(context);
                userPrincipal.Enabled = true;
                searcher.QueryFilter = userPrincipal;

                foreach (var result in searcher.FindAll())
                {
                    var de = result.GetUnderlyingObject() as DirectoryEntry;
                    
                    // Getting proxyAddresses
                    var proxyAddrString = de.Properties["proxyAddresses"];
                    var mailList = GetMailLists(proxyAddrString);
                    
                    var displayName = $"{de.Properties["displayName"].Value}";
                    if (string.IsNullOrEmpty(displayName))
                        displayName = $"{de.Properties["sAMAccountName"].Value}";

                    users.Add(new User()
                    {
                        Name = $"{de.Properties["givenName"].Value}",
                        LastName = $"{de.Properties["sn"].Value}",
                        DisplayName = displayName,
                        Picture = "pack://application:,,,/assets/user.png",
                        SAMAccountName = $"{de.Properties["sAMAccountName"].Value}",
                        MailList = mailList
                    });
                }
            }
            users = users.OrderBy(i => i.DisplayName).ToList();

            UserListView.ItemsSource = users;
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

        private static bool ContainsAny(string s, List<string> substrings)
        {
            if (string.IsNullOrEmpty(s) || substrings == null)
                return false;

            return substrings.Any(substring => s.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        }

        #endregion
    }
}
