using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using System;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels;

public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isEnable = true;

    protected async Task TryRunAsync(Func<Task> action, string errorTitle)
    {
        try
        {
            await action();
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
