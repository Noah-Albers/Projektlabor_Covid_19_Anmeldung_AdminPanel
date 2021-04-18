using Newtonsoft.Json.Linq;
using System;
using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class LogoutRequest : PLCARequest
    {
        // If the user is already logged in
        public Action OnUnauthorizedError;
        // If the user hasn't been found
        public Action OnUserNotFound;

        // If the user's login was successfull
        public Action OnSuccessfullLogout;

        protected override int GetEndpointId() => 3;

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of the user that shall be loged out</param>
        public void DoRequest(RequestData credentials, int userId)
        {
            // Generates the logger
            Logger log = this.GenerateLogger("LogoutRequest");

            log
                .Debug("Starting request to logout user")
                .Critical("Userid="+userId);

            // Starts the request
            this.DoRequest(credentials,log, new JObject()
            {
                ["id"] = userId
            }, (_,_2) =>
            {
                log.Debug("Logout successful");
                this.OnSuccessfullLogout?.Invoke();
            }, this.OnFailure);
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err, JObject resp, Logger log)
        {
            log
                .Debug("Logout failed: "+err)
                .Critical(resp.ToString());

            switch (err)
            {
                // Server error (Database error eg. unreachable)
                case "database":
                    this.OnNonsenseError?.Invoke(NonsensicalError.SERVER_DATABASE);
                    break;
                // User not found
                case "user":
                    this.OnUserNotFound?.Invoke();
                    break;
                // User is already logged in (Log out first)
                case "unauthorized":
                    this.OnUnauthorizedError?.Invoke();
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }

    }
}
