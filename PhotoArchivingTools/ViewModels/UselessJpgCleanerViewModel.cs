using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class UselessJpgCleanerViewModel : ViewModelBase
{
    private UselessJpgCleanerUtility utility;

    public UselessJpgCleanerConfig Config { get; set; } = AppConfig.Instance.UselessJpgCleanerConfig;
   
    [ObservableProperty]
    private string dir;

    [ObservableProperty]
    private List<SimpleFileViewModel> deletingJpgFiles;

    protected override async Task InitializeImplAsync()
    {
        Config.Dir = Dir;
        utility = new UselessJpgCleanerUtility(Config);
        await utility.InitializeAsync();
        DeletingJpgFiles = utility.DeletingJpgFiles;
    }

    protected override async Task ExecuteImplAsync(CancellationToken token)
    {
        await utility.ExecuteAsync();
        utility = null;
        DeletingJpgFiles = null;
    }
}
