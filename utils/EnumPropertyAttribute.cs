using System;

namespace projektlabor.covid19login.adminpanel.utils
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class EnumProperty : Attribute
    {

        // The key
        public string Key { get; private set; }
        // The value of the property
        public object Value { get; private set; }

        public EnumProperty(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets the value
        /// </summary>
        /// <returns>The value of the property</returns>
        /// <exception cref="InvalidCastException">If the value of this property is not the same as the requested type</exception>
        public T GetValue<T>()
        {
            return (T)this.Value;
        }
    }

    public static class EnumPropertyExtension
    {
        /// <summary>
        /// Method to get the value of an enum-property from an enum.
        /// </summary>
        /// <returns>The stored object of the key. If the key could not be found, null</returns>
        public static EnumProperty GetEnumProperty(this Enum enm, string key) => enm.GetAttribute<EnumProperty>(x => x.Key.Equals(key));
    }
}
