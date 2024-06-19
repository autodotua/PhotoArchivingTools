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
        private Regex rBlack;
        private Regex rCompress;
        private Regex rCopy;
        private Regex rWhite;
        public PhotoSlimmingUtility(PhotoSlimmingConfig config)
        {
            Config = config;
            rCopy = new Regex(@$"\.({string.Join('|', Config.CopyDirectlyExtensions)})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            rCompress = new Regex(@$"\.({string.Join('|', Config.CompressExtensions)})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            rBlack = new Regex(Config.BlackList);
            rWhite = new Regex(string.IsNullOrWhiteSpace(Config.WhiteList) ? ".*" : Config.WhiteList);
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

                foreach (var file in DeleteFiles.ProcessingFiles)
                {
                    File.Delete(file.FullName);
                }
            });
        }

        public override Task InitializeAsync()
        {
            CompressFiles = new SlimmingFilesInfo(Config.SourceDir);
            CopyFiles = new SlimmingFilesInfo(Config.SourceDir);
            DeleteFiles = new SlimmingFilesInfo(Config.SourceDir);

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
                    .Select(file => GetDistPath(file.FullName, null,  out _))
                     .Concat(CompressFiles.SkippedFiles
                        .Select(file => GetDistPath(file.FullName, Config.OutputFormat,  out _)))
                     .ToHashSet();


                foreach (var file in Directory
                    .EnumerateFiles(Config.DistDir, "*", SearchOption.AllDirectories)
                     .Where(p => !rBlack.IsMatch(p)))
                {
                    if (!desiredDistFiles.Contains(file))
                    {
                        DeleteFiles.Add(new FileInfo(file));
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
            string distPath = GetDistPath(file.FullName, Config.OutputFormat, out string subPath);
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
                file.CopyTo(distPath);
            }
        }

        private DateTime? FindExifTime(string file)
        {
            var directories = ImageMetadataReader.ReadMetadata(file);
            MetadataExtractor.Directory? dir = null;
            if ((dir = directories.FirstOrDefault(p => p.Name == "Exif SubIFD")) != null)
            {
                if (dir.TryGetDateTime(36867, out DateTime time1))
                {
                    return time1;
                }
                if (dir.TryGetDateTime(36868, out DateTime time2))
                {
                    return time2;
                }
            }
            if ((dir = directories.FirstOrDefault(p => p.Name == "Exif IFD0")) != null)
            {
                if (dir.TryGetDateTime(306, out DateTime time))
                {
                    return time;
                }
            }



            return null;
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


            var distFile = new FileInfo(GetDistPath(file.FullName, type is TaskType.Copy ? null : Config.OutputFormat,  out _));

            if (distFile.Exists && (type is TaskType.Compress || file.Length == distFile.Length && file.LastWriteTime == distFile.LastWriteTime))
            {
                return false;
            }

            return true;

        }
    }
    public class SlimmingFilesInfo
    {
        private string rootDir;

        public SlimmingFilesInfo(string rootDir)
        {
            this.rootDir = rootDir;
        }

        private List<FileInfo> processingFiles = new List<FileInfo>();

        private List<FileInfo> skippedFiles = new List<FileInfo>();

        public IReadOnlyList<FileInfo> ProcessingFiles => processingFiles.AsReadOnly();

        public long ProcessingFilesLength { get; private set; } = 0;

        public IReadOnlyList<FileInfo> SkippedFiles => skippedFiles.AsReadOnly();

        public IReadOnlyList<string> ProcessingFilesRelativePaths => processingFiles
            .Select(p => Path.GetRelativePath(rootDir, p.FullName))
            .ToList().AsReadOnly();

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
