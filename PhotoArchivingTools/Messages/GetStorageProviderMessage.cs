using Avalonia.Platform.Storage;

namespace PhotoArchivingTools.Messages
{
    public class GetStorageProviderMessage
    {
        public IStorageProvider StorageProvider { get; set; }
    }
}
