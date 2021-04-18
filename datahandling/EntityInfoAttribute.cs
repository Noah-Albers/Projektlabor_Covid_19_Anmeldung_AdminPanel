using System;

namespace projektlabor.covid19login.adminpanel.datahandling
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    class EntityInfoAttribute : Attribute
    {
        public string JsonName { get; private set; }


        public EntityInfoAttribute(string jsonName)
        {
            this.JsonName = jsonName;
        }

    }
}
