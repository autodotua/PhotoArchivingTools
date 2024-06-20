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

    protected override async Task InitializeImplAsync()
    {
        utility = new PhotoSlimmingUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        Message = "正在生成统计信息";
        await utility.CopyFiles.CreateRelativePathsAsync();
        await utility.CompressFiles.CreateRelativePathsAsync();
        await utility.DeleteFiles.CreateRelativePathsAsync();
        CopyFiles = utility.CopyFiles;
        CompressFiles = utility.CompressFiles;
        DeleteFiles = utility.DeleteFiles;
        ErrorMessages = new ObservableCollection<string>(utility.ErrorMessages);
        Message = "就绪";
    }

    private void Utility_ProgressUpdate(object sender, ProgressUpdateEventArgs<int> e)
    {
        Progress = 1.0 * e.Current / e.Maximum;
        Message = e.Message;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        ErrorMessages = new ObservableCollection<string>(utility.ErrorMessages);
        CopyFiles = new();
        CompressFiles = new();
        DeleteFiles = new();
    }
}
