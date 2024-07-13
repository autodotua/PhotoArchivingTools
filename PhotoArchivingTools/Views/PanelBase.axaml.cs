using Avalonia;
using Avalonia.Controls;
using System;

namespace PhotoArchivingTools.Views
{
    public partial class PanelBase : UserControl
    {
        public static readonly StyledProperty<object> ConfigsContentProperty =
            AvaloniaProperty.Register<PanelBase, object>(nameof(ConfigsContent));

        public static readonly StyledProperty<string> DescriptionProperty =
            AvaloniaProperty.Register<PanelBase, string>(nameof(Description));

        public static readonly StyledProperty<object> ResultsContentProperty =
            AvaloniaProperty.Register<PanelBase, object>(nameof(ResultsContent));

        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<PanelBase, string>(nameof(Title));

        public PanelBase()
        {
            InitializeComponent();
        }
        public object ConfigsContent
        {
            get => GetValue(ConfigsContentProperty);
            set => SetValue(ConfigsContentProperty, value);
        }

        public string Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public object ResultsContent
        {
            get => GetValue(ResultsContentProperty);
            set => SetValue(ResultsContentProperty, value);
        }

        public string Title
        {
            get => this.GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private void ReturnButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            RequestClosing?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler RequestClosing;
    }
}
