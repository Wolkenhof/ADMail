using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using ADMail.Common;
using static ADMail.Pages.UserList;

namespace ADMail.Pages
{
    /// <summary>
    /// Interaktionslogik für Editor.xaml
    /// </summary>
    public partial class Editor
    {
        private List<MailList> _mails = new List<MailList>();
        public List<MailList> MailList => _mails;
        internal User? User;

        // Event definition
        public event EventHandler<User>? UserUpdated;

        public Editor(User selectedUser)
        {
            InitializeComponent();
            DisplayNameLabel.Text = selectedUser.DisplayName;
            UsernameLabel.Text = selectedUser.SAMAccountName;
            User = selectedUser;
        }

        private void Editor_OnLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var mail in User?.MailList!)
            {
                if (mail.isPrimary)
                    PrimaryEmail.Text = mail.Mail;
                else _mails.Add(new MailList { Mail = mail.Mail });
            }

            this.DataContext = this;
        }

        private void Back_OnClicked(object sender, RoutedEventArgs e)
        {
            ContentPage.ContentWindow!.FrameWindow.Content = ContentPage.UserListPage;
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (MailListBox.SelectedItem is MailList selectedItem)
            {
                var mailToRemove = _mails.FirstOrDefault(u => u.Mail == selectedItem.Mail);
                if (mailToRemove == null) return;

                _mails = _mails.Except(new[] { mailToRemove }).ToList();
                MailListBox.ItemsSource = _mails.ToList();
            }
        }

        private void AddAddressBtn_OnClick(object sender, RoutedEventArgs e) => AddEmail();

        private void AddEmail()
        {
            var isValid = IsValidEmail(SecondMailTextBox.Text);
            if (!isValid)
            {
                const string errorMessage = "Die eingegebene E-Mail Adresse ist ungültig!";
                var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                messageUi.ShowDialog();
                return;
            }

            _mails.Add(new MailList { Mail = SecondMailTextBox.Text });
            MailListBox.ItemsSource = _mails.ToList();
            SecondMailTextBox.Text = "";
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                return regex.IsMatch(email);
            }
            catch (RegexParseException)
            {
                return false;
            }
        }

        private void SecondMailTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AddEmail();
        }

        private void Save_OnClicked(object sender, RoutedEventArgs e)
        {
            var isValid = IsValidEmail(PrimaryEmail.Text);
            if (!isValid)
            {
                const string errorMessage = "Die eingegebene Primäre-E-Mail Adresse ist ungültig!";
                var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                messageUi.ShowDialog();
                return;
            }

            // Add final list
            var mailList = _mails.Select(mail => new MailList { Mail = mail.Mail }).ToList();
            mailList.Add(new MailList()
            {
                isPrimary = true,
                Mail = PrimaryEmail.Text
            });

            // try to write
            var status = ADManager.UpdateProxyAddresses(UsernameLabel.Text, mailList);
            if (status)
            {
                UserUpdated?.Invoke(this, User!);
                ContentPage.ContentWindow!.FrameWindow.Content = ContentPage.UserListPage;
            }
        }
    }

}
