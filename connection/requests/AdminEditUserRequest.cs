using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.datahandling.exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminEditUserRequest : PLCARequest
    {
        // Executor if the request has success
        public Action OnSuccess;
        // Executor if the request received an database error
        public Action OnDatabaseError;

        protected override int GetEndpointId() => 9;

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="RequiredEntitySerializeException">If the user failed to save (Required attribute not given)</exception>
        /// <param name="creds"></param>
        /// <param name="authCode"></param>
        /// <param name="user"></param>
        public void DoRequest(RequestData creds,long authCode,UserEntity user)
        {
            // Creates the logger
            Logger log = this.GenerateLogger("AdminEditUserRequest");

            // Creates the request
            JObject request = new JObject();

            try
            {
                // Saves the user to the entity
                user.Save(request, UserEntity.REQUIRED_ATTRIBUTE_LIST, UserEntity.OPTIONAL_ATTRIBUTE_LIST);
            }
            catch (RequiredEntitySerializeException)
            {
                throw;
            }

            // Starts the request
            this.DoRequest(creds, authCode, log, request, (_, _2) => this.OnSuccess?.Invoke(), this.OnError);
        }

        // Executec if the server returned an error
        private void OnError(string errorCode,JObject data, Logger log)
        {
            switch (errorCode)
            {
                case "database":
                    this.OnDatabaseError?.Invoke();
                    break;
                default:
                    // Unknown error
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }

    }
}
