using ADMail;
using System.Diagnostics;
using System.Windows;

namespace ADMail.Common
{
    internal class LocalizationManager
    {
        public static void LoadLanguage()
        {
            // Override language for testing
            const string overrideLanguage = "";

            var language = Thread.CurrentThread.CurrentCulture.ToString();
            var dict = new ResourceDictionary();
            if (!string.IsNullOrEmpty(overrideLanguage)) language = overrideLanguage;
            Console.WriteLine("Trying to load language: " + language);
            dict.Source = language switch
            {
                "en-US" => new Uri("/ADMail;component/Localization/ResourceDictionary.xaml", UriKind.Relative),
                "de-DE" => new Uri("/ADMail;component/Localization/ResourceDictionary.de-DE.xaml", UriKind.Relative),
                _ => new Uri("/ADMail;component/Localization/ResourceDictionary.xaml", UriKind.Relative)
            };
            try
            {
                Application.Current.Resources.MergedDictionaries.Add(dict);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load language: {ex}");
                var messageUi = new MessageUi("ADMail",
                    $"Failed to load language: {ex.Message}", "OK", isMainThread: true);
                messageUi.ShowDialog();
                Environment.Exit(1);
            }

            if (dict.Source ==
                new Uri("/ADMail;component/Localization/ResourceDictionary.xaml", UriKind.Relative) &&
                language != "en-US")
            {
                Console.WriteLine($"No localization file found for language {language}, falling back to English ...");
            }
        }

        public static string LocalizeValue(string value)
        {
            try
            {
                var localizedValue = (string)Application.Current.Resources[value]!;

                if (string.IsNullOrEmpty(localizedValue))
                    return value;

                if (localizedValue.Contains("\\n"))
                {
                    return localizedValue.Replace("\\n", "\n");
                }

                return localizedValue;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to localize value: {ex}");
                return value;
            }
        }

        public static string LocalizeValue(string value, params object[]? args)
        {
            var localizedValue = LocalizeValue(value);

            if (args is not { Length: > 0 })
                return localizedValue;

            try
            {
                localizedValue = string.Format(localizedValue, args);

                if (localizedValue.Contains("\\n"))
                {
                    localizedValue = localizedValue.Replace("\\n", "\n");
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Failed to format localized value: {ex}");
                return value;
            }

            return localizedValue;
        }
    }
}