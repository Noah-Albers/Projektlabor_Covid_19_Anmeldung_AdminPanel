using projektlabor.covid19login.adminpanel.datahandling;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace projektlabor.covid19login.adminpanel.datahandling.entities
{
    class TimespentEntity : Entity
    {
        public const string
        ID = "id",
        START = "start",
        STOP = "stop",
        DISCONNECTED_ON_END = "enddisconnect",
        USER_ID = "userid";

        // Copy-Paste generated. Just change the class name
        // Automatically grabs and stores all attributes from the class to easily serialize and deserialize those
        private static readonly Dictionary<string, FieldInfo> ATTRIBUTES = GetAttributes(typeof(TimespentEntity));
        public static readonly string[] ATTRIBUTE_LIST = GetAttributeNames(typeof(TimespentEntity));
        public static readonly string[] OPTIONAL_ATTRIBUTE_LIST = GetAttributeNames(typeof(TimespentEntity), true);
        public static readonly string[] REQUIRED_ATTRIBUTE_LIST = GetAttributeNames(typeof(TimespentEntity), false);

        /// The unique id of the timespent entity
        [EntityInfo(ID)]
        public int? Id;

        /// When the user started to work
        [EntityInfo(START)]
        public DateTime? Start;

        /// When the user ended his work
        [EntityInfo(STOP,true)]
        public DateTime? Stop;

        /// If the day-end stopped the work
        [EntityInfo(DISCONNECTED_ON_END)]
        public bool? Enddisconnect;

        /// The user's id this timespententity belongs to
        [EntityInfo(USER_ID)]
        public int? UserId;

        protected override Dictionary<string, FieldInfo> Attributes() => ATTRIBUTES;
    }
}
