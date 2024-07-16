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
using PhotoArchivingTools.ViewModels;
using System.Diagnostics;

namespace PhotoArchivingTools.Utilities
{
    public class PhotoSlimmingUtility : UtilityBase
    {
        private readonly Regex rBlack;
        private readonly Regex rCompress;
        private readonly Regex rCopy;
        private readonly Regex rWhite;
        private ConcurrentBag<string> errorMessages;
        private int progress = 0;
        public PhotoSlimmingUtility(PhotoSlimmingConfig config)
        {
            Config = config;
            rCopy = new Regex(@$"\.({string.Join('|', Config.CopyDirectlyExtensions)})$", RegexOptions.IgnoreCase);
            rCompress = new Regex(@$"\.({string.Join('|', Config.CompressExtensions)})$", RegexOptions.IgnoreCase);
            rBlack = new Regex(Config.BlackList);
            rWhite = new Regex(string.IsNullOrWhiteSpace(Config.WhiteList) ? ".*" : Config.WhiteList, RegexOptions.IgnoreCase);
            
            if (!config.FolderNameTemplate.Contains(PhotoSlimmingConfig.FolderNamePlaceholder))
            {
                throw new Exception("文件夹名模板不包含文件夹名占位符");
            }
            if (!config.FileNameTemplate.Contains(PhotoSlimmingConfig.FileNamePlaceholder))
            {
                throw new Exception("文件夹名模板不包含文件夹名占位符");
            }
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

        public IReadOnlyCollection<string> ErrorMessages => errorMessages;

        public override Task ExecuteAsync(CancellationToken token)
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

                progress = 0;

                Compress(token);

                Copy(token);

                Clear(token);
            }, token);
        }

        public override Task InitializeAsync()
        {
            CompressFiles = new SlimmingFilesInfo(Config.SourceDir);
            CopyFiles = new SlimmingFilesInfo(Config.SourceDir);
            DeleteFiles = new SlimmingFilesInfo(Config.SourceDir);
            errorMessages = new ConcurrentBag<string>();

            return Task.Run(() =>
            {
                SearchCopyingAndCompressingFiles();

                SearchDeletingFiles();
            });
        }

        private void SearchDeletingFiles()
        {
            if (!Directory.Exists(Config.DistDir))
            {
                return;
            }
            NotifyProgressUpdate(1, -1, "正在筛选需要删除的文件");
            var desiredDistFiles = CopyFiles.SkippedFiles
            .Select(file => GetDistPath(file.FullName, null, out _))
             .Concat(CompressFiles.SkippedFiles
                .Select(file => GetDistPath(file.FullName, Config.OutputFormat, out _)))
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

            NotifyProgressUpdate(1, -1, "正在需要删除的文件夹");
            var desiredDistFolders= desiredDistFiles.Select(Path.GetDirectoryName).ToHashSet();

            foreach (var dir in Directory.EnumerateDirectories(Config.DistDir, "*", SearchOption.AllDirectories))
            {
                if(!desiredDistFolders.Contains(dir))
                {
                    DeleteFiles.Add(new FileInfo(dir));
                }
            }
        }

        private void SearchCopyingAndCompressingFiles()
        {
            NotifyProgressUpdate(1, -1, "正在搜索目录");
            var files = new DirectoryInfo(Config.SourceDir)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .ToList();
            int index = 0;
            foreach (var file in files)
            {
                index++;
                NotifyProgressUpdate(files.Count, index, $"正在查找文件 ({index}/{files.Count})");

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
        }

        private void Clear(CancellationToken token)
        {
            foreach (var file in DeleteFiles.ProcessingFiles)
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (file.Exists)
                    {
                        File.Delete(file.FullName);
                    }
                    else if (Directory.Exists(file.FullName))
                    {
                        Directory.Delete(file.FullName, true);
                    }
                }
                catch (Exception ex)
                {
                    errorMessages.Add($"删除 {Path.GetRelativePath(Config.SourceDir, file.FullName)} 失败：{ex.Message}");
                }
                finally
                {
                    int totalCount = CopyFiles.ProcessingFiles.Count + CompressFiles.ProcessingFiles.Count + DeleteFiles.ProcessingFiles.Count;
                    Interlocked.Increment(ref progress);
                    NotifyProgressUpdate(totalCount, progress, $"正在删除 ({progress} / {totalCount})");
                }
            }

        }

        private void Compress(CancellationToken token)
        {
            if (Config.Thread != 1)
            {
                Parallel.ForEach(CompressFiles.ProcessingFiles,
                    new ParallelOptions()
                    {
                        MaxDegreeOfParallelism = Config.Thread <= 0 ? -1 : Config.Thread,
                        CancellationToken = token
                    },
                    file =>
                    {
                        token.ThrowIfCancellationRequested();
                        CompressSingle(file);
                    });
            }
            else
            {
                foreach (var file in CompressFiles.ProcessingFiles)
                {
                    token.ThrowIfCancellationRequested();
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
            finally
            {
                int totalCount = CopyFiles.ProcessingFiles.Count + CompressFiles.ProcessingFiles.Count + DeleteFiles.ProcessingFiles.Count;
                Interlocked.Increment(ref progress);
                NotifyProgressUpdate(totalCount, progress, $"正在压缩 ({progress} / {totalCount})");
            }
        }

        private void Copy(CancellationToken token)
        {
            foreach (var file in CopyFiles.ProcessingFiles)
            {
                token.ThrowIfCancellationRequested();
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
                catch (Exception ex)
                {
                    errorMessages.Add($"赋值 {Path.GetRelativePath(Config.SourceDir, file.FullName)} 失败：{ex.Message}");
                }
                finally
                {
                    int totalCount = CopyFiles.ProcessingFiles.Count + CompressFiles.ProcessingFiles.Count + DeleteFiles.ProcessingFiles.Count;
                    Interlocked.Increment(ref progress);
                    NotifyProgressUpdate(totalCount, progress, $"正在复制 ({progress} / {totalCount})");
                }
            }
        }

        private string GetDistPath(string sourceFileName, string newExtension, out string subPath)
        {
            char splitter = sourceFileName.Contains('\\') ? '\\' : '/';
            string subDir = Path.GetDirectoryName(Path.GetRelativePath(Config.SourceDir, sourceFileName));
            if (!Path.IsPathRooted(sourceFileName))
            {
                Debug.Assert(false);
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName);
            string extension = Path.GetExtension(sourceFileName);

            if (Config.FileNameTemplate != PhotoSlimmingConfig.FileNamePlaceholder)
            {
                fileNameWithoutExtension = Config.FileNameTemplate.Replace(PhotoSlimmingConfig.FileNamePlaceholder, fileNameWithoutExtension);
            }
            if (!string.IsNullOrEmpty(newExtension))
            {
                extension = $".{newExtension}";
            }

            int level = subDir.Count(c => c == splitter) + 1;

            if (level > Config.DeepestLevel)
            {
                string[] dirParts = subDir.Split(splitter);
                subDir = string.Join(splitter, dirParts[..Config.DeepestLevel]);
                fileNameWithoutExtension = $"{string.Join('-', dirParts[Config.DeepestLevel..])}-{fileNameWithoutExtension}";
            }

            if (Config.FolderNameTemplate != PhotoSlimmingConfig.FolderNamePlaceholder && subDir.Length > 0)
            {
                string[] dirParts = subDir.Split(splitter);
                subDir = Path.Combine(dirParts.Select(p =>
                Config.FolderNameTemplate.Replace(PhotoSlimmingConfig.FolderNamePlaceholder, p))
                    .ToArray());
            }

            subPath = Path.Combine(subDir, fileNameWithoutExtension + extension);

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
}
