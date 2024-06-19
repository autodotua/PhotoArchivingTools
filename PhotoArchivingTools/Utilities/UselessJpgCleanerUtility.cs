using PhotoArchivingTools.Configs;
using PhotoArchivingTools.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public class UselessJpgCleanerUtility(UselessJpgCleanerConfig config) : UtilityBase
    {
        public UselessJpgCleanerConfig Config { get; init; } = config;

        public List<SimpleFileViewModel> DeletingJpgFiles { get; set; }
        public override Task ExecuteAsync()
        {
            ArgumentNullException.ThrowIfNull(DeletingJpgFiles);
            return Task.Run(() =>
            {
                foreach (var file in DeletingJpgFiles)
                {
                    File.Delete(file.Path);
                }
                DeletingJpgFiles = null;
            });
        }

        public override Task InitializeAsync()
        {
            DeletingJpgFiles = new List<SimpleFileViewModel>();
            return Task.Run(() =>
            {
                var jpgs = Directory
                    .EnumerateFiles(Config.Dir, "*.jp*g", SearchOption.AllDirectories)
                    .Where(p => p.EndsWith(".jpg", System.StringComparison.InvariantCultureIgnoreCase) || p.EndsWith(".jpeg", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                foreach (var jpg in jpgs)
                {
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
