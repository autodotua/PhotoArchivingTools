using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PhotoArchivingTools.Messages;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;
public partial class MainViewModel : ObservableObject
{
    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<GetDirMessage>(this, (_, m) =>
        {
            m.Dir = Dir;
        });
    }

    [ObservableProperty]
    private string dir;

    [RelayCommand]
    private async Task BrowseDirAsync()
    {
        FolderPickerOpenOptions options = new FolderPickerOpenOptions()
        {
            AllowMultiple = false,
            Title = "选择操作目录",
        };
        var result = await WeakReferenceMessenger.Default
            .Send(new GetStorageProviderMessage())
            .StorageProvider
            .OpenFolderPickerAsync(options);
        if (result is { Count: > 0 })
        {
            Dir = result[0].TryGetLocalPath();
        }
    }
}
