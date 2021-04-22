﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.connection.requests
{
    class AdminFrezeSelfRequest : PLCARequest
    {

        // Executor if the request has success
        public Action OnSuccess;
        // Executor if the request received an database error
        public Action OnDatabaseError;

        protected override int GetEndpointId() => 10;

        public void DoRequest(RequestData creds,long authCode)
        {
            // Creates the logger
            Logger log = this.GenerateLogger("AdminFrezeSelfRequest");

            // Performs the request
            this.DoRequest(creds, authCode, log, null,(_,_2)=>this.OnSuccess?.Invoke(),(err,data,logger)=>
            {
                // Checks if the error is a database error
                if (err.Equals("database"))
                    this.OnDatabaseError?.Invoke();
                else
                    this.OnNonsenseError?.Invoke(NonsensicalError.UNKNOWN);
            });
        }
    }
}
