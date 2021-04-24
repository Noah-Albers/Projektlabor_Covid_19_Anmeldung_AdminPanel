using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminInfectedContactsRequest : PLCARequest
    {
        // Executer once the request finishes successfully
        public Action<KeyValuePair<UserEntity, ContactInfoEntity[]>[]> OnSuccess;
        // Executer if the user could not be found
        public Action OnNotFoundError;


        // Values of the users that will be received
        private static readonly string[] RECEIVE_USER_REQUIRED =
        {
            SimpleUserEntity.ID,
            SimpleUserEntity.FIRSTNAME,
            SimpleUserEntity.LASTNAME,
            UserEntity.POSTAL_CODE,
            UserEntity.LOCATION,
            UserEntity.STREET,
            UserEntity.HOUSE_NUMBER
        };

        private static readonly string[] RECEIVE_USER_OPTIONAL =
        {
            UserEntity.TELEPHONE,
            UserEntity.EMAIL
        };

        protected override int GetEndpointId() => 13;

        /// <param name="afterDate">the date after which the contacts should be listed</param>
        /// <param name="infectedUserId">The user-id of the user that is infected</param>
        /// <param name="marginTime">how many minutes of margin (spacing) should be counted to a users logout time. Represents the time that the aerosols are still present</param>
        public void DoRequest(RequestData creds,long authCode,DateTime afterDate,int infectedUserId,int marginTime)
        {
            // Creates the reqeust with the different values
            JObject request = new JObject()
            {
                ["afterdate"] = new DateTimeOffset(afterDate).ToUnixTimeMilliseconds(),
                ["user"] = infectedUserId,
                ["margintime"] = marginTime
            };

            // Gets the logger
            Logger log = this.GenerateLogger("AdminInfectedContactsRequest");

            // Starts the reqeust
            this.DoRequest(creds, authCode, log, request, this.OnReceive, this.OnFailure);
        }

        // Executer if a successfull response gets received
        private void OnReceive(JObject data,Logger log)
        {
            // Gets the passed users and contacts
            var info = ((JArray)data["users"])
                // Maps them to their form
                .Select(instance =>
                {
                    // Gets the user-object as a jobject
                    JObject asJson = instance as JObject;

                    // Creates and loads the user
                    UserEntity user = new UserEntity();
                    user.Load(asJson, RECEIVE_USER_REQUIRED, RECEIVE_USER_OPTIONAL);

                    // Gets the contacts
                    var contacts = ((JArray)asJson["contactinfo"]).Select(g =>
                    {
                        // Creates and gets the contact
                        ContactInfoEntity contact = new ContactInfoEntity();
                        contact.Load(g as JObject, ContactInfoEntity.ATTRIBUTE_LIST);

                        return contact;
                    })
                    // Converts the contact to an array
                    .ToArray();

                    // Returns the user with their contacts
                    return new KeyValuePair<UserEntity, ContactInfoEntity[]>(user, contacts);
                })
                // Maps the user-contacts to an array
                .ToArray();

            // Executes the success callback
            this.OnSuccess?.Invoke(info);
        }

        // Executer if a failed response gets received
        private void OnFailure(string errorCode,JObject data,Logger log)
        {
            // Checks the error code
            switch (errorCode)
            {
                case "database":
                    this.OnCommonError?.Invoke(CommonError.SERVER_DATABASE);
                    break;
                case "not_found":
                    this.OnNotFoundError?.Invoke();
                    break;
                default:
                    throw new Exception();
            }
        }
    }
}
