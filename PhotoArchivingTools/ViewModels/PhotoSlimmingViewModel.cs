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
using System.Threading;
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

    [ObservableProperty]
    private bool canCancel;

    [ObservableProperty]
    private PhotoSlimmingUtility utility;

    public ObservableCollection<PhotoSlimmingConfig> Configs { get; set; } = new ObservableCollection<PhotoSlimmingConfig>(AppConfig.Instance.PhotoSlimmingConfigs);

    [ObservableProperty]
    private SlimmingFilesInfo compressFiles = new SlimmingFilesInfo();

    [ObservableProperty]
    private SlimmingFilesInfo copyFiles = new SlimmingFilesInfo();

    [ObservableProperty]
    private SlimmingFilesInfo deleteFiles = new SlimmingFilesInfo();

    [ObservableProperty]
    private PhotoSlimmingConfig config;

    [ObservableProperty]
    private double progress;

    [ObservableProperty]
    private string message;

    [ObservableProperty]
    private ObservableCollection<string> errorMessages = new ObservableCollection<string>();

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
        Utility = new PhotoSlimmingUtility(Config);
        await TryRunAsync(async () =>
        {
            await Utility.InitializeAsync();
            Utility.ProgressUpdate += Utility_ProgressUpdate;
            CopyFiles = Utility.CopyFiles;
            CompressFiles = Utility.CompressFiles;
            DeleteFiles = Utility.DeleteFiles;
            ErrorMessages = new ObservableCollection<string>(Utility.ErrorMessages);
        }, "初始化失败");
    }

    private void Utility_ProgressUpdate(object sender, ProgressUpdateEventArgs<int> e)
    {
        Progress = 1.0 * e.Current / e.Maximum;
        Message = e.Message;
    }



    [RelayCommand(IncludeCancelCommand = true)]
    private Task ExecuteAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(Utility, nameof(Utility));
        return TryRunAsync(async () =>
        {
            await Utility.ExecuteAsync(token);
            Utility.ProgressUpdate -= Utility_ProgressUpdate;
            ErrorMessages = new ObservableCollection<string>(Utility.ErrorMessages);
            Utility = null;
            CopyFiles = new();
            CompressFiles = new();
            DeleteFiles = new();
        }, "执行失败");

    }

    [RelayCommand]
    private void Cancel()
    {
        ExecuteCommand.Cancel();
    }
}
