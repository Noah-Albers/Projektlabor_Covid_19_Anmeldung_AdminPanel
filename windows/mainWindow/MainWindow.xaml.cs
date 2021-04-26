using System.Windows;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.windows.utils;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.connection.requests;
using System.Threading.Tasks;
using projektlabor.covid19login.adminpanel.windows.mainWindow.uielements;
using projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Profile
        private readonly SimpleAdminEntity profile;

        // Current config
        private readonly Config config;

        // Requestdata
        private readonly RequestData credentials;

        // Session-auth code
        private readonly long authCode;

        public MainWindow(SimpleAdminEntity profile, Config config, RequestData credentials, long authCode)
        {
            this.profile = profile;
            this.config = config;
            this.credentials = credentials;
            this.authCode = authCode;

            InitializeComponent();

            // Updates the profile
            this.UpdateMenuByProfile();
        }

        /// <summary>
        /// Updates the window based on the current profile.
        /// Sets name, en/disables buttons based on the profile's permissions and so one
        /// </summary>
        private void UpdateMenuByProfile()
        {
            // Sets the username
            this.textUsername.Text = this.profile.Name;

            // Updates the frozen text
            this.frozenText.Visibility = this.profile.IsFrozen.Value ? Visibility.Visible : Visibility.Collapsed;

            // Updates all buttons
            foreach(var x in this.actionButtons.Children)
            {
                // Checks that the child is an action-button
                if (!(x is ActionButton))
                    continue;

                // Updates the button
                ((ActionButton)x).UpdateButtonByProfile(this.profile);
            }
        }

        /// <summary>
        /// Opens a window to inform the user about something and waits for it to close
        /// </summary>
        /// <param name="title">The window-title</param>
        /// <param name="text">The text to display to the user</param>
        private void OpenErrorInfo(string title,string text)
        {
            // Creates the window
            var win = new AcknowledgmentWindow(title, text, () => { }, Lang.global_button_ok);

            // Opens the window
            win.ShowDialog();
        }

        #region Request-Error-handlers

        /// <summary>
        /// Error handler for technical errors. Can be sets as the handler for requests
        /// </summary>
        private void TechnicalErrorHandler(TechnicalError err) => this.Dispatcher.Invoke(() =>
        {
            // Checks the error for special handle cases
            switch (err)
            {
                case TechnicalError.NO_ENDPOINT_PERMISSIONS:
                    // Informs the user and closes the application
                    this.OpenErrorInfo(
                        Lang.main_reqerror_permission_title,
                        Lang.main_reqerror_permission
                    );
                    this.Close();
                    return;
                default:
                    // Informs the user to ask an admin
                    this.OpenErrorInfo(
                        Lang.request_error_tech_title,
                        err.GetTextInCurrentLanguage() + Lang.requests_error_tech_help
                    );
                    return;
            }
        });

        /// <summary>
        /// Error handler for common errors. Can be sets as the handler for requests
        /// </summary>
        /// <param name="err"></param>
        private void CommonErrorHandler(CommonError err) => this.Dispatcher.Invoke(() =>
        {
            // Checks the error for special handle cases
            switch (err)
            {
                case CommonError.ACCOUNT_FROZEN:
                    // Updates the account
                    this.profile.IsFrozen = true;

                    // Updates the profile
                    this.UpdateMenuByProfile();
                    return;
                case CommonError.AUTH_EXPIRED:
                case CommonError.AUTH_INVALID:
                    // Informs the user and closes the application
                    this.OpenErrorInfo(
                        Lang.main_reqerror_auth_title,
                        err == CommonError.AUTH_EXPIRED ?
                            Lang.main_reqerror_auth_expired :
                            Lang.main_reqerror_auth_invalid
                    );
                    this.Close();
                    return;
                default:
                    // Displays the normal info window
                    this.OpenErrorInfo(Lang.request_error_common_title, err.GetTextInCurrentLanguage());
                    break;
            }
        });

        #endregion Request-Error-handlers

        #region Actions

        private void OnActionFreeze(object sender, RoutedEventArgs e)
        {
            // Creates the window that asks if the user wants to lock his account
            var win = new YesNoWindow(Lang.action_freeze_title, OnAcceptAccountLog, () => { }, Lang.action_freeze_yes, Lang.global_button_cancel, Lang.action_freeze_title, Lang.action_freeze_description);

            // Displays the prompt
            win.ShowDialog();

            // Executes if the user confirms the prompt
            void OnAcceptAccountLog()
            {
                // Creates the loading-window
                var loadWindow = new LoadingWindow(Lang.action_freeze_title);

                // Creates a request to freeze the account
                var req = new AdminFreezeSelfRequest()
                {
                    OnCommonError = this.CommonErrorHandler,
                    OnTechnicalError = this.TechnicalErrorHandler,
                    OnSuccess=()=>OnAccountFreeze(loadWindow)
                };

                // Starts the task with the request
                Task.Run(() => req.DoRequest(this.credentials, this.authCode));

                // Displays the window
                loadWindow.ShowDialog();
            }

            // Executes once the account has been locked
            void OnAccountFreeze(LoadingWindow window) => this.Dispatcher.Invoke(() =>
            {
                // Closes the window
                window.Close();

                // Updates the account
                this.profile.IsFrozen = true;

                // Updates the menu
                this.UpdateMenuByProfile();
            });
        }

        private void OnActionEditUser(object sender, RoutedEventArgs e)
            => new MainEdituserSubWindow(
                this.TechnicalErrorHandler,
                this.CommonErrorHandler,
                this.credentials,
                this.authCode
               ).ShowDialog();

        private void OnActionExportContacts(object sender, RoutedEventArgs e)
            => new MainExportContactsSubWindow(
                this.TechnicalErrorHandler,
                this.CommonErrorHandler,
                this.credentials,
                this.authCode
               ).ShowDialog();

        #endregion Actions

    }
}
