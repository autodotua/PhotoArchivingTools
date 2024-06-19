using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;
public partial class TimeClassifyViewModel : ViewModelBase
{
    private TimeClassifyUtility utility;

    public TimeClassifyConfig Config { get; set; } = new TimeClassifyConfig();


    [ObservableProperty]
    private List<SimpleDirViewModel> sameTimePhotosDirs;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Config.Dir = GetDir();
        utility = new TimeClassifyUtility(Config);
        await TryRunAsync(async () =>
        {
            await utility.InitializeAsync();
            SameTimePhotosDirs = utility.TargetDirs;
        }, "初始化失败");
    }

    [RelayCommand]
    private async Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(utility);
        await TryRunAsync(async () =>
          {
              await utility.ExecuteAsync();
              SameTimePhotosDirs = null;
          }, "执行失败");
    }
}
