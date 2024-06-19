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

public partial class PhotoSlimmingModel : ViewModelBase
{
    private PhotoSlimmingUtility utility;

    public PhotoSlimmingConfig Config { get; set; } = new PhotoSlimmingConfig();

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Config.Dir = GetDir();
        utility = new PhotoSlimmingUtility(Config);
        await TryRunAsync(async () =>
        {
            //await repairModifiedTimeUtility.InitializeAsync();
            //UpdatingFiles = repairModifiedTimeUtility.UpdatingFilesAndMessages;
            //ErrorFiles = repairModifiedTimeUtility.ErrorFilesAndMessages;
        }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(utility, nameof(utility));
        return TryRunAsync(async () =>
        {
            //await repairModifiedTimeUtility.ExecuteAsync();
            //UpdatingFiles = null;
            //ErrorFiles = repairModifiedTimeUtility.ErrorFilesAndMessages;
            //repairModifiedTimeUtility = null;
        }, "执行失败");

    }
}
