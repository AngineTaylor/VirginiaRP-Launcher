using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Launcher.WPF.Views
{
    public partial class SteamLoginWindow : Window
    {
        public string SteamResponseUrl { get; private set; }

        public SteamLoginWindow()
        {
            InitializeComponent();
            Loaded += SteamLoginWindow_Loaded;
        }

        private async void SteamLoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Browser.EnsureCoreWebView2Async();
            }
            catch
            {
                MessageBox.Show("Не удалось инициализировать встроенный браузер. Проверьте наличие WebView2 runtime.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
                return;
            }

            string redirectUrl = "http://localhost/steamlogin";
            string steamOpenIdUrl =
                "https://steamcommunity.com/openid/login" +
                "?openid.ns=http://specs.openid.net/auth/2.0" +
                "&openid.mode=checkid_setup" +
                "&openid.return_to=" + Uri.EscapeDataString(redirectUrl) +
                "&openid.realm=" + Uri.EscapeDataString(redirectUrl) +
                "&openid.identity=http://specs.openid.net/auth/2.0/identifier_select" +
                "&openid.claimed_id=http://specs.openid.net/auth/2.0/identifier_select";

            void NavigationHandler(object s, CoreWebView2NavigationCompletedEventArgs ev)
            {
                try
                {
                    var src = Browser.Source?.ToString() ?? "";
                    if (src.StartsWith(redirectUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        SteamResponseUrl = src;
                        DialogResult = true;
                        Browser.CoreWebView2.NavigationCompleted -= NavigationHandler;
                        Close();
                    }
                }
                catch { }
            }

            Browser.CoreWebView2.NavigationCompleted += NavigationHandler;
            Browser.Source = new Uri(steamOpenIdUrl);
        }
    }
}