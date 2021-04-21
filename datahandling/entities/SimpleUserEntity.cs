using projektlabor.covid19login.adminpanel.datahandling;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace projektlabor.covid19login.adminpanel.datahandling.entities
{
    public class SimpleUserEntity : Entity
    {
        public const string
        ID = "id",
	    FIRSTNAME = "firstname",
	    LASTNAME = "lastname";

        // Holds all entrys
        private static readonly Dictionary<string,FieldInfo> ENTRYS = GetEntrys(typeof(SimpleUserEntity));

        // Holds a list with all names for the entrys
        public static readonly string[] ENTRYS_LIST = ENTRYS.Select(i => i.Key).ToArray();

        /// The users unique id
        [EntityInfo(ID)]
        public int? Id;

        /// The users firstname
        [EntityInfo(FIRSTNAME)]
        public string Firstname;

        /// The users lastname
        [EntityInfo(LASTNAME)]
        public string Lastname;

        /// <summary>
        /// Checks if the user matches the search
        /// </summary>
        /// <param name="search"></param>
        /// <returns>True if the user matches; else false</returns>
        public bool IsMatching(string search) => this.ToString().ToLower().Contains(search.ToLower());

        public override string ToString() => $"{this.Firstname} {this.Lastname}";

        protected override Dictionary<string, FieldInfo> Entrys() => ENTRYS;
    }
}
