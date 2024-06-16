using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PhotoArchivingTools.ViewModels
{
    public class SimpleDirViewModel : SimpleFileOrDirViewModel
    {
        public SimpleDirViewModel()
        {

        }
        public SimpleDirViewModel(string path) : base(path)
        {
            var subFiles = Directory.EnumerateFiles(path).ToList();
            Subs = subFiles.Select(p => new SimpleFileOrDirViewModel(p)).ToList();
            FilesCount = subFiles.Count;
            if (FilesCount > 0)
            {
                EarliestTime = new DateTime(subFiles
                    .Select(File.GetLastWriteTime)
                    .Select(p => p.Ticks)
                    .Min());
                LatestTime = new DateTime(subFiles
                    .Select(File.GetLastWriteTime)
                    .Select(p => p.Ticks)
                    .Max());
            }
        }
        public int FilesCount { get; set; }
        public DateTime EarliestTime { get; set; }
        public DateTime LatestTime { get; set; }
        public List<SimpleFileOrDirViewModel> Subs { get; set; } = new List<SimpleFileOrDirViewModel>();
    }
}
