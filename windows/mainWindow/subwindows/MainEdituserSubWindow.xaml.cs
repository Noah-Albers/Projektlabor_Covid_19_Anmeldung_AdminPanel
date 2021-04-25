using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.datahandling.exceptions;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.windows.utils;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows
{
    /// <summary>
    /// Interaction logic for MainEdituserSubWindow.xaml
    /// </summary>
    public partial class MainEdituserSubWindow : Window
    {
        /// <summary>
        /// Task that edits the user or grabs a selected user for users.
        /// </summary>
        private Task userEditTask;

        /// <summary>
        /// Task that waits to receive a user's profile
        /// </summary>
        private Task userGrabTask;

        /// <summary>
        /// The last selected and currently editing user-entity
        /// </summary>
        private UserEntity selectedEntity;

        // Main window error handlers
        private readonly Action<TechnicalError> technicalErrorHandler;
        private readonly Action<CommonError> commonErrorHandler;

        /// <summary>
        /// Information required for requests
        /// </summary>
        private readonly RequestData credentials;
        private readonly long authCode;

        public MainEdituserSubWindow(Action<TechnicalError> technicalErrorHandler,Action<CommonError> commonErrorHandler,RequestData credentials,long authCode)
        {
            this.technicalErrorHandler = technicalErrorHandler;
            this.commonErrorHandler = commonErrorHandler;
            this.credentials = credentials;
            this.authCode = authCode;

            InitializeComponent();
        }

        #region Request-error handlers

        /// <summary>
        /// Error handler for common-request errors
        /// </summary>
        private void CommonRequestErrorHandler(CommonError err) => this.Dispatcher.Invoke(() =>
        {
            // Closes the window
            this.Close();
            // Executes the error handler
            this.commonErrorHandler(err);
        });

        /// <summary>
        /// Error handler for technical-request errors
        /// </summary>
        private void TechnicalRequestErrorHandler(TechnicalError err) => this.Dispatcher.Invoke(() =>
        {
            // Closes the window
            this.Close();
            // Executes the error handler
            this.technicalErrorHandler(err);
        });

        # endregion Request-error handlers

        /// <summary>
        /// Displays an acknowledgement window with the message
        /// </summary>
        private void DisplayMessage(string title, string text) => this.Dispatcher.Invoke(() =>
        {
            // Creates the window
            var win = new AcknowledgmentWindow(title, text, () => { }, Lang.global_button_ok);

            // Displays the window
            win.ShowDialog();
        });

        #region Events

        /// <summary>
        /// Event handler the executes when the user-selection asks for users
        /// </summary>
        private void OnRequestUsers()
        {
            // Checks if there is already a request running
            if (this.userEditTask != null)
                return;

            // Creates the request
            var req = new GrabUsersRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                // Redirects the received users to the user-select
                OnReceive = users => this.Dispatcher.Invoke(() => this.userSearch.SupplyUsers(users))
            };

            // Starts the task
            this.userEditTask = Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode);
                // Updates the task-status
                this.userEditTask = null;
            });
        }


        /// <summary>
        /// Event handler that executes when the user selects a user from the user-selection
        /// </summary>
        /// <param name="user"></param>
        private void OnSelectUser(SimpleUserEntity user)
        {
            // Checks if there is already a request
            if (this.userGrabTask != null)
                return;

            // Creates the request to select the user
            var req = new AdminGrabUserRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnNotFoundError = () => this.DisplayMessage(Lang.main_sub_edituser_notfound_title, Lang.main_sub_edituser_notfound_text),
                OnSuccess = OnReceiveUserProfile
            };

            // Run the task
            this.userGrabTask = Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode, user.Id.Value);

                // Frees the task
                this.userGrabTask = null;
            });

            // Executes if the user's-profile gets received
            void OnReceiveUserProfile(UserEntity prof) => this.Dispatcher.Invoke(() =>
            {
                // Enables the save button and the edit-form
                this.buttonSave.IsEnabled = this.userEditField.IsEnabled = true;

                // Inserts the user
                this.userEditField.UserInput = this.selectedEntity = prof;
            });
        }

        /// <summary>
        /// Event handler for the cancel button
        /// </summary>
        private void OnButtonCancelClicked(object sender, RoutedEventArgs e) => this.Close();

        /// <summary>
        /// Event handler for the save button
        /// </summary>
        private void OnButtonSaveClicked(object sender, RoutedEventArgs e)
        {
            // Checks if the task is already running
            if (this.userEditTask != null)
                return;

            // Checks if any-field is not valid
            if (!this.userEditField.UpdateFields())
                return;

            // Updates the user-entity
            this.userEditField.MergeOn(this.selectedEntity);
            
            // Creates the save request
            var req = new AdminEditUserRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnSuccess = OnEditSuccessful
            };

            // Starts the task
            this.userEditTask = Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode, this.selectedEntity);

                // Resets the task
                this.userEditTask = null;
            });

            // Executes if the request succeeded and the user has been edited
            void OnEditSuccessful() => this.Dispatcher.Invoke(() =>
            {
                // Clears the user-field
                this.userEditField.ResetForm();

                // Disables the save button and edit-form
                this.buttonSave.IsEnabled = this.userEditField.IsEnabled = false;

                // Creates the ackwindow
                var win = new AcknowledgmentWindow(string.Empty, Lang.main_sub_edituser_success, () => { }, Lang.global_button_ok);

                // Displays the window 
                win.ShowDialog();
             });
        }

        #endregion Events
    }
}
