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
    private EncryptorUtility utility;

    public EncryptorConfig Config { get; set; } = AppConfig.Instance.EncryptorConfig;

    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<EncryptorFileViewModel> processingFiles;

    protected override async Task InitializeImplAsync()
    {
        utility = new EncryptorUtility(Config);
        utility.ProgressUpdate += Utility_ProgressUpdate;
        await utility.InitializeAsync();
        ProcessingFiles = utility.ProcessingFiles;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(utility);
        await utility.ExecuteAsync(token);
        utility.ProgressUpdate -= Utility_ProgressUpdate;
        utility = null;
        ProcessingFiles = null;
    }

    protected override void ResetImpl()
    {
        ProcessingFiles = null;
    }
}
