using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class UselessJpgCleanerPanel : UserControl
    {
        public UselessJpgCleanerPanel()
        {
            DataContext = new UselessJpgCleanerViewModel();
            InitializeComponent();
        }
    }
}
