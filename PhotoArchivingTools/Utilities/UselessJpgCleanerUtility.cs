using PhotoArchivingTools.Configs;
using PhotoArchivingTools.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public class UselessJpgCleanerUtility(UselessJpgCleanerConfig config) : UtilityBase
    {
        public UselessJpgCleanerConfig Config { get; init; } = config;

        public List<SimpleFileViewModel> DeletingJpgFiles { get; set; }
        public override Task ExecuteAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(DeletingJpgFiles);
            return Task.Run(() =>
            {
                int index = 0;
                foreach (var file in DeletingJpgFiles)
                {
                    index++;
                    token.ThrowIfCancellationRequested();
                    NotifyProgressUpdate(DeletingJpgFiles.Count, index, $"正在删除JPG（{index}/{DeletingJpgFiles.Count}）");
                    File.Delete(file.Path);
                }
                DeletingJpgFiles = null;
            }, token);
        }

        public override Task InitializeAsync()
        {
            DeletingJpgFiles = new List<SimpleFileViewModel>();
            return Task.Run(() =>
            {
                NotifyProgressUpdate(0, -1, "正在搜索JPG文件");
                var jpgs = Directory
                    .EnumerateFiles(Config.Dir, "*.jp*g", SearchOption.AllDirectories)
                    .Where(p => p.EndsWith(".jpg", StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                int index = 0;
                foreach (var jpg in jpgs)
                {
                    index++;
                    NotifyProgressUpdate(jpgs.Count, index, $"正在查找RAW文件（{index}/{jpgs.Count}）");
                    var rawFile = $"{Path.Combine(Path.GetDirectoryName(jpg), Path.GetFileNameWithoutExtension(jpg))}.{Config.RawExtension}";
                    if (File.Exists(rawFile))
                    {
                        DeletingJpgFiles.Add(new SimpleFileViewModel(jpg));
                    }
                }
            });

        }
    }
}
