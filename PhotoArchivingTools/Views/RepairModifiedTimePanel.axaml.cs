using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class RepairModifiedTimePanel : PanelBase
    {
        public RepairModifiedTimePanel()
        {
            DataContext = new RepairModifiedTimeModel();
            InitializeComponent();
        }
    }
}
