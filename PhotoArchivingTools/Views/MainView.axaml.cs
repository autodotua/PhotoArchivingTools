﻿using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using FzLib.Avalonia.Dialogs;
using FzLib.Avalonia.Messages;
using PhotoArchivingTools.Messages;
using PhotoArchivingTools.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Views;

public partial class MainView : UserControl
{
    private CancellationTokenSource loadingToken = null;

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
        WeakReferenceMessenger.Default.Register<LoadingMessage>(this, (_, m) =>
        {
            if (m.IsVisible)
            {
                loadingToken ??= LoadingOverlay.ShowLoading(this);
            }
            else
            {
                if (loadingToken != null)
                {
                    loadingToken.Cancel();
                    loadingToken = null;
                }
            }
        });
    }
}
