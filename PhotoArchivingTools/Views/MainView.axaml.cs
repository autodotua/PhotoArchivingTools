using Avalonia.Controls;
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

    private void ToolItemBox_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        ToolPanelInfo panelInfo = (sender as UserControl).DataContext as ToolPanelInfo;
        if (panelInfo.PanelInstance == null)
        {
            panelInfo.PanelInstance = Activator.CreateInstance(panelInfo.PanelType) as PanelBase;
            panelInfo.PanelInstance.RequestClosing += (s, e) =>
            {

                (DataContext as MainViewModel).MainContent = null;
            };
        }
        PanelBase panel = panelInfo.PanelInstance;
        panel.Title = panelInfo.Title;
        panel.Description = panelInfo.Description;
        (DataContext as MainViewModel).MainContent = panel;
    }
}
