using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using FzLib.Cryptography;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using PhotoArchivingTools.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;
public partial class PhotoSlimmingViewModel : ViewModelBase
{
    private PhotoSlimmingUtility utility;

    public ObservableCollection<PhotoSlimmingConfig> Configs { get; set; } = new ObservableCollection<PhotoSlimmingConfig>() { new PhotoSlimmingConfig() };

    [ObservableProperty]
    private PhotoSlimmingConfig config;

    [RelayCommand]
    private async Task CreateAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog()) ;
        WeakReferenceMessenger.Default.Send(message);
        var result = await message.Task;
        if(result is PhotoSlimmingConfig config)
        {
            Configs.Add(config);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog(Config)) ;
        WeakReferenceMessenger.Default.Send(message);
        var result = await message.Task;
        if (result is PhotoSlimmingConfig config)
        {
            Configs[Configs.IndexOf(Config)] = config;
            Config = config;
        }
    }

    [RelayCommand]
    private void Remove()
    {
        Configs.Remove(Config);
    }

    [RelayCommand]
    private async Task InitializeAsync(bool copy)
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
