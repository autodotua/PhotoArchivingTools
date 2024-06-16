using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class DeleteJpgIfExistRawPanel : UserControl
    {
        public DeleteJpgIfExistRawPanel()
        {
            DataContext = new DeleteJpgIfExistRawViewModel();
            InitializeComponent();
        }
    }
}
