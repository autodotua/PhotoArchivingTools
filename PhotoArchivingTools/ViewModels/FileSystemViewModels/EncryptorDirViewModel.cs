using CommunityToolkit.Mvvm.ComponentModel;
using PhotoArchivingTools.ViewModels.FileSystemViewModels;

namespace PhotoArchivingTools.ViewModels
{
    public partial class EncryptorDirViewModel : SimpleDirViewModel
    {
        [ObservableProperty]
        private bool isDirNameEncrypted;
    }
}
