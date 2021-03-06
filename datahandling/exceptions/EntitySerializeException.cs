using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.datahandling.exceptions
{
    [Serializable]
    class EntitySerializeException : Exception
    {
        // The name of the entry that failed to save
        public string KeyName { get; private set; }

        public EntitySerializeException(string keyName)
        {
            this.KeyName = keyName;
        }

    }
}
