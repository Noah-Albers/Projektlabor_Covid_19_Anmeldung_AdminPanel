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
        // Executes if the request has success
        public Action OnSuccess;

        // Executes if the given entity has a missing field that is required
        public Action<string/*Keyname*/> OnMissingFieldError;

        protected override int GetEndpointId() => 9;

        /// <exception cref="RequiredEntitySerializeException">If the user failed to save (Required attribute not given)</exception>
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
            catch (RequiredEntitySerializeException e)
            {
                this.OnMissingFieldError?.Invoke(e.KeyName);
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
                    this.OnCommonError?.Invoke(CommonError.SERVER_DATABASE);
                    break;
                default:
                    throw new Exception();
            }
        }

    }
}
