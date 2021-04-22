using System;

namespace projektlabor.covid19login.adminpanel.datahandling
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    public class EntityInfoAttribute : Attribute
    {
        /// <summary>
        /// The json-name to serialize the attribute
        /// </summary>
        public string JsonName { get; private set; }

        /// <summary>
        /// If the attribute is optional
        /// </summary>
        public bool Optional { get; private set; }

        public EntityInfoAttribute(string jsonName,bool optional = false)
        {
            this.JsonName = jsonName;
            this.Optional = optional;
        }

    }
}
