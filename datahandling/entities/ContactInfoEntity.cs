using System;
using System.Collections.Generic;
using System.Reflection;

namespace projektlabor.covid19login.adminpanel.datahandling.entities
{
    class ContactInfoEntity : Entity
    {

       public const string
       INFECTED_STARTTIME = "istart",
       INFECTED_STOPTIME = "istop",
       CONTACT_STARTTIME = "cstart",
       CONTACT_STOPTIME = "cstop",
       CONTACT_ID = "cid";


        // Copy-Paste generated. Just change the class name
        // Automatically grabs and stores all attributes from the class to easily serialize and deserialize those
        private static readonly Dictionary<string, FieldInfo> ATTRIBUTES = GetAttributes(typeof(ContactInfoEntity));
        public static readonly string[] ATTRIBUTE_LIST = GetAttributeNames(typeof(ContactInfoEntity));

        [EntityInfo(INFECTED_STARTTIME)]
        public DateTime infectedStartTime;
        [EntityInfo(INFECTED_STOPTIME)]
        public DateTime infectedStopTime;

        [EntityInfo(CONTACT_STARTTIME)]
        public DateTime contactStartTime;
        [EntityInfo(CONTACT_STOPTIME)]
        public DateTime contactStopTime;

        [EntityInfo(CONTACT_ID)]
        public int? contactId;

        protected override Dictionary<string, FieldInfo> Attributes() => ATTRIBUTES;
    }
}
