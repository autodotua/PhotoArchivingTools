using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using PhotoArchivingTools.Messages;
using System;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isEnable = true;

    protected string GetDir()
    {
        return WeakReferenceMessenger.Default.Send(new GetDirMessage()).Dir;
    }

    protected async Task TryRunAsync(Func<Task> action, string errorTitle)
    {
        try
        {
            await action();
        }
        catch(Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Ok,
                Title = "操作已取消",
                Message = "操作已取消",
                Detail = ex.ToString()
            });
        }
        catch (Exception ex)
        {
            WeakReferenceMessenger.Default.Send(new CommonDialogMessage()
            {
                Type = CommonDialogMessage.CommonDialogType.Error,
                Title = errorTitle,
                Exception = ex
            });
        }
    }
}
