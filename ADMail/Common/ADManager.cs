using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using ADMail.Pages;

namespace ADMail.Common
{
    public class ADManager
    {
        public static void UpdateProxyAddresses(string samAccountName, List<UserList.MailList> newProxyAddresses)
        {
            try
            {
                var deviceDomain = Environment.UserDomainName;
                using var context = new PrincipalContext(ContextType.Domain, deviceDomain);
                using var searcher = new PrincipalSearcher(new UserPrincipal(context) { SamAccountName = samAccountName });
                var result = searcher.FindOne();
                if (result != null)
                {
                    if (result.GetUnderlyingObject() is DirectoryEntry de)
                    {
                        de.Properties["proxyAddresses"].Clear();
                        foreach (var address in newProxyAddresses)
                        {
                            if (address.isPrimary)
                                de.Properties["proxyAddresses"].Add($"SMTP:{address.Mail}");
                            else
                                de.Properties["proxyAddresses"].Add($"smtp:{address.Mail}");
                        }
                        de.CommitChanges();
                        var errorMessage = "Proxy Adressen wurden erfolgreich geändert!";
                        var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                        messageUi.ShowDialog();
                    }
                }
                else
                {
                    var errorMessage = "Benutzer nicht gefunden!";
                    var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                    messageUi.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Fehler: " + ex.Message;
                var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                messageUi.ShowDialog();
            }
        }
    }
}
