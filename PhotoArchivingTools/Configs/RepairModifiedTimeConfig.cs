using System;

namespace PhotoArchivingTools.Configs
{
    public class RepairModifiedTimeConfig : ConfigBase
    {
        public string Dir {  get; set; }
        public int ThreadCount { get; set; } = 2;
        public TimeSpan MaxDurationTolerance { get; set; } = TimeSpan.FromSeconds(1);
    }
}
