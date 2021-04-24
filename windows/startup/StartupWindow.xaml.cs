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
        // Config file that will be loaded
        private Config cfg;

        // Credentials that will be loaded
        private RequestData credentials;

        // Auth-code of the session that will be loaded
        private long authCode;

        // The user-profile that will be loaded
        private SimpleAdminEntity profile;

        public StartupWindow() => InitializeComponent();

        /// <summary>
        /// Event handler for the window-load event
        /// </summary>
        private void OnWindowLoaded(object sender, RoutedEventArgs e) =>
        // Gets all important data from the user and performs the login with the backend
        this.StartLoginProcess(() =>
        {
            // Opens the main-window
            var win = new MainWindow(this.profile, this.cfg, this.credentials, this.authCode);

            // Starts the window
            win.Show();

            // Closes this window
            this.Close();
        });

        /// <summary>
        /// Displays the given error to the user and asks if he wants to retry
        /// </summary>
        private void DisplayConnectionError(string title,string text,Action retry)
        {
            // Creates the window for the error
            var win = new YesNoWindow(title, retry, this.Close, Lang.main_startup_getauth_retry, Lang.global_button_cancel, title, text);

            // Displays the window
            win.ShowDialog();
        }

        /// <summary>
        /// Displays a technical error to the user can shuts-down the program
        /// </summary>
        private void DisplayTechnicalErrorAndClose(TechnicalError err)
        {
            // Creates the window with the acknowledgment
            var win = new AcknowledgmentWindow(
                Lang.request_error_tech_title,
                err.GetTextInCurrentLanguage() + Lang.requests_error_tech_help,
                this.Close,
                Lang.global_button_ok
            );

            // Displays the window
            win.ShowDialog();
        }

        /// <summary>
        /// Requests all required values from the user and performs the login with the backend-server
        /// </summary>
        private void StartLoginProcess(Action OnSuccess)
        {
            // Loads the config
            Config.GetConfigFromUser((isNew, cfg, pw) =>
            {
                // Checks if the config got newly created
                if (isNew)
                    // Opens a new config-edit window
                    new ConfigWindow(cfg, pw).ShowDialog();

                // Updates the config
                this.cfg = cfg;

                // Executes the config-loaded event
                this.OnceConfigLoaded(OnSuccess);
            }, this.Close);
        }

        /// <summary>
        /// Executes once the config got loaded successfully and editing has been done
        /// </summary>
        /// <param name="cfg">The loaded config</param>
        private void OnceConfigLoaded(Action onSuccess)
        {
            // Generates the credentials
            this.credentials = new RequestData(cfg.Host, cfg.Port, cfg.UserId, cfg.PrivateKey);

            // Action to resend the authcode request
            void resendAuthRequest() => OnceConfigLoaded(onSuccess);

            // Creates the request
            var authRequest = new AdminAuthcodeRequest()
            {
                OnEmailError = () =>
                    this.DisplayConnectionError(
                        Lang.request_error_email_title,
                        Lang.request_error_email_text, 
                        resendAuthRequest
                    ),
                OnCommonError = err =>
                    this.DisplayConnectionError(
                        Lang.request_error_common_title,
                        err.GetTextInCurrentLanguage(),
                        resendAuthRequest
                    ),
                OnTechnicalError = this.DisplayTechnicalErrorAndClose,
                OnSuccess = () => this.OnceAuthcodeSend(onSuccess)
            };

            // Starts the request
            authRequest.DoRequest(this.credentials);
        }

        /// <summary>
        /// Executes once the auth-code got send via email
        /// </summary>
        /// <param name="cfg">The config file </param>
        /// <param name="credentials"></param>
        private void OnceAuthcodeSend(Action onSuccess)
        {

            // Waits for the user to input the auth-code
            new TextinputWindow(Lang.main_startup_askauth_title, Lang.main_startup_askauth_title, authCodeText =>
            {
                // Checks and gets the authcode for/as a long
                if (!long.TryParse(authCodeText, out this.authCode))
                {
                    // Displays the error
                    MessageBox.Show(Lang.startup_getauth_invalid);
                    // Reasks the user
                    OnceAuthcodeSend(onSuccess);
                    return;
                }

                // Prepares the request
                var req = new AdminGetProfileRequest()
                {
                    OnCommonError = err => DisplayConnectionError(Lang.request_error_common_title, err.GetTextInCurrentLanguage(), () => OnceAuthcodeSend(onSuccess)),
                    OnTechnicalError = this.DisplayTechnicalErrorAndClose,
                    OnSuccess = prof =>
                    {
                        // Updates the profile
                        this.profile = prof;
                        // Executes the callback
                        onSuccess();
                    }
                };

                // Starts the request
                req.DoRequest(this.credentials, this.authCode);

            }, this.Close, Lang.global_button_ok, Lang.global_button_cancel).ShowDialog();
        }
    }
}
