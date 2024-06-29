using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using FzLib.Cryptography;
using Mapster;
using PhotoArchivingTools.Configs;
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
    [ObservableProperty]
    private bool canCancel;

    [ObservableProperty]
    private SlimmingFilesInfo compressFiles;

    [ObservableProperty]
    private PhotoSlimmingConfig config;

    [ObservableProperty]
    private SlimmingFilesInfo copyFiles;

    [ObservableProperty]
    private SlimmingFilesInfo deleteFiles;

    [ObservableProperty]
    private ObservableCollection<string> errorMessages;

    private PhotoSlimmingUtility utility;

    public PhotoSlimmingViewModel()
    {
        Configs = new ObservableCollection<PhotoSlimmingConfig>(AppConfig.Instance.PhotoSlimmingConfigs);
        if (Configs.Count > 0)
        {
            Config = Configs[0];
        }
        (App.Current as App).Exit += (s, e) =>
        {
            AppConfig.Instance.PhotoSlimmingConfigs = new List<PhotoSlimmingConfig>(Configs);
        };
    }
    public ObservableCollection<PhotoSlimmingConfig> Configs { get; set; }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        ErrorMessages = new ObservableCollection<string>(utility.ErrorMessages);
        CopyFiles = null;
        CompressFiles = null;
        DeleteFiles = null;
    }

    protected override async Task InitializeImplAsync()
    {
        if (Config == null)
        {
            throw new ArgumentException("请先选择配置");
        }
        utility = new PhotoSlimmingUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        Progress = -1;
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

    protected override void ResetImpl()
    {
        CopyFiles = null;
        CompressFiles = null;
        DeleteFiles = null;
        ErrorMessages = null;
    }

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
    private void Clone()
    {
        var newObj = Config.Adapt<PhotoSlimmingConfig>();
        newObj.Name += "（复制）";
        Configs.Add(newObj);
    }

}
