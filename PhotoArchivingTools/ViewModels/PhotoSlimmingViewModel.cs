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
    public PhotoSlimmingViewModel()
    {
        (App.Current as App).Exit += (s, e) =>
        {
            AppConfig.Instance.PhotoSlimmingConfigs = new List<PhotoSlimmingConfig>(Configs);
        };
    }

    private PhotoSlimmingUtility utility;

    public ObservableCollection<PhotoSlimmingConfig> Configs { get; set; } = new ObservableCollection<PhotoSlimmingConfig>(AppConfig.Instance.PhotoSlimmingConfigs);

    [ObservableProperty]
    private SlimmingFilesInfo compressFiles;

    [ObservableProperty]
    private SlimmingFilesInfo copyFiles;

    [ObservableProperty]
    private SlimmingFilesInfo deleteFiles;

    [ObservableProperty]
    private PhotoSlimmingConfig config;

    [ObservableProperty]
    private ObservableCollection<string> errorMessages;

    [RelayCommand]
    private async Task CreateAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog());
        WeakReferenceMessenger.Default.Send(message);
        var result = await message.Task;
        if (result is PhotoSlimmingConfig config)
        {
            Configs.Add(config);
        }
    }

    [RelayCommand]
    private async Task EditAsync()
    {
        var message = new DialogHostMessage(new PhotoSlimmingConfigDialog(Config));
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
    private async Task InitializeAsync()
    {
        utility = new PhotoSlimmingUtility(Config);
        await TryRunAsync(async () =>
        {
            await utility.InitializeAsync();
            CopyFiles = utility.CopyFiles;
            CompressFiles = utility.CompressFiles;
            DeleteFiles = utility.DeleteFiles;
            ErrorMessages = new ObservableCollection<string>(utility.ErrorMessages);
        }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        ArgumentNullException.ThrowIfNull(utility, nameof(utility));
        return TryRunAsync(async () =>
        {
            await utility.ExecuteAsync();
            ErrorMessages = new ObservableCollection<string>(utility.ErrorMessages);
            utility = null;
            CopyFiles = null;
            CompressFiles = null;
            DeleteFiles = null;
        }, "执行失败");

    }
}
