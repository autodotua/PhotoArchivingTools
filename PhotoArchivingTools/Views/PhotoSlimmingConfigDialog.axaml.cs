using Avalonia.Controls;
using FzLib.Avalonia.Dialogs;
using PhotoArchivingTools.Configs;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class PhotoSlimmingConfigDialog : DialogHost
    {
        public PhotoSlimmingConfigDialog()
        {
            DataContext = new PhotoSlimmingConfigViewModel();
            InitializeComponent();
        }

        public PhotoSlimmingConfigDialog(PhotoSlimmingConfig config)
        {
            DataContext = new PhotoSlimmingConfigViewModel(config);
            InitializeComponent();
        }

        protected override void OnCloseButtonClick()
        {
            Close();
        }

        protected override void OnPrimaryButtonClick()
        {
            Close((DataContext as PhotoSlimmingConfigViewModel).Config);
        }
    }
}
