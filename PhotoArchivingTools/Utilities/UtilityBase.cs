using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public abstract class UtilityBase
    {
        public abstract Task ExecuteAsync();

        public abstract Task InitializeAsync();
    }
}
