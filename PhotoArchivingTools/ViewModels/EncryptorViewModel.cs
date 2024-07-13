using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Messages;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Utilities;
using PhotoArchivingTools.ViewModels.FileSystemViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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

    public CipherMode[] CipherModes => Enum.GetValues<CipherMode>();

    public PaddingMode[] PaddingModes => Enum.GetValues<PaddingMode>();


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
        if (ProcessingFiles.Any(p => p.Error != null))
        {
            string typeDesc = IsEncrypting ? "加密" : "解密";
            var errorDetails = ProcessingFiles.Where(p => p.Error != null).Select(p => $"{p.Name}：{p.Error.Message}");
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = $"{typeDesc}存在错误",
                Message = $"{typeDesc}过程已结束，部分文件{typeDesc}失败，请检查",
                Detail = string.Join(Environment.NewLine, errorDetails)
            });
        }
    }

    [RelayCommand]
    private async Task CopyErrorAsync(Exception exception)
    {
        await WeakReferenceMessenger.Default.Send(new GetClipboardMessage())
              .Clipboard
              .SetTextAsync(exception.ToString());
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
