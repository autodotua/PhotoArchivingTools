using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;

namespace PhotoArchivingTools.Controls
{
    public partial class FilePickerTextBox : UserControl
    {
        public FilePickerTextBox()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<string> LabelProperty =
            AvaloniaProperty.Register<FilePickerTextBox, string>(nameof(Label));

        public string Label
        {
            get => GetValue(LabelProperty);
            set => SetValue(LabelProperty, value);
        }

        public static readonly StyledProperty<string> FileNamesProperty =
            AvaloniaProperty.Register<FilePickerTextBox, string>(nameof(FileNames), defaultBindingMode: BindingMode.TwoWay);

        public string FileNames
        {
            get => GetValue(FileNamesProperty);
            set => SetValue(FileNamesProperty, value);
        }

        public static readonly StyledProperty<object> ButtonContentProperty =
            AvaloniaProperty.Register<FilePickerTextBox, object>(nameof(ButtonContent), "ä¯ÀÀ..");

        public object ButtonContent
        {
            get => GetValue(ButtonContentProperty);
            set => SetValue(ButtonContentProperty, value);
        }

        public PickerType Type { get; set; } = PickerType.OpenFile;
        public string Title { get; set; }
        public bool AllowMultiple { get; set; }
        public bool? ShowOverwritePrompt { get; set; }
        public List<FilePickerFileType> FileTypeFilter { get; set; }
        public string MultipleFilesSeparator { get; set; } = "; ";
        public string SaveFileDefaultExtension { get; set; }
        public string SaveFileSuggestedFileName { get; set; }
        public string SuggestedStartLocation { get; set; }

        private async void Button_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var storageProvider = TopLevel.GetTopLevel(this).StorageProvider;

            switch (Type)
            {
                case PickerType.OpenFile:
                    var openFiles = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
                    {
                        AllowMultiple = AllowMultiple,
                        FileTypeFilter = FileTypeFilter,
                        Title = Title,
                        SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(SuggestedStartLocation)
                    });
                    if (openFiles != null && openFiles.Count > 0)
                    {
                        FileNames = string.Join(MultipleFilesSeparator, openFiles.Select(p => p.TryGetLocalPath()));
                    }
                    break;
                case PickerType.OpenFolder:
                    var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
                    {
                        Title = Title,
                        AllowMultiple = AllowMultiple,
                        SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(SuggestedStartLocation),
                    });
                    if (folders != null && folders.Count > 0)
                    {
                        FileNames = string.Join(MultipleFilesSeparator, folders.Select(p => p.TryGetLocalPath()));
                    }
                    break;
                case PickerType.SaveFile:
                    var saveFiles = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
                    {
                        Title = Title,
                        FileTypeChoices = FileTypeFilter,
                        DefaultExtension = SaveFileDefaultExtension,
                        ShowOverwritePrompt = ShowOverwritePrompt,
                        SuggestedFileName = SaveFileSuggestedFileName,
                        SuggestedStartLocation = await storageProvider.TryGetFolderFromPathAsync(SuggestedStartLocation),
                    });
                    if (saveFiles != null)
                    {
                        FileNames = saveFiles.TryGetLocalPath();
                    }
                    break;
            }
        }

        public enum PickerType
        {
            OpenFile,
            OpenFolder,
            SaveFile
        }
    }
}
