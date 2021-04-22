using projektlabor.covid19login.adminpanel.datahandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace projektlabor.covid19login.adminpanel.datahandling.entities
{
    public class UserEntity : SimpleUserEntity
    {
        public const string
        POSTAL_CODE = "postalcode",
	    LOCATION = "location",
	    STREET = "street",
	    HOUSE_NUMBER = "housenumber",
	    TELEPHONE = "telephone",
	    EMAIL = "email",
	    RFID = "rfidcode",
	    AUTODELETE = "autodeleteaccount",
	    REGISTER_DATE = "createdate";


        // Copy-Paste generated. Just change the class name
        // Automatically grabs and stores all attributes from the class to easily serialize and deserialize those
        private static readonly Dictionary<string, FieldInfo> ATTRIBUTES = GetAttributes(typeof(UserEntity));
        public new static readonly string[] ATTRIBUTE_LIST = GetAttributeNames(typeof(UserEntity));
        public static readonly string[] OPTIONAL_ATTRIBUTE_LIST = GetAttributeNames(typeof(UserEntity), true);
        public static readonly string[] REQUIRED_ATTRIBUTE_LIST = GetAttributeNames(typeof(UserEntity), false);

        [EntityInfo(LOCATION)]
        public string Location;
        [EntityInfo(POSTAL_CODE)]
        public int? PLZ;
        [EntityInfo(STREET)]
        public string Street;
        [EntityInfo(HOUSE_NUMBER)]
        public string StreetNumber;
        [EntityInfo(TELEPHONE,true)]
        public string TelephoneNumber;
        [EntityInfo(EMAIL,true)]
        public string Email;
        [EntityInfo(RFID,true)]
        public string Rfid;
        [EntityInfo(AUTODELETE)]
        public bool? AutoDeleteAccount;
        [EntityInfo(REGISTER_DATE)]
        public DateTime RegisterDate;

        protected override Dictionary<string, FieldInfo> Attributes() => ATTRIBUTES;

        public override string ToString() => $"{base.ToString()} Loc= {this.Street} {this.StreetNumber} {this.Location} {this.PLZ} Telephone={this.TelephoneNumber} Email={this.Email} RFID={this.Rfid} Autodelete={this.AutoDeleteAccount} Registered={this.RegisterDate}";
    }
}
