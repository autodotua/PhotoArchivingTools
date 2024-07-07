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
public partial class EncryptorViewModel : ViewModelBase
{
    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private bool isEncrypting = true;

    [ObservableProperty]
    private List<EncryptorFileViewModel> processingFiles;

    private EncryptorUtility utility;

    public EncryptorViewModel()
    {
        (App.Current as App).Exit += (s, e) =>
        {
            if (!Config.RememberPassword)
            {
                Config.Password = null;
            }
        };
    }

    public EncryptorConfig Config { get; set; } = AppConfig.Instance.EncryptorConfig;
    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility);
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
    }

    protected override async Task InitializeImplAsync()
    {
        if (string.IsNullOrEmpty(Config.Password))
        {
            throw new ArgumentException("密码为空");
        }
        Config.Type = IsEncrypting ? EncryptorConfig.EncryptorTaskType.Encrypt : EncryptorConfig.EncryptorTaskType.Decrypt;
        utility = new EncryptorUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        ProcessingFiles = utility.ProcessingFiles;
    }
    protected override void ResetImpl()
    {
        ProcessingFiles = null;
    }
}
