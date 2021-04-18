using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.datahandling.exceptions
{
    class RequiredEntitySerializeException : EntitySerializeException
    {
        public RequiredEntitySerializeException(string name) : base(name) {}
    }
}
