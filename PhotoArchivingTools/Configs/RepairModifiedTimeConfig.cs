using System;

namespace PhotoArchivingTools.Configs
{
    public class RepairModifiedTimeConfig : ConfigBase
    {
        public int ThreadCount { get; set; } = 2;
        public TimeSpan MaxDurationTolerance { get; set; } = TimeSpan.FromSeconds(1);
    }
}
