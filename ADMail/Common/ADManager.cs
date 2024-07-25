using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using ADMail.Pages;

namespace ADMail.Common
{
    public class ADManager
    {
        public static bool UpdateProxyAddresses(string samAccountName, List<UserList.MailList> newProxyAddresses)
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
                        var currentProxyAddresses = de.Properties["proxyAddresses"];

                        // Mark & delete old smtp-addresses
                        var addressesToRemove = currentProxyAddresses.Cast<object?>().Where(address => address.ToString().StartsWith("SMTP:", StringComparison.OrdinalIgnoreCase)).ToList();
                        foreach (var address in addressesToRemove)
                        {
                            currentProxyAddresses.Remove(address);
                        }

                        // Add new addresses
                        foreach (var address in newProxyAddresses)
                        {
                            currentProxyAddresses.Add(address.isPrimary
                                ? $"SMTP:{address.Mail}"
                                : $"smtp:{address.Mail}");
                        }

                        de.CommitChanges();
                        var errorMessage = "Proxy Adressen wurden erfolgreich geändert!";
                        var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                        messageUi.ShowDialog();
                        return true;
                    }
                }
                else
                {
                    var errorMessage = "Benutzer nicht gefunden!";
                    var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                    messageUi.ShowDialog();
                    return false;
                }
            }
            catch (Exception ex)
            {
                var errorMessage = "Fehler: " + ex.Message;
                var messageUi = new MessageUi("ADMail", errorMessage, "OK");
                messageUi.ShowDialog();
                return false;
            }
            return false;
        }
    }
}
