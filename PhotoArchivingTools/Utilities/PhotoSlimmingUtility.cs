using MetadataExtractor;
using PhotoArchivingTools.Configs;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Directory = System.IO.Directory;
using ImageMagick;

namespace PhotoArchivingTools.Utilities
{
    public class PhotoSlimmingUtility : UtilityBase
    {
        private readonly Regex rBlack;
        private readonly Regex rCompress;
        private readonly Regex rCopy;
        private readonly Regex rWhite;
        public PhotoSlimmingUtility(PhotoSlimmingConfig config)
        {
            Config = config;
            rCopy = new Regex(@$"\.({string.Join('|', Config.CopyDirectlyExtensions)})$", RegexOptions.IgnoreCase);
            rCompress = new Regex(@$"\.({string.Join('|', Config.CompressExtensions)})$", RegexOptions.IgnoreCase);
            rBlack = new Regex(Config.BlackList);
            rWhite = new Regex(string.IsNullOrWhiteSpace(Config.WhiteList) ? ".*" : Config.WhiteList, RegexOptions.IgnoreCase);
        }

        public enum TaskType
        {
            Compress,
            Copy,
            Delete
        }
        public SlimmingFilesInfo CompressFiles { get; private set; }

        public PhotoSlimmingConfig Config { get; set; }

        public SlimmingFilesInfo CopyFiles { get; private set; }

        public SlimmingFilesInfo DeleteFiles { get; private set; }

        private ConcurrentBag<string> errorMessages;

        public IReadOnlyCollection<string> ErrorMessages => errorMessages;

        public override Task ExecuteAsync()
        {
            return Task.Run(() =>
            {
                if (Config.ClearAllBeforeRunning)
                {
                    if (Directory.Exists(Config.DistDir))
                    {
                        Directory.Delete(Config.DistDir, true);
                    }
                }
                if (!Directory.Exists(Config.DistDir))
                {
                    Directory.CreateDirectory(Config.DistDir);
                }


                Compress();

                Copy();

                Clear();
            });
        }

        public override Task InitializeAsync()
        {
            CompressFiles = new SlimmingFilesInfo(Config.SourceDir);
            CopyFiles = new SlimmingFilesInfo(Config.SourceDir);
            DeleteFiles = new SlimmingFilesInfo(Config.SourceDir);
            errorMessages = new ConcurrentBag<string>();

            return Task.Run(() =>
            {
                foreach (var file in new DirectoryInfo(Config.SourceDir).EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    if (rBlack.IsMatch(file.FullName)
                        || !rWhite.IsMatch(Path.GetFileNameWithoutExtension(file.Name)))
                    {
                        continue;
                    }

                    if (rCompress.IsMatch(file.Name))
                    {
                        if (NeedProcess(TaskType.Compress, file))
                        {
                            CompressFiles.Add(file);
                        }
                        else
                        {
                            CompressFiles.AddSkipped(file);
                        }
                    }
                    else if (rCopy.IsMatch(file.Name))
                    {
                        if (NeedProcess(TaskType.Copy, file))
                        {
                            CopyFiles.Add(file);
                        }
                        else
                        {
                            CopyFiles.AddSkipped(file);
                        }
                    }

                }


                var desiredDistFiles = CopyFiles.SkippedFiles
                    .Select(file => GetDistPath(file.FullName, null, out _))
                     .Concat(CompressFiles.SkippedFiles
                        .Select(file => GetDistPath(file.FullName, Config.OutputFormat, out _)))
                     .ToHashSet();


                if (File.Exists(Config.DistDir))
                {
                    foreach (var file in Directory
                    .EnumerateFiles(Config.DistDir, "*", SearchOption.AllDirectories)
                     .Where(p => !rBlack.IsMatch(p)))
                    {
                        if (!desiredDistFiles.Contains(file))
                        {
                            DeleteFiles.Add(new FileInfo(file));
                        }
                    }
                }

            });
        }
        private void Compress()
        {
            if (Config.Thread != 1)
            {
                if (Config.Thread <= 0)
                {
                    Parallel.ForEach(CompressFiles.ProcessingFiles, CompressSingle);
                }
                else
                {
                    Parallel.ForEach(CompressFiles.ProcessingFiles, new ParallelOptions() { MaxDegreeOfParallelism = Config.Thread }, CompressSingle);
                }
            }
            else
            {
                foreach (var file in CompressFiles.ProcessingFiles)
                {
                    CompressSingle(file);
                }
            }
        }

