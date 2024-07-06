using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Utilities;
using PhotoArchivingTools.ViewModels.FileSystemViewModels;
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
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        SameTimePhotosDirs = utility.TargetDirs;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility);
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
        SameTimePhotosDirs = null;
    }

    protected override void ResetImpl()
    {
        SameTimePhotosDirs = null;
    }
}
