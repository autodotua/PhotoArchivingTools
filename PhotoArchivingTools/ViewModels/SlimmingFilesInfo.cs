using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoArchivingTools.ViewModels
{
    public class SlimmingFilesInfo
    {
        private List<FileInfo> processingFiles = new List<FileInfo>();

        private IList<string> processingFilesRelativePaths;

        private string rootDir;

        private List<FileInfo> skippedFiles = new List<FileInfo>();

        public SlimmingFilesInfo(string rootDir)
        {
            this.rootDir = rootDir;
        }
        public IReadOnlyList<FileInfo> ProcessingFiles => processingFiles.AsReadOnly();

        public long ProcessingFilesLength { get; private set; } = 0;

        public IList<string> ProcessingFilesRelativePaths => processingFilesRelativePaths ?? throw new Exception($"还未调用{nameof(CreateRelativePathsAsync)}方法");

        public IReadOnlyList<FileInfo> SkippedFiles => skippedFiles.AsReadOnly();

        public void Add(FileInfo file)
        {
            processingFiles.Add(file);
            if (file.Exists)
            {
                ProcessingFilesLength += file.Length;
            }
        }

        public void AddSkipped(FileInfo file)
        {
            skippedFiles.Add(file);
        }

        public void Clear()
        {
            processingFiles = null;
            skippedFiles = null;
            ProcessingFilesLength = 0;
        }

        public Task CreateRelativePathsAsync()
        {
            return Task.Run(() =>
            {
                processingFilesRelativePaths = processingFiles
                .Select(p =>
                {
                    return Path.GetRelativePath(rootDir, p.FullName) + (p.Exists ? "" : "/");
                })
               .ToList();
            });
        }
    }
}
