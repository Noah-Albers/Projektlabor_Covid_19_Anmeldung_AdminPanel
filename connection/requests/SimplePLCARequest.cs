﻿using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel.connection.exceptions;
using projektlabor.covid19login.adminpanel.utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    abstract class SimplePLCARequest
    {
        // Random generator
        private readonly static Random RDM_GENERATOR = new Random();

        // Executer when an io error occurres
        public Action OnErrorIO;
        // Executer when the server returns a known handler but one that does not make sense. Eg. a permission error where to applicatation can by default only request resources where the permission is given
        public Action<NonsensicalError> OnNonsenseError;
        // Executer has no permissions to request that endpoint
        public Action OnNoPermissionError;

        /// <summary>
        /// Generates a logger with a random id that can be 
        /// </summary>
        /// <param name="presetName"></param>
        /// <returns></returns>
        protected Logger GenerateLogger(string presetName) => new Logger(presetName + "." + RDM_GENERATOR.Next());

        /// <summary>
        /// The endpoint id at the server. Can be seen like a path in http
        /// </summary>
        protected abstract int GetEndpointId();

        /// <summary>
        /// Handles any fatal errors if any occure.
        /// Fatal errors are errors that occurre before the handler for the request is executed on the server.
        /// Usually those occurre when a request has no valid endpoint or data for the handler.
        /// </summary>
        /// <param name="exc">The except-error-string to determin what kind of except-error occurred</param>
        /// <param name="log">The logger to log the error to</param>
        /// <returns>The json-object that can be forwarded as a response from the server. Otherwise just throw an error</returns>
        /// <exception cref="Exception">Can throw an exception if no special handling for the error is required</exception>
        protected virtual void HandlePreprocessingError(string exc, Logger log)
        {
            log.Debug("Fatal error returned by remote server:" + exc);

            // Checks the returned error
            switch (exc)
            {
                case "auth":
                    this.OnNoPermissionError?.Invoke();
                    break;
                case "handler":
                    this.OnNonsenseError?.Invoke(NonsensicalError.HANDLER);
                    break;
                default:
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                    break;
            }
        }

        /// <summary>
        /// Does the usual request with simple error handling (Callbacks).
        /// Executes the onReceive function if a valid response got returned that can be handled.
        /// </summary>
        /// <param name="credentials">The credentials and informations that are required to send data</param>
        /// <param name="log">The logger for the specific request</param>
        /// <param name="requestData">The request object that defines any special data that might be required to fullfil the request.</param>
        /// <param name="onReceive">The callback to handle data if it got received successfully. The callback can throw an error. It will be catched and the unknown error callback will be executed</param>
        /// <param name="onError">The callback to handle any error that might have been returned by the remote handler. Exceptions that will be thrown will be handled as unknown error, same as the receive.</param>
        protected void DoRequest(RequestData credentials, Logger log, JObject requestData, Action<JObject, Logger> onReceive, Action<string, JObject, Logger> onError = null)
        {
            try
            {

                // Starts the connection
                using (PLCASocket socket = new PLCASocket(log, credentials))
                {
                    // Creates the request
                    JObject request = new JObject()
                    {
                        ["endpoint"] = this.GetEndpointId(),     // Appends the endpoint
                        ["data"] = requestData ?? new JObject(), // Appends the request-data
                    };

                    // Gets the bytes
                    byte[] requestBytes = Encoding.UTF8.GetBytes(request.ToString());

                    log
                        .Debug("Sending request...")
                        .Critical($"Bytes=[{string.Join(",", requestBytes)}]");

                    // Sends the request
                    socket.SendPacket(requestBytes);

                    log.Debug("Waiting for response");

                    // Waits for the response
                    JObject resp = JObject.Parse(Encoding.UTF8.GetString(socket.ReceivePacket()));

                    switch ((int)resp["status"])
                    {
                        // Pre-Processing-Error
                        case 1:
                            this.HandlePreprocessingError((string)resp["error"], log);
                            return;
                        case 2:
                            // Handles the actual error
                            onError?.Invoke((string)resp["error"], (JObject)resp["data"], log);
                            return;
                        case 0:
                            // Gets the actual response from the handler
                            onReceive?.Invoke(
                                (JObject)resp["data"],
                                log
                            );
                            return;
                        default:
                            this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                            return;
                    }
                }
            }
            catch (HandshakeException)
            {
                this.OnNonsenseError?.Invoke(NonsensicalError.AUTH_KEY);
            }
            catch (Exception e)
            {
                // Checks if the error is an io-error
                if (e is IOException || e is SocketException)
                    // Handle the io-error
                    this.OnErrorIO?.Invoke();
                else
                {
                    log
                        .Debug("Unknown error occurred while performing the request")
                        .Critical(e.Message);

                    // Unknown error
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
                }
            }
        }
    }

    /// <EnumProperty>lang - the name of the enum value that can be used to grab a certaint information from the Language file.</EnumProperty>
    public enum NonsensicalError
    {
        [EnumProperty("lang", "database")] // The server database has an error (or could not be reached)
        SERVER_DATABASE,
        [EnumProperty("lang", "handler")] // The requested endpoint does not exist
        HANDLER,
        [EnumProperty("lang", "authkey")] // The rsa-key seems to not match up
        AUTH_KEY,
        [EnumProperty("lang", "unknown")] // Unknown error
        UNKNOWN
    }
}