using OfficeOpenXml;
using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.windows.configWindow;
using projektlabor.covid19login.adminpanel.windows.mainWindow;
using projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows;
using projektlabor.covid19login.adminpanel.windows.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace projektlabor.covid19login.adminpanel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Config file that will be loaded
        private Config cfg;

        // Credentials that will be loaded
        private RequestData credentials;

        // Auth-code of the session that will be loaded
        private long authCode;

        // The user-profile that will be loaded
        private SimpleAdminEntity profile;

        // Background loading window
        private LoadingWindow backgroundLoadingWindow;

        public App()
        {
            // Disables the licence for epplus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Starts the logger
            try
            {
                Logger.init(
                    "logs/",
                    Logger.DEBUG | Logger.INFO | Logger.WARNING | Logger.ERROR,
                    Logger.INFO | Logger.WARNING | Logger.ERROR
                );
            }
            catch (Exception e)
            {
                // Displays the info
                MessageBox.Show("Failed to start logger: " + e.Message);
                // Kills the app
                Current.Shutdown(-1);
                return;
            }

            // Starts the login-process
            this.StartLoginProcess();
        }

        /// <summary>
        /// Executes once the whole login process has finished and all values have been received
        /// </summary>
        private void OnLoginSuccessfull() => this.Dispatcher.Invoke(() =>
        {
            // Opens the main-window
            var win = new MainWindow(this.profile, this.cfg, this.credentials, this.authCode);

            // Starts the window
            win.Show();

            // Closes the loading-window
            this.backgroundLoadingWindow.Close();
        });

        #region util-functions

        /// <summary>
        /// Displays the given error to the user and asks if he wants to retry
        /// </summary>
        private void DisplayConnectionError(string title, string text, Action retry) => this.Dispatcher.Invoke(() =>
        {
            // Creates the window for the error
            var win = new YesNoWindow(title, retry, Current.Shutdown, Lang.global_button_retry, Lang.global_button_cancel, title, text);

            // Displays the window
            win.Show();
        });

        /// <summary>
        /// Displays a technical error to the user can shuts-down the program
        /// </summary>
        private void DisplayTechnicalErrorAndClose(TechnicalError err) => this.Dispatcher.Invoke(() =>
        {
            // Creates the window with the acknowledgment
            var win = new AcknowledgmentWindow(
                Lang.request_error_tech_title,
                err.GetTextInCurrentLanguage() + Lang.requests_error_tech_help,
                Current.Shutdown,
                Lang.global_button_ok
            );

            // Displays the window
            win.Show();
        });

        #endregion utils-functions

        #region login-process

        /// <summary>
        /// Requests all required values from the user and performs the login with the backend-server
        /// </summary>
        private void StartLoginProcess()
        {
            // Creates the background window
            this.backgroundLoadingWindow = new LoadingWindow(Lang.startup_askconfig);

            // Shows the background window
            this.backgroundLoadingWindow.Show();

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
                this.OnConfigLoaded();
            }, Current.Shutdown);
        }

        /// <summary>
        /// Executes when the config got loaded successfully and editing has been done
        /// </summary>
        private void OnConfigLoaded()
        {
            // Updates the window
            this.backgroundLoadingWindow.DisplayText = Lang.startup_requestauth;

            // Generates the credentials
            this.credentials = new RequestData(cfg.Host, cfg.Port, cfg.UserId, cfg.PrivateKey);

            // Creates the request
            var authRequest = new AdminAuthcodeRequest()
            {
                OnEmailError = () =>
                    this.DisplayConnectionError(
                        Lang.request_error_email_title,
                        Lang.request_error_email_text,
                        OnConfigLoaded
                    ),
                OnCommonError = err =>
                    this.DisplayConnectionError(
                        Lang.request_error_common_title,
                        err.GetTextInCurrentLanguage(),
                        OnConfigLoaded
                    ),
                OnTechnicalError = this.DisplayTechnicalErrorAndClose,
                OnSuccess = () => this.OnAuthcodeSend()
            };

            // Starts the request
            Task.Run(() => authRequest.DoRequest(this.credentials));
        }

        /// <summary>
        /// Executes when the auth-code got send via email
        /// </summary>
        private void OnAuthcodeSend() => this.Dispatcher.Invoke(() =>
        {
            // Updates the window
            this.backgroundLoadingWindow.DisplayText = Lang.startup_sendauth;

            // Creates the input form for the authcode
            var win = new TextinputWindow(Lang.startup_askauth_title, Lang.startup_askauth_text, authCodeText =>
            {
                // Checks and gets the authcode for/as a long
                if (!long.TryParse(authCodeText, out this.authCode))
                {
                    // Displays the error
                    MessageBox.Show(Lang.startup_getauth_invalid);
                    // Reasks the user
                    OnAuthcodeSend();
                    return;
                }

                // Executes the next step
                this.OnUserinputAuthcode();
            }, Current.Shutdown, Lang.global_button_ok, Lang.global_button_cancel);

            // Displays the input form
            win.Show();
        });

        /// <summary>
        /// Executes when the user has input a valid authcode
        /// </summary>
        private void OnUserinputAuthcode()
        {
            // Prepares the request
            var req = new AdminGetProfileRequest()
            {
                OnCommonError = err => DisplayConnectionError(Lang.request_error_common_title, err.GetTextInCurrentLanguage(), () => OnAuthcodeSend()),
                OnTechnicalError = this.DisplayTechnicalErrorAndClose,
                OnSuccess = prof =>
                {
                    // Updates the profile
                    this.profile = prof;

                    // Executes the callback
                    this.OnLoginSuccessfull();
                }
            };

            // Starts the request
            Task.Run(()=>req.DoRequest(this.credentials, this.authCode));
        }

        #endregion login-process
    }
}
