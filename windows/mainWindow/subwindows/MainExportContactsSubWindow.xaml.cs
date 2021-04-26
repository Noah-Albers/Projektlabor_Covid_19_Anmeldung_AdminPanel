using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.Properties.langs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows
{
    /// <summary>
    /// Interaction logic for MainExportContactsSubWindow.xaml
    /// </summary>
    public partial class MainExportContactsSubWindow : MainSubWindow
    {

        /// <summary>
        /// Task that performs a request to the server to ask for users that can be displayed at the user-select
        /// </summary>
        private Task userSelectTask;

        public MainExportContactsSubWindow(Action<TechnicalError> technicalErrorHandler, Action<CommonError> commonErrorHandler, RequestData credentials, long authCode) : base(technicalErrorHandler,commonErrorHandler,credentials,authCode)
        {
            InitializeComponent();

            // Set the current date minus 2 weeks
            this.dateSelect.SelectedDate = DateTime.Now.AddDays(-14);
        }

        /// <summary>
        /// Event handler for the user-select event to request users
        /// </summary>
        private void OnUserSelectRequestUsers()
        {
            // Checks if there is already a request running
            if (this.userSelectTask != null)
                return;

            // Creates the request
            var req = new GrabUsersRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnReceive = OnReceiveUsers
            };

            // Starts the task
            this.userSelectTask = Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode);

                // Resets the task
                this.userSelectTask = null;
            });

            // Executes if the request is successfull and users are returned
            void OnReceiveUsers(SimpleUserEntity[] users) => this.Dispatcher.Invoke(() =>
                // Forwords the received users
                this.userSelect.SupplyUsers(users)
            );
        }

        /// <summary>
        /// Event handler for when the export button gets clicked
        /// </summary>
        private void OnExportButtonClicked(object sender, RoutedEventArgs e)
        {
            // Tries to get the aerosole time
            if(!int.TryParse(this.timeSelect.Text,out int marginTime) || marginTime < 0)
            {
                this.DisplayMessage(string.Empty,Lang.main_sub_contacts_time);
                return;
            }

            // Tries to get the time
            if (!this.dateSelect.SelectedDate.HasValue)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_time);
                return;
            }

            // Checks if a date has been specified
            if(this.dateSelect.SelectedDate == null)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_date);
                return;
            }

            // Checks if a user has been selected
            if(this.userSelect.User == null)
            {
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_user);
                return;
            }

            // Creates the request
            var req = new AdminInfectedContactsRequest()
            {
                OnCommonError = this.CommonRequestErrorHandler,
                OnTechnicalError = this.TechnicalRequestErrorHandler,
                OnNotFoundError = OnUserNotFound,
                OnSuccess = OnReceiveContacs

            };

            // Gets the missing values
            DateTime afterDate = this.dateSelect.SelectedDate.Value;
            int userId = this.userSelect.User.Id.Value;

            // Opens the loading-overlay
            this.OverlayLoading.Visibility = Visibility.Visible;

            // Starts the task
            Task.Run(() =>
            {
                // Performs the request
                req.DoRequest(this.credentials, this.authCode, afterDate, userId, marginTime);

                // Hides the overlay
                this.Dispatcher.Invoke(() => this.OverlayLoading.Visibility = Visibility.Collapsed);
            });

            // Executes if the selected user could not be found
            void OnUserNotFound() => this.Dispatcher.Invoke(() =>
            {
                // Clears the selected user
                this.userSelect.User = null;

                // Displays the error
                this.DisplayMessage(string.Empty, Lang.main_sub_contacts_req_user);
            });

            // Executes if the request was successfull and the contacts have been received
            void OnReceiveContacs(KeyValuePair<UserEntity, ContactInfoEntity[]>[] contacts)
            {
                Console.WriteLine();
            }
        }
    }
}
