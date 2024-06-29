using Avalonia;
using Avalonia.Controls;

namespace PhotoArchivingTools.Views
{
    public partial class PanelBase : UserControl
    {
        public PanelBase()
        {
            InitializeComponent();
        }


        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<PanelBase, string>(nameof(Description));

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }


        public static readonly StyledProperty<object> ConfigsContentProperty =
            AvaloniaProperty.Register<PanelBase, object>(nameof(ConfigsContent));

        public object ConfigsContent
        {
            get => GetValue(ConfigsContentProperty);
            set => SetValue(ConfigsContentProperty, value);
        }


        public static readonly StyledProperty<object> ResultsContentProperty =
            AvaloniaProperty.Register<PanelBase, object>(nameof(ResultsContent));

        public object ResultsContent
        {
            get => GetValue(ResultsContentProperty);
            set => SetValue(ResultsContentProperty, value);
        }




    }
}
