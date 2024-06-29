using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class TimeClassifyPanel : PanelBase
    {
        public TimeClassifyPanel()
        {
            DataContext = new TimeClassifyViewModel();
            InitializeComponent();
        }
    }
}
