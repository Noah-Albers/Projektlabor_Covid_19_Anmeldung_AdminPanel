using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.datahandling.exceptions;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class RegisterUserRequest : PLCARequest
    {
        // If a user with the name and lastname already exists
        public Action OnUserAlreadyExists;
        // If a user with the rfid already exists
        public Action OnRFIDAlreadyUsed;

        // If the registration was successfull (Holds the user's id)
        public Action<int> OnSuccess;

        // All entry's that the user has to pass
        private static readonly string[] REGISTER_ENTRYS = {
		    UserEntity.AUTODELETE,
		    UserEntity.HOUSE_NUMBER,
		    UserEntity.LOCATION,
		    UserEntity.POSTAL_CODE,
		    UserEntity.STREET,
		    UserEntity.FIRSTNAME,
		    UserEntity.LASTNAME
        };

        // All optional entry's that the user can pass
        private static readonly string[] OPTIONAL_REGISTER_ENTRYS = {
		    UserEntity.EMAIL,
		    UserEntity.TELEPHONE,
		    UserEntity.RFID
        };

        protected override int GetEndpointId() => 4;

        /// <summary>
        /// Starts the request
        /// </summary>
        /// <param name="userId">The id of the user that shall be loged out</param>
        public void DoRequest(RequestData credentials, UserEntity user)
        {
            // Generates the logger
            Logger log = this.GenerateLogger("RegisterUserRequest");

            log
                .Debug("Starting request to register a new user.")
                .Critical("User="+user.ToString());

            // The object that will hold the request
            JObject request = new JObject();

            try
            {
                // Tries to save the user
                user.Save(request, REGISTER_ENTRYS, OPTIONAL_REGISTER_ENTRYS);
            }catch(RequiredEntitySerializeException e)
            {
                log.Warn("Failed to serialize entity. Missing value. Missing = " + e.KeyName);

                this.OnNonsenseError?.Invoke(NonsensicalError.LESS_DATA);
                return;
            }
            catch(EntitySerializeException e)
            {
                log.Warn("Invalid input or unknown error while serializing entity. Parameter="+ e.KeyName);

                this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                return;
            }

            // Starts the request
            this.DoRequest(credentials, log, request, this.OnSuccessResponse, this.OnFailure);
        }

        /// <summary>
        /// Executes if the response is success
        /// </summary>
        private void OnSuccessResponse(JObject resp, Logger log)
        {
            // Gets the id
            int id = (int)resp["id"];

            log
                .Debug("Successfully registered a new user")
                .Critical("New userid: " + id);

            // Executes the success handler with the received id
            this.OnSuccess?.Invoke(id);
        }

        /// <summary>
        /// Error handler
        /// </summary>
        /// <exception cref="Exception">Any exception will be converted into an unknown error</exception>
        private void OnFailure(string err,JObject resp, Logger log)
        {
            log
                .Debug("Failed to register user: "+err)
                .Critical(resp);

            // Checks the error
            switch (err)
            {
                case "database":
                    this.OnNonsenseError?.Invoke(NonsensicalError.SERVER_DATABASE);
                    break;
                case "user":
                    this.OnNonsenseError?.Invoke(NonsensicalError.LESS_DATA);
                    break;
                case "dup.name":
                    this.OnUserAlreadyExists?.Invoke();
                    break;
                case "dup.rfid":
                    this.OnRFIDAlreadyUsed?.Invoke();
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;

            }
        }

    }
}
