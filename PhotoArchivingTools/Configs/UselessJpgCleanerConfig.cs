namespace PhotoArchivingTools.Configs
{
    public class UselessJpgCleanerConfig : ConfigBase
    {
        public string Dir { get; set; }
        public string RawExtension { get; set; } = "DNG";
    }
}
