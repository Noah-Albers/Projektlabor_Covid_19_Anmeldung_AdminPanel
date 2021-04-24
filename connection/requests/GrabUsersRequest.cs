using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class GrabUsersRequest : PLCARequest
    {
        // Executer when the request has success
        public Action<SimpleUserEntity[]> OnReceive;

        protected override int GetEndpointId() => 0;

        /// <summary>
        /// Starts the request
        /// </summary>
        public void DoRequest(RequestData credentials,long authCode)
        {
            // Gets the logger
            Logger log = this.GenerateLogger("GrabUserRequest");

            log.Debug("Starting request to fetch all users (Simple version)");

            // Starts the request
            this.DoRequest(
                credentials,
                authCode,
                log,
                new JObject(),
                this.OnReceiveRequest, (err, _2, _3) =>
                {
                    // Checks if the error is a database error
                    if (err.Equals("database"))
                        this.OnCommonError?.Invoke(CommonError.SERVER_DATABASE);
                    else
                        throw new Exception();
                }
            );
        }

        private void OnReceiveRequest(JObject resp,Logger log)
        {

            // Gets the raw users as an jarray
            JArray rawUserArr = (JArray)resp["users"];

            log
                .Debug("Received users")
                .Critical($"Users={rawUserArr.ToString(Newtonsoft.Json.Formatting.None)}");

            // Creates the array
            SimpleUserEntity[] users = new SimpleUserEntity[rawUserArr.Count];

            // Parses every user
            for (int i = 0; i < users.Length; i++)
                try
                {
                    // Tries to load the user
                    users[i] = new SimpleUserEntity();
                    users[i].Load((JObject)rawUserArr[i], SimpleUserEntity.ATTRIBUTE_LIST);
                }
                catch(Exception e)
                {
                    log.Debug("Failed to load user").Critical(e);
                    throw;
                }

            // Sends all received users
            this.OnReceive?.Invoke(users);
        }
    }
}
