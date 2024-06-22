using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Cryptography;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class RepairModifiedTimeModel : ViewModelBase
{
    private RepairModifiedTimeUtility utility;

    public RepairModifiedTimeConfig Config { get; set; } = AppConfig.Instance.RepairModifiedTimeConfig;

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<string> updatingFiles;

    [ObservableProperty]
    private List<string> errorFiles;

    protected override async Task InitializeImplAsync()
    {
        Config.Dir = Dir;
        utility = new RepairModifiedTimeUtility(Config);
        await utility.InitializeAsync();
        UpdatingFiles = utility.UpdatingFilesAndMessages;
        ErrorFiles = utility.ErrorFilesAndMessages;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility, nameof(utility));
        await utility.ExecuteAsync();
        UpdatingFiles = null;
        ErrorFiles = utility.ErrorFilesAndMessages;
        utility = null;
    }
}
