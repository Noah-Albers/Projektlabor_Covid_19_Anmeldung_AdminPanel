namespace projektlabor.covid19login.adminpanel.config
{
    /// <summary>
    /// If the loaded config could be decrypted, but the config is currputed
    /// </summary>
    class ConfigCurruptedException : ConfigException
    {

        /// <summary>
        /// The content that could be loaded from the config but could not be passed
        /// </summary>
        public readonly string RawInput;
        public ConfigCurruptedException(string rawInput)
        {
            this.RawInput = rawInput;
        }

    }
}
