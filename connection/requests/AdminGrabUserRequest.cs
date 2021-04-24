using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminGrabUserRequest : PLCARequest
    {
        // Executer if the request has success
        public Action<UserEntity> OnSuccess;

        // Executer if the user does not exist
        public Action OnNotFoundError;

        protected override int GetEndpointId() => 12;

        public void DoRequest(RequestData creds,long authCode,int userId)
        {
            // Creates the logger
            Logger log = this.GenerateLogger("AdminGrabUserRequest");

            // Performs the request
            this.DoRequest(creds, authCode, log, new JObject()
            {
                ["user"] = userId
            },this.OnReceive,this.OnError);
        }

        // Executer once the request succeeds
        private void OnReceive(JObject data,Logger log)
        {
            // Loads the user
            UserEntity user = new UserEntity();
            user.Load(data, UserEntity.REQUIRED_ATTRIBUTE_LIST, UserEntity.OPTIONAL_ATTRIBUTE_LIST);

            // Executes the success callback
            this.OnSuccess?.Invoke(user);
        }

        // Executer once the request failes
        private void OnError(string errorCode,JObject data,Logger log)
        {
            // Check the error
            switch (errorCode)
            {
                case "not_found":
                    this.OnNotFoundError?.Invoke();
                    break;
                case "database":
                    this.OnCommonError?.Invoke(CommonError.SERVER_DATABASE);
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
