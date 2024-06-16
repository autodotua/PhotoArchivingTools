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
public partial class SameTimePhotosMergeModel : ViewModelBase
{
    private SameTimePhotosMergePanelUtility sameTimePhotosMerge;

    [ObservableProperty]
    private int minTimeIntervalMinutes = 10;

    [ObservableProperty]
    private List<SimpleDirViewModel> sameTimePhotosDirs;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        sameTimePhotosMerge = new SameTimePhotosMergePanelUtility()
        {
            Dir = WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir,
            MinTimeInterval = TimeSpan.FromMinutes(MinTimeIntervalMinutes),
        };
        try
        {
            await sameTimePhotosMerge.InitializeAsync();
            SameTimePhotosDirs = sameTimePhotosMerge.TargetDirs;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = "初始化失败",
                Exception = ex
            });
        }
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(sameTimePhotosMerge);
        try
        {
            await sameTimePhotosMerge.ExecuteAsync();
            SameTimePhotosDirs = null;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = "执行失败",
                Exception = ex
            });
        }
    }
}
