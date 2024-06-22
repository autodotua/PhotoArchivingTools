using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;
public partial class TimeClassifyViewModel : ViewModelBase
{
    private TimeClassifyUtility utility;

    public TimeClassifyConfig Config { get; set; } = AppConfig.Instance.TimeClassifyConfig;

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<SimpleDirViewModel> sameTimePhotosDirs;

    protected override async Task InitializeImplAsync()
    {
        Config.Dir = Dir;
        utility = new TimeClassifyUtility(Config);

        await utility.InitializeAsync();
        SameTimePhotosDirs = utility.TargetDirs;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility);
        await utility.ExecuteAsync();
        SameTimePhotosDirs = null;
    }
}
