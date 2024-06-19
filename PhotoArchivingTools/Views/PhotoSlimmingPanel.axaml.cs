using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class PhotoSlimmingPanel : UserControl
    {
        public PhotoSlimmingPanel()
        {
            DataContext = new PhotoSlimmingViewModel();
            InitializeComponent();
        }
    }
}
