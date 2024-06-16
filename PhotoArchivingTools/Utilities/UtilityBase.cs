using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public abstract class UtilityBase
    {
        public string Dir { get; set; }

        public abstract Task ExecuteAsync();

        public abstract Task InitializeAsync();
    }
}
