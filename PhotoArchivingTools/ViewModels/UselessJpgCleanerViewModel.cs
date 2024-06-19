using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public partial class UselessJpgCleanerViewModel : ViewModelBase
{
    private UselessJpgCleanerUtility utility;

    public UselessJpgCleanerConfig Config { get; set; } = new UselessJpgCleanerConfig();

    [ObservableProperty]
    private List<SimpleFileViewModel> deletingJpgFiles;

    [RelayCommand]
    private async Task InitializeAsync()
    {
        Config.Dir = GetDir();
        utility = new UselessJpgCleanerUtility(Config);
        await TryRunAsync(async () =>
       {
           await utility.InitializeAsync();
           DeletingJpgFiles = utility.DeletingJpgFiles;
       }, "初始化失败");
    }

    [RelayCommand]
    private Task ExecuteAsync()
    {
        return TryRunAsync(async () =>
        {
            await utility.ExecuteAsync();
            utility = null;
            DeletingJpgFiles = null;
        }, "执行失败");

    }
}
