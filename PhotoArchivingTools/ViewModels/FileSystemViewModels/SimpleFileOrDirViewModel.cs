using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace PhotoArchivingTools.ViewModels
{
    public partial class SimpleFileOrDirViewModel : ObservableObject
    {
        public SimpleFileOrDirViewModel()
        {
        }

        public SimpleFileOrDirViewModel(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            Name = System.IO.Path.GetFileName(path);
            Path = path;
        }

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string path;
    }
}
