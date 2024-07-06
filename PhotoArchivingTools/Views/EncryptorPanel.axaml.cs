using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class EncryptorPanel : PanelBase
    {
        public EncryptorPanel()
        {
            DataContext = new EncryptorViewModel();
            InitializeComponent();
        }
    }
}
