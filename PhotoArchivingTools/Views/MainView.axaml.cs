using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using FrpGUI.Avalonia.Messages;
using FzLib.Avalonia.Dialogs;
using PhotoArchivingTools.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoArchivingTools.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
        RegisterMessages();
    }

    private void RegisterMessages()
    {
        WeakReferenceMessenger.Default.Register<DialogHostMessage>(this, async (_, m) =>
        {
            try
            {
                var result = await m.Dialog.ShowDialog<object>(DialogContainerType.PopupPreferred, TopLevel.GetTopLevel(this));
                m.SetResult(result);
            }
            catch (Exception ex)
            {
                m.SetException(ex);
            }
        });



        WeakReferenceMessenger.Default.Register<CommonDialogMessage>(this, async (_, m) =>
        {
            try
            {
                object result = null;
                switch (m.Type)
                {
                    case CommonDialogMessage.CommonDialogType.Ok:
                        await this.ShowOkDialogAsync(m.Title, m.Message, m.Detail);
                        break;
                    case CommonDialogMessage.CommonDialogType.Error:
                        if (m.Exception == null)
                        {
                            result = await this.ShowErrorDialogAsync(m.Title, m.Message, m.Detail);
                        }
                        else
                        {
                            result = await this.ShowErrorDialogAsync(m.Title, m.Exception);
                        }
                        break;
                    case CommonDialogMessage.CommonDialogType.YesNo:
                        result = await this.ShowYesNoDialogAsync(m.Title, m.Message, m.Detail);
                        break;
                }
                m.SetResult(result);
            }
            catch (Exception ex)
            {
                m.SetException(ex);
            }
        });
    }

}
