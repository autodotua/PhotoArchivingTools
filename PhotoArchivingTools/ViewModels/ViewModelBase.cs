﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{

    [ObservableProperty]
    private bool canInitialize = true;

    [ObservableProperty]
    private bool canReset = false;

    [ObservableProperty]
    private bool canExecute = false;

    [ObservableProperty]
    private bool isEnable = true;

    protected static string GetDir()
    {
        return WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir;
    }

    protected abstract Task ExecuteImplAsync(CancellationToken token);

    protected abstract Task InitializeImplAsync();

    private static async Task<bool> TryRunAsync(Func<Task> action, string errorTitle)
    {
        try
        {
            await action();
            return true;
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Title = "操作已取消",
                Message = "操作已取消",
                Detail = ex.ToString()
            });
            return false;
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = errorTitle,
                Exception = ex
            });
            return false;
        }
    }

    [RelayCommand]
    private void CancelExecute()
    {
        ExecuteCommand.Cancel();
    }

    [RelayCommand(IncludeCancelCommand = true, CanExecute = nameof(CanExecute))]
    private async Task ExecuteAsync(CancellationToken token)
    {
        CanExecute = false;

        if (await TryRunAsync(() => ExecuteImplAsync(token), "执行失败"))
        {

        }
        else
        {
            CanExecute = true;
        }
    }

    [RelayCommand(CanExecute = nameof(CanInitialize))]
    private async Task InitializeAsync()
    {
        CanInitialize = false;
        InitializeCommand.NotifyCanExecuteChanged();
        CanReset = false;
        ResetCommand.NotifyCanExecuteChanged();
        if (await TryRunAsync(InitializeImplAsync, "初始化失败"))
        {
            CanExecute = true;
            CanReset = true;
        }
        else
        {
            CanExecute = false;
            CanReset = false;
            CanInitialize = true;
        }
        ExecuteCommand.NotifyCanExecuteChanged();
        ResetCommand.NotifyCanExecuteChanged();
        InitializeCommand.NotifyCanExecuteChanged();
    }
    [RelayCommand(CanExecute = nameof(CanReset))]
    private void Reset()
    {
        CanReset = false;
        CanInitialize = true;
        CanExecute = false;

        ResetCommand.NotifyCanExecuteChanged();
        ExecuteCommand.NotifyCanExecuteChanged();
        InitializeCommand.NotifyCanExecuteChanged();
    }
}
