using Newtonsoft.Json.Linq;
using projektlabor.covid19login.adminpanel;
using projektlabor.covid19login.adminpanel.connection;
using projektlabor.covid19login.adminpanel.connection.requests;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using projektlabor.covid19login.adminpanel.security;
using System;
using System.Security.Cryptography;
using System.Windows;

namespace projektlabor.covid19login.adminpanel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Starts the logger
            try
            {
                Logger.init(
                    "logs/",
                    Logger.DEBUG | Logger.INFO | Logger.WARNING | Logger.ERROR,
                    Logger.INFO | Logger.WARNING | Logger.ERROR
                );
            }
            catch (Exception e)
            {
                // Displays the info
                MessageBox.Show("Failed to start logger: " + e.Message);
                // Kills the app
                Current.Shutdown(-1);
            }

            var obj = JObject.Parse(@"
                 {
                     'modulus': 'z+1THEUom0elr2ECzc7zOgd59IIjzsRtsPKfnbu4wy82fSqwQde0+xToT+aa/LxmOy+OwC9LBqr78oJJotAotBIeYK2FubWNmhHfqWfG8c3ku7btdEcknTHiaOqlVGbwoE2/VIKZi1fZTVhXPY1fq77rwOLx86adcU8L8QrGxq7nZeNm1ruw2vRfFFV/LBQoqHMWGUxYwZo3HL2yOY3KaYcjqWVSHN2wfevf6pzuD95hvyv/Z7YnVDNsvWZn3y+fL5K0FUNUPksZIvhg5PTxFSqQsjcrOuBIUP616AxdOBTJphTCfDKX/OIg4QgkeMSlGRFsgbuBdCIyvTLDggREbQ==',
                     'exponent': 'AQAB',
                     'p': '0SqKiN14ZhvbGcajIruUkXdFSS2bS35KNk8WoXt4hKysbjjwpSCn+HAAEskn6huAR/KAfM3zKHF+KQNrpLta+sLUqum6eXLq/ppq5TIgyfqeEdN5tYpyugLS3nBKH3qssnbBBV52U9W51I5MwaUlxZ0y3G+Nav1TaTgWcecJPzc=',
                     'q': '/nvBpQhR7GW3G7blnx6NROeu4uJWfL4PpvbzHzVMySR/CSzFkVthuxwWcMqNRoOse8ZyKU/hE1d7XyGt4tvUE5r+oHfG4XTk9uR/b24aIv+47/VRc+wrSUCR/FhVnuLZ5kd2Iv/z9ed+zBqSf8Hf9iec12bzPzROEnA0pPP0w3s=',
                     'dp': 'ghKVcgVf4QfDmeToACpseoWURayh3TGdDubh6Ovyh3cmB6lLJTUIn7tuoEANnU0a2iMY+gPNCcKCNRkWcKu+KSDNxbdxqiLntgrrHLqun0xFzkoXbui47anh3kgwICFWkei9ogwbQ4kuddtEKkv8EEbwoRqR9A2zOEST2KNXIcM=',
                     'dq': 'QCLmkfY/12lnNafpxSmJBxWxAON0Uqn//d99NJ9VQ9hb3+8Vt+WlAug4S6Lw6hWjcep4uSq1mg9RO4+caHFoyKwmgkDNseKpFgROjcHc+ncin+9e4O1jl2mboVKN+aZIrn3SK04AqTf3v+7ufx5YmIwxPiRnJ8XB62m2CuClDmM=',
                     'inverseq': 'Mc0ndxKRQlf5lispsAFV06vbJYhrj6cSV6zWc7LWL4kPwAYUD29dZ+whrM7SG4AyLKLagZAC4UYOGC8rNKHBAVIU6iD1WCAQOCYbMYkWaMJzD06ztfPjqCveqZVbFfTMlhPz1jPiJXNU3j35dVf+eN8VrFM79SpVy0UpOoXb3fs=',
                     'd': 'kRU29bXoBJl0qbAWRccOfkIzPYIFPERhiaNx7pzK6h6qdaHwxLCfzsai5wWwxYMsDkY75CvbvPZXwLpaaSm4DRXLbogFlDRzbrrkBo+sCJMy9CxK+eSeTrU9FxoLbJ47bo9xXqWWP913efmXPhLEW9FnLPrt+qYam7KdUX7EfmLO/FenWrAAtnvBK0EHJQPhmPjektJHn0LsrNx4P4otLh5hUAUCUTE6AQBwYw9mZxj4e71OaTgjo80sjXyuzm7engxiIXPK6CYaE2Hd+6KUNKfDw2oivW/EQR5HAGFJYpTu2QsOe5OFzfQWzJjzADHnG2yUT55KD1RC62pQVWcPPQ=='
                  }
            ");

            var key = SimpleSecurityUtils.LoadRSAFromJson(obj);

            var cred = new RequestData("localhost", 12345, 1, key);

            long authCode = 6326506555566115719;

            void write(string g) => Console.WriteLine($"\n======================================\n{g}\n======================================\n");

            /*var r = new AdminEditUserRequest
            {
                OnDatabaseError = () => write("Database error"),
                OnErrorIO = () => write("I/O Error"),
                OnNonsenseError = x => write("Common error: " + x),
                OnAccountFrozenError = () => write("Account is frozen"),
                OnAuthExpiredError = ()=> write("Auth code is expired"),
                OnAuthInvalidError = ()=>write("Auth code is invalid"),
                OnNoPermissionError = () => write("No permissions to request endpoint"),
                OnSuccess = () => write("Success"),
            };

            UserEntity u = new UserEntity()
            {
                Id = 1,
                AutoDeleteAccount = true,
                Firstname = "Json",
                Lastname = "Banane",
                Location = "Yeetloc",
                PLZ = 12345,
                RegisterDate = DateTime.Now,
                Street = "Some straße",
                StreetNumber = "7g",
                Rfid="ABCDERFID"
            };

            r.DoRequest(cred,authCode,u);*/

            var r = new AdminGetProfileRequest()
            {
                OnErrorIO = () => write("I/O Error"),
                OnNonsenseError = x => write("Common error: " + x),
                OnAccountFrozenError = () => write("Account is frozen"),
                OnAuthExpiredError = () => write("Auth code is expired"),
                OnAuthInvalidError = () => write("Auth code is invalid"),
                OnNoPermissionError = () => write("No permissions to request endpoint"),
                OnSuccess = x =>
                {
                    write("Success");

                    Console.WriteLine();
                },
            };

            r.DoRequest(cred, authCode);

            Current.Shutdown(0);
        }
    }
}
