using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Cryptography;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class RepairModifiedTimeModel : ViewModelBase
{
    RepairModifiedTimeUtility repairModifiedTimeUtility;

    [ObservableProperty]
    private int threadCount;

    [ObservableProperty]
    private int maxDurationToleranceSeconds;

    [ObservableProperty]
    private List<string> updatingFiles;

    [ObservableProperty]
    private List<string> errorFiles;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        repairModifiedTimeUtility = new RepairModifiedTimeUtility()
        {
            Dir = WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir,
            MaxDurationTolerance = TimeSpan.FromSeconds(MaxDurationToleranceSeconds),
            ThreadCount = ThreadCount,
        };
        await TryRunAsync(async () =>
        {
            await repairModifiedTimeUtility.InitializeAsync();
            UpdatingFiles = repairModifiedTimeUtility.UpdatingFilesAndMessages;
            ErrorFiles = repairModifiedTimeUtility.ErrorFilesAndMessages;
        }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(repairModifiedTimeUtility, nameof(repairModifiedTimeUtility));
        return TryRunAsync(async () =>
        {
            await repairModifiedTimeUtility.ExecuteAsync();
            UpdatingFiles = null;
            ErrorFiles = repairModifiedTimeUtility.ErrorFilesAndMessages;
            repairModifiedTimeUtility = null;
        }, "执行失败");

    }
}
