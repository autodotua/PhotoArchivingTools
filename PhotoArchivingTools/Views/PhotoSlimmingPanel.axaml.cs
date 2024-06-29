using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class PhotoSlimmingPanel : PanelBase
    {
        public PhotoSlimmingPanel()
        {
            DataContext = new PhotoSlimmingViewModel();
            InitializeComponent();
        }
    }
}
