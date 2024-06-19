using System;

namespace PhotoArchivingTools.Configs
{
    public class TimeClassifyConfig: ConfigBase
    {
        public TimeSpan MinTimeInterval { get; set; } = TimeSpan.FromMinutes(60);
    }
}
