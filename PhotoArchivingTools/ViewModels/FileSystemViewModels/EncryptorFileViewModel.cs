using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PhotoArchivingTools.ViewModels
{
    public partial class EncryptorFileViewModel : SimpleFileViewModel
    {
        [ObservableProperty]
        private bool isEncrypted;

        [ObservableProperty]
        private bool isFileNameEncrypted;

        [ObservableProperty]
        private Exception error;

        public EncryptorFileViewModel(string path) : base(path)
        {
        }
    }
}