        private void CompressSingle(FileInfo file)
        {
            string distPath = GetDistPath(file.FullName, Config.OutputFormat, out _);
            if (File.Exists(distPath))
            {
                File.Delete(distPath);
            }
            string dir = Path.GetDirectoryName(distPath)!;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            Console.OutputEncoding = System.Text.Encoding.Unicode;
            try
            {
                using (MagickImage image = new MagickImage(file))
                {
                    bool portrait = image.Height > image.Width;
                    int width = portrait ? image.Height : image.Width;
                    int height = portrait ? image.Width : image.Height;
                    if (width > Config.MaxLongSize || height > Config.MaxShortSize)
                    {
                        double ratio = width > Config.MaxLongSize ? 1.0 * Config.MaxLongSize / width : 1;
                        ratio = Math.Min(ratio, height > Config.MaxShortSize ? 1.0 * Config.MaxShortSize / height : 1);
                        width = (int)(width * ratio);
                        height = (int)(height * ratio);
                        if (portrait)
                        {
                            (width, height) = (height, width);
                        }
                        image.AdaptiveResize(width, height);
                    }
                    image.Quality = Config.Quality;
                    image.Write(distPath);
                }
                File.SetLastWriteTime(distPath, file.LastWriteTime);

                FileInfo distFile = new FileInfo(distPath);
                if (distFile.Length > file.Length)
                {
                    file.CopyTo(distPath, true);
                }

            }
            catch (Exception ex)
            {
                errorMessages.Add($"压缩 {Path.GetRelativePath(Config.SourceDir, file.FullName)} 失败：{ex.Message}");
            }
        }

        private void Clear()
        {
            foreach (var file in DeleteFiles.ProcessingFiles)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"删除 {Path.GetRelativePath(Config.SourceDir, file.FullName)} 失败：{ex.Message}");
                }
            }
        }

        private void Copy()
        {
            foreach (var file in CopyFiles.ProcessingFiles)
            {
                string distPath = GetDistPath(file.FullName, null, out string subPath);
                if (File.Exists(distPath))
                {
                    File.Delete(distPath);
                }
                string dir = Path.GetDirectoryName(distPath)!;
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                try
                {
                    file.CopyTo(distPath);
                }
                catch(Exception ex)
                {
                    errorMessages.Add($"赋值 {Path.GetRelativePath(Config.SourceDir, file.FullName)} 失败：{ex.Message}");
                }
            }
        }

        private string GetDistPath(string sourceFileName, string newExtension, out string subPath)
        {
            char spliiter = sourceFileName.Contains('\\') ? '\\' : '/';
            subPath = Path.IsPathRooted(sourceFileName) ? Path.GetRelativePath(Config.SourceDir, sourceFileName) : sourceFileName;
            string filename = Path.GetFileName(subPath);
            string dir = Path.GetDirectoryName(subPath);
            int level = dir.Count(p => p == spliiter) + 1;
            if (level > Config.DeepestLevel)
            {
                string[] dirParts = dir.Split(spliiter);
                dir = string.Join(spliiter, dirParts[..Config.DeepestLevel]);
                filename = $"{string.Join('-', dirParts[Config.DeepestLevel..])}-{filename}";
                subPath = Path.Combine(dir, filename);
            }

            if (newExtension != null)
            {
                subPath = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename) + "." + newExtension);
            }
            return Path.Combine(Config.DistDir, subPath);

        }

        private bool NeedProcess(TaskType type, FileInfo file)
        {
            if (type is TaskType.Delete)
            {
                return true;
            }
            if (!Config.SkipIfExist)
            {
                return true;
            }


            var distFile = new FileInfo(GetDistPath(file.FullName, type is TaskType.Copy ? null : Config.OutputFormat, out _));

            if (distFile.Exists && (type is TaskType.Compress || file.Length == distFile.Length && file.LastWriteTime == distFile.LastWriteTime))
            {
                return false;
            }

            return true;

        }
    }
    public class SlimmingFilesInfo
    {
        private List<FileInfo> processingFiles = new List<FileInfo>();
        private string rootDir;

        private List<FileInfo> skippedFiles = new List<FileInfo>();

        public SlimmingFilesInfo(string rootDir)
        {
            this.rootDir = rootDir;
        }
        public IReadOnlyList<FileInfo> ProcessingFiles => processingFiles.AsReadOnly();

        public long ProcessingFilesLength { get; private set; } = 0;

        public IReadOnlyList<string> ProcessingFilesRelativePaths => processingFiles
            .Select(p => Path.GetRelativePath(rootDir, p.FullName))
            .ToList().AsReadOnly();

        public IReadOnlyList<FileInfo> SkippedFiles => skippedFiles.AsReadOnly();
        public void Add(FileInfo file)
        {
            processingFiles.Add(file);
            ProcessingFilesLength += file.Length;
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

    }
}
