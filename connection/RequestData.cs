using System.Security.Cryptography;

namespace projektlabor.covid19login.adminpanel.connection
{
    public struct RequestData
    {
        // User-data (Credentials) and hosts for the request
        public readonly string Host;
        public readonly int Port;
        public readonly byte ClientId;
        public readonly RSAParameters PrivateKey;

        public RequestData(string host, int port, byte clientd, RSAParameters privateKey)
        {
            this.Host = host;
            this.Port = port;
            this.ClientId = clientd;
            this.PrivateKey = privateKey;
        }
    }
}
