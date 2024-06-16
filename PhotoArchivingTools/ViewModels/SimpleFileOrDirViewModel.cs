using System;

namespace PhotoArchivingTools.ViewModels
{
    public class SimpleFileOrDirViewModel
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

        public string Name { get; set; }
        public string Path { get; set; }
    }
}
