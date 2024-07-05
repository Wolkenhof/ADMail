﻿using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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
                    var proxyAddrString = $"{de.Properties["proxyAddresses"].Value}";
                    var mailList = GetMailLists(proxyAddrString);

                    users.Add(new User()
                    {
                        Name = $"{de.Properties["givenName"].Value}",
                        LastName = $"{de.Properties["sn"].Value}",
                        DisplayName = $"{de.Properties["displayName"].Value}",
                        Picture = $"pack://application:,,,/assets/user.png",
                        SAMAccountName = $"{de.Properties["sAMAccountName"].Value}",
                        MailList = mailList
                    });
                }
            }
            users = users.OrderBy(i => i.DisplayName).ToList();

            UserListView.ItemsSource = users;
        }

        private static IEnumerable<MailList> GetMailLists(string proxyAddrString)
        {
            var mailList = new List<MailList>();
            using var reader = new StringReader(proxyAddrString);
            while (reader.ReadLine() is { } line)
            {
                if (line.StartsWith("SMTP:"))
                    mailList.Add(new MailList
                    {
                        isPrimary = true,
                        Mail = line[4..]
                    });
                else if (line.StartsWith("smtp:"))
                    mailList.Add(new MailList
                    {
                        isPrimary = false,
                        Mail = line[4..]
                    });
            }

            return mailList;
        }

        private void UserListView_Selected(object sender, SelectionChangedEventArgs e)
        {
            // empty
        }

        #region Modules

        private static bool ContainsAny(string s, List<string> substrings)
        {
            if (string.IsNullOrEmpty(s) || substrings == null)
                return false;

            return substrings.Any(substring => s.Contains(substring, StringComparison.CurrentCultureIgnoreCase));
        }

        #endregion
    }
}