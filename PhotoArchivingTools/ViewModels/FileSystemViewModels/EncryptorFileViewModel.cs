using CommunityToolkit.Mvvm.ComponentModel;

namespace PhotoArchivingTools.ViewModels
{
    public partial class EncryptorFileViewModel : SimpleFileViewModel
    {
        [ObservableProperty]
        public bool isEncrypted;

        [ObservableProperty]
        public bool isFileNameEncrypted;

        public EncryptorFileViewModel(string path) : base(path)
        {
        }
    }
}
