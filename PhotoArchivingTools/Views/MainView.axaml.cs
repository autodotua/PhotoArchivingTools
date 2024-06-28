using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;
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
        this.RegisterDialogHostMessage();
        this.RegisterGetClipboardMessage();
        this.RegisterGetStorageProviderMessage();
        this.RegisterCommonDialogMessage();
    }

}
