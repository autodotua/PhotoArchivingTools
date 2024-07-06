using CommunityToolkit.Mvvm.ComponentModel;

namespace PhotoArchivingTools.ViewModels
{
    public partial class EncryptorFileViewModel : SimpleFileViewModel
    {
        [ObservableProperty]
        private bool isEncrypted;

        [ObservableProperty]
        private bool isFileNameEncrypted;

        public EncryptorFileViewModel(string path) : base(path)
        {
        }
    }
}
