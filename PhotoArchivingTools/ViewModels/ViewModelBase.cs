using CommunityToolkit.Mvvm.ComponentModel;

namespace PhotoArchivingTools.ViewModels;

public partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool isEnable = true;
}
