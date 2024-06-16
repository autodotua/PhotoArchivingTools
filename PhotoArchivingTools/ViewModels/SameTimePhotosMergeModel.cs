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
    private SameTimePhotosMergeUtility sameTimePhotosMerge;

    [ObservableProperty]
    private int minTimeIntervalMinutes = 10;

    [ObservableProperty]
    private List<SimpleDirViewModel> sameTimePhotosDirs;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        sameTimePhotosMerge = new SameTimePhotosMergeUtility()
        {
            Dir = WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir,
            MinTimeInterval = TimeSpan.FromMinutes(MinTimeIntervalMinutes),
        };
        await TryRunAsync(async () =>
        {
            await sameTimePhotosMerge.InitializeAsync();
            SameTimePhotosDirs = sameTimePhotosMerge.TargetDirs;
        }, "初始化失败");
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(sameTimePhotosMerge);
        await TryRunAsync(async () =>
          {
              await sameTimePhotosMerge.ExecuteAsync();
              SameTimePhotosDirs = null;
          }, "执行失败");
    }
}
