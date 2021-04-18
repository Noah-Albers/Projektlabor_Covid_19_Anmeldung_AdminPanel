﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class GetStatusRequest : PLCARequest
    {
        // If the server returns that the send id is invalid
        public Action OnUserNotFound;

        // If the user is not logged in
        public Action OnUserNotLoggedIn;

        // If the user is logged in
        public Action<TimespentEntity> OnUserLoggedIn;

        protected override int GetEndpointId() => 1;

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of a user</param>
        public void DoRequest(RequestData credentials, int userId)
        {
            // Gets the logger
            Logger log = this.GenerateLogger("GetStatusRequest");

            log
                .Debug("Starting get status request")
                .Critical("UserId=" + userId);

            // Starting the request
            this.DoRequest(credentials, log, new JObject()
            {
                ["id"] = userId
            }, this.OnSuccess,this.OnFailure);
        }
        
        /// <summary>
        /// Handler for a successfull request
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnSuccess(JObject resp, Logger log)
        {
            // If the user is logged in
            bool loggedIn = (bool)resp["loggedin"];

            log.Critical("User is logged "+(loggedIn?"in":"out"));

            // Checks if the user isn't logged in
            if (!loggedIn)
                // Executes the not logged in callback
                this.OnUserNotLoggedIn?.Invoke();
            else
            {
                // Creates the timespent entity
                var ts = new TimespentEntity()
                {
                    Start = (DateTime)resp["start"]
                };

                // Executes the logged in callback with an unfinished timespent entity
                this.OnUserLoggedIn?.Invoke(ts);
            }
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err,JObject resp, Logger log)
        {
            log
                .Debug("Failed to perform request: "+err)
                .Critical(resp.ToString(Formatting.None));

            switch (err)
            {
                case "user":
                    // Executes the unknow user id
                    this.OnUserNotFound?.Invoke();
                    break;
                case "database":
                    this.OnNonsenseError?.Invoke(NonsensicalError.SERVER_DATABASE);
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }
    }
}
