using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminGetProfileRequest : PLCARequest
    {
        // Executer if the request is a success
        public Action<SimpleAdminEntity> OnSuccess;

        protected override int GetEndpointId() => 11;

        public void DoRequest(RequestData creds,long authCode)
        {
            // Generates the logger
            Logger log = this.GenerateLogger("AdminGetProfileRequest");

            // Executes the request
            this.DoRequest(creds, authCode, log, null, this.OnReceive, (_, _2, _3) => throw new Exception("Unknown error occurred"));
        }

        // Handler if a response has been received
        private void OnReceive(JObject data,Logger log)
        {
            // Serializes the entity
            SimpleAdminEntity ent = new SimpleAdminEntity();
            ent.Load(data, SimpleAdminEntity.REQUIRED_ATTRIBUTE_LIST, SimpleAdminEntity.OPTIONAL_ATTRIBUTE_LIST);

            // Executes the callback
            this.OnSuccess?.Invoke(ent);
        }
    }
}
