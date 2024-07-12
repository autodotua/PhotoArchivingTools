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

        [ObservableProperty]
        private string relativePath;

        [ObservableProperty]
        private string targetName;

        [ObservableProperty]
        private string targetPath;

        [ObservableProperty]
        private string targetRelativePath;


        public EncryptorFileViewModel(string path) : base(path)
        {
        }
    }
}
