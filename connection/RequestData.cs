using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection
{
    struct RequestData
    {
        // User-data (Credentials) and hosts for the request
        public readonly string Host;
        public readonly int Port;
        public readonly byte ClientId;
        public readonly RSAParameters PrivateKey;
    }
}
