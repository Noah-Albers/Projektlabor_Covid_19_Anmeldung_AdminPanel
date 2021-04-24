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
        // Executer when the server failed to deliver the email (Problem is eighter with the server-email or the client-email)
        public Action OnEmailError;

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
            switch (err)
            {
                case "database":
                    this.OnCommonError?.Invoke(CommonError.SERVER_DATABASE);
                    break;
                case "email":
                    this.OnEmailError?.Invoke();
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
