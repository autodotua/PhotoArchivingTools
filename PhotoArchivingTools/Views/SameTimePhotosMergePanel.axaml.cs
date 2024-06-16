using Avalonia.Controls;
using PhotoArchivingTools.ViewModels;

namespace PhotoArchivingTools.Views
{
    public partial class SameTimePhotosMergePanel : UserControl
    {
        public SameTimePhotosMergePanel()
        {
            DataContext = new SameTimePhotosMergeModel();
            InitializeComponent();
        }
    }
}
