using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using PhotoArchivingTools.Messages;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace PhotoArchivingTools.ViewModels;
public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        WeakReferenceMessenger.Default.Register<GetDirMessage>(this, (_, m) =>
        {
            m.Dir = Dir;
        });
    }

    [ObservableProperty]
    private string dir;


}
