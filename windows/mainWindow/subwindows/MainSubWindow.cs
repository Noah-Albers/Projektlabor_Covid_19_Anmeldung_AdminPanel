using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.Properties.langs;
using projektlabor.covid19login.adminpanel.windows.utils;
using System;
using System.Windows;

namespace projektlabor.covid19login.adminpanel.windows.mainWindow.subwindows
{
    public abstract class MainSubWindow : Window
    {
        // Main window error handlers
        private readonly Action<TechnicalError> technicalErrorHandler;
        private readonly Action<CommonError> commonErrorHandler;

        /// <summary>
        /// Information required for requests
        /// </summary>
        protected readonly RequestData credentials;
        protected readonly long authCode;

        public MainSubWindow(Action<TechnicalError> technicalErrorHandler, Action<CommonError> commonErrorHandler, RequestData credentials, long authCode)
        {
            this.technicalErrorHandler = technicalErrorHandler;
            this.commonErrorHandler = commonErrorHandler;
            this.credentials = credentials;
            this.authCode = authCode;
        }

        #region Request-error handlers

        /// <summary>
        /// Error handler for common-request errors
        /// </summary>
        protected void CommonRequestErrorHandler(CommonError err) => this.Dispatcher.Invoke(() =>
        {
            // Closes the window
            this.Close();
            // Executes the error handler
            this.commonErrorHandler(err);
        });

        /// <summary>
        /// Error handler for technical-request errors
        /// </summary>
        protected void TechnicalRequestErrorHandler(TechnicalError err) => this.Dispatcher.Invoke(() =>
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
        protected void DisplayMessage(string title, string text) => this.Dispatcher.Invoke(() =>
        {
            // Creates the window
            var win = new AcknowledgmentWindow(title, text, () => { }, Lang.global_button_ok);

            // Displays the window
            win.ShowDialog();
        });

    }
}
