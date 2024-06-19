using CommunityToolkit.Mvvm.ComponentModel;
using Mapster;
using PhotoArchivingTools.Configs;

namespace PhotoArchivingTools.ViewModels;

public partial class PhotoSlimmingConfigViewModel : ViewModelBase
{
    [ObservableProperty]
    private PhotoSlimmingConfig config = new PhotoSlimmingConfig();

    public PhotoSlimmingConfigViewModel(PhotoSlimmingConfig config)
    {
        config.Adapt(Config);
    }
    public PhotoSlimmingConfigViewModel()
    {
    }
}
