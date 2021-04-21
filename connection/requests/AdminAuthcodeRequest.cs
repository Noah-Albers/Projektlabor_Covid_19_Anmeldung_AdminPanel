using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminAuthcodeRequest : SimplePLCARequest
    {
        // Executer when the request has success
        public Action OnSuccess;
        // Executer when the database returned an error
        public Action OnDatabaseError;

        protected override int GetEndpointId() => 8;

        /// <summary>
        /// Starts the request
        /// </summary>
        public void DoRequest(RequestData credentials)
        {
            // Gets the logger
            Logger log = this.GenerateLogger("AdminAuthcodeRequest");

            log.Debug("Starting request to fetch all users (Simple version)");

            // Starts the request
            this.DoRequest(
                credentials,
                log,
                new JObject(),
                (_,_2) => this.OnSuccess?.Invoke(),
                this.OnError
            );
        }

        /// <summary>
        /// Error handler
        /// </summary>
        private void OnError(string err,JObject data,Logger log)
        {
            // Checks if the error is an database-error
            if (err.Equals("database"))
                this.OnDatabaseError?.Invoke();
            else
                // Unknown error
                this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
        }
    }
}
