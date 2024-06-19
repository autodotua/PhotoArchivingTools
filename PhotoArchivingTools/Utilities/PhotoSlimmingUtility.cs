using PhotoArchivingTools.Configs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public class PhotoSlimmingUtility : UtilityBase
    {
        public PhotoSlimmingConfig Config { get; set; }

        public PhotoSlimmingUtility(PhotoSlimmingConfig config)
        {
            Config = config;
        }

        public override Task ExecuteAsync()
        {
            throw new System.NotImplementedException();
        }

        public override Task InitializeAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
