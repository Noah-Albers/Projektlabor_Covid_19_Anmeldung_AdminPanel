using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.windows.configWindow;
using projektlabor.covid19login.adminpanel.windows.dialogs;
using projektlabor.covid19login.adminpanel.windows.mainWindow;
using projektlabor.covid19login.adminpanel.windows.requests;
using System;
using System.Windows;

namespace projektlabor.covid19login.adminpanel.windows.startup
{
    /// <summary>
    /// Interaction logic for StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow() => InitializeComponent();

        /// <summary>
        /// Event handler for the window-load event
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e) =>
        // Gets all important data from the user and performs the login with the backend
        this.GetRequiredData((prof, cfg, cred, auth) =>
        {
            // Opens the main-window
            var win = new MainWindow(prof, cfg, cred, auth);

            // Starts the window
            win.Show();

            // Closes this window
            this.Close();
        });

        /// <summary>
        /// Requests all required values from the user and performs the login with the backend-server
        /// </summary>
        private void GetRequiredData(Action<SimpleAdminEntity /*profile*/, Config /*config*/, RequestData /*credentials*/, long /*authCode*/> OnSuccess)
        {
            // Loads the config
            Config.GetConfigFromUser((isNew, cfg, pw) =>
            {
                // Checks if the config got newly created
                if (isNew)
                    // Opens a new config-edit window
                    new ConfigWindow(cfg, pw).ShowDialog();

                // Executes the config-loaded event
                OnConfigLoaded(cfg);
            }, this.Close);

            // Executes if the connection failed
            void OnConnectionError(string title, string text, Action callback) =>
            // Displays the error and asks the user to retry the connection
            new YesNoWindow(title, callback, this.Close, Lang.main_startup_getauth_retry, Lang.global_button_cancel, title, text).ShowDialog();


            // Executes once the config got loaded successfully and editing has been done
            void OnConfigLoaded(Config cfg)
            {
                // Generates the credentials
                RequestData creds = new RequestData(cfg.Host, cfg.Port, cfg.UserId, cfg.PrivateKey);

                // Action to resend the authcode request
                Action resendAuthRequest = () => OnConfigLoaded(cfg);

                // Creates the request
                var authRequest = new AdminAuthcodeRequest()
                {
                    OnEmailError = () => OnConnectionError(Lang.requests_error_nons_title, Lang.requests_error_email_text, resendAuthRequest),
                    OnErrorIO = () => OnConnectionError(Lang.requests_error_io_title, Lang.requests_error_io_text, resendAuthRequest),
                    OnNonsenseError = x => OnConnectionError(Lang.requests_error_nons_title, Lang.ResourceManager.GetString($"requests.error.nons.{x.GetLanguageKey()}"), resendAuthRequest),
                    OnSuccess = () => OnRequestedAuthcode(cfg, creds)
                };

                // Starts the request
                authRequest.DoRequest(creds);
            }

            // Executes once the auth-code got requested successfully
            void OnRequestedAuthcode(Config cfg, RequestData credentials)
            {

                // Waits for the user to input the auth-code
                new TextinputWindow(Lang.main_startup_askauth_title, Lang.main_startup_askauth_title, authCodeText =>
                {
                    // Checks and gets the authcode for/as a long
                    if (!long.TryParse(authCodeText, out long authCode))
                    {
                        // Displays the error
                        MessageBox.Show(Lang.startup_getauth_invalid);
                        // Reasks the user
                        OnRequestedAuthcode(cfg, credentials);
                        return;
                    }

                    // Prepares the request
                    var req = new AdminGetProfileRequest()
                    {
                        OnErrorIO = () => OnConnectionError(Lang.requests_error_io_title, Lang.requests_error_io_text, () => OnRequestedAuthcode(cfg, credentials)),
                        OnNonsenseError = err => OnConnectionError(Lang.requests_error_nons_title, Lang.ResourceManager.GetString($"requests.error.nons.{err.GetLanguageKey()}"), () => OnRequestedAuthcode(cfg, credentials)),
                        OnSuccess = prof => OnSuccess(prof, cfg, credentials, authCode)
                    };

                    // Starts the request
                    req.DoRequest(credentials, authCode);

                }, this.Close, Lang.global_button_ok, Lang.global_button_cancel).ShowDialog();
            }

        }
    }
}
