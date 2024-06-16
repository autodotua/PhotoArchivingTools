using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class DeleteJpgIfExistRawViewModel : ViewModelBase
{
    private DeleteJpgIfExistRawUtility deleteJpgIfExistRaw;

    [ObservableProperty]
    private string rawExtension = "DNG";

    [ObservableProperty]
    private List<SimpleFileViewModel> deletingJpgFiles;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        deleteJpgIfExistRaw = new DeleteJpgIfExistRawUtility()
        {
            Dir = WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir,
            RawExtension = RawExtension
        };
        await TryRunAsync(async () =>
       {
           await deleteJpgIfExistRaw.InitializeAsync();
           DeletingJpgFiles = deleteJpgIfExistRaw.DeletingJpgFiles;
       }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        return TryRunAsync(async () =>
        {
            await deleteJpgIfExistRaw.ExecuteAsync();
            deleteJpgIfExistRaw = null;
            DeletingJpgFiles = null;
        }, "执行失败");

    }
}
