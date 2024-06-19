using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Cryptography;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class RepairModifiedTimeModel : ViewModelBase
{
   private RepairModifiedTimeUtility utility;

    public RepairModifiedTimeConfig Config { get; set; } =AppConfig.Instance.RepairModifiedTimeConfig;

    [ObservableProperty]
    private List<string> updatingFiles;

    [ObservableProperty]
    private List<string> errorFiles;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Config.Dir = GetDir();
        utility = new RepairModifiedTimeUtility(Config);
        await TryRunAsync(async () =>
        {
            await utility.InitializeAsync();
            UpdatingFiles = utility.UpdatingFilesAndMessages;
            ErrorFiles = utility.ErrorFilesAndMessages;
        }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(utility, nameof(utility));
        return TryRunAsync(async () =>
        {
            await utility.ExecuteAsync();
            UpdatingFiles = null;
            ErrorFiles = utility.ErrorFilesAndMessages;
            utility = null;
        }, "执行失败");

    }
}
