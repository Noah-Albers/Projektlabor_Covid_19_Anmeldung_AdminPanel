using System.Collections.Generic;
using System.Reflection;

namespace projektlabor.covid19login.adminpanel.datahandling.entities
{
    class SimpleAdminEntity : Entity
    {
        public const string
        ID = "id",
        AUTH_CODE = "authcode",
        IS_FROZEN = "isfrozen",
        NAME = "name",
        PERMISSIONS = "permissions";

        // Copy-Paste generated. Just change the class name
        // Automatically grabs and stores all attributes from the class to easily serialize and deserialize those
        private static readonly Dictionary<string, FieldInfo> ATTRIBUTES = GetAttributes(typeof(SimpleAdminEntity));
        public static readonly string[] ATTRIBUTE_LIST = GetAttributeNames(typeof(SimpleAdminEntity));
        public static readonly string[] OPTIONAL_ATTRIBUTE_LIST = GetAttributeNames(typeof(SimpleAdminEntity), true);
        public static readonly string[] REQUIRED_ATTRIBUTE_LIST = GetAttributeNames(typeof(SimpleAdminEntity), false);

        /// The unique id of the timespent entity
        [EntityInfo(ID)]
        public int? Id;

        /// When the user started to work
        [EntityInfo(AUTH_CODE,true)]
        public long? AuthCode;

        /// When the user ended his work
        [EntityInfo(IS_FROZEN)]
        public bool? IsFrozen;

        /// If the day-end stopped the work
        [EntityInfo(NAME)]
        public string Name;

        /// The user's id this timespententity belongs to
        [EntityInfo(PERMISSIONS)]
        public int? Permissions;

        protected override Dictionary<string, FieldInfo> Attributes() => ATTRIBUTES;
    }
}
