using System;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public abstract class UtilityBase
    {
        public event EventHandler<ProgressUpdateEventArgs<int>> ProgressUpdate;

        protected void NotifyProgressUpdate(int maximum, int current, string message)
        {
            ProgressUpdate?.Invoke(this, new ProgressUpdateEventArgs<int>(maximum, current, message));
        }

        public abstract Task ExecuteAsync();

        public abstract Task InitializeAsync();
    }
}
