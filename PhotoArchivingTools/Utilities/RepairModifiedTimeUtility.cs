using FzLib;
using MetadataExtractor;
using PhotoArchivingTools.Configs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoArchivingTools.Utilities
{
    public class RepairModifiedTimeUtility(RepairModifiedTimeConfig config) : UtilityBase
    {
        public string[] Extensions = { "jpg", "jpeg", "heif", "heic" };
        private ConcurrentDictionary<FileInfo, string> errorFiles = new ConcurrentDictionary<FileInfo, string>();
        private ConcurrentDictionary<FileInfo, DateTime> fileExifTimes;

        private int progress = 0;
        private Regex rRepairTime;
        public RepairModifiedTimeConfig Config { get; set; } = config;
        public List<string> ErrorFilesAndMessages { get; private set; }
        public List<string> UpdatingFilesAndMessages { get; private set; }
        public override Task ExecuteAsync()
        {
            return ExecuteAsync(CancellationToken.None);
        }
        public Task ExecuteAsync(CancellationToken token)
        {
            ErrorFilesAndMessages = new List<string>();
            return Task.Run(() =>
            {
                int index = 0;
                foreach (var file in fileExifTimes.Keys)
                {
                    token.ThrowIfCancellationRequested();
                    index++;
                    NotifyProgressUpdate(fileExifTimes.Count, index, $"正在处理（{index}/{fileExifTimes.Count}）：{file.FullName}");
                    try
                    {
                        file.LastWriteTime = fileExifTimes[file];
                    }
                    catch (Exception ex)
                    {
                        ErrorFilesAndMessages.Add($"{file.FullName}：{ex.Message}");
                    }
                }
            });
        }
        public override async Task InitializeAsync()
        {
            fileExifTimes = new ConcurrentDictionary<FileInfo, DateTime>();
            errorFiles = new ConcurrentDictionary<FileInfo, string>();
            rRepairTime = new Regex(@$"\.({string.Join('|', Extensions)})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            List<FileInfo> files = null;
            await Task.Run(() =>
            {
                NotifyProgressUpdate(1, -1, "正在扫描文件");
                files = new DirectoryInfo(Config.Dir).EnumerateFiles("*", SearchOption.AllDirectories).ToList();
                if (Config.ThreadCount > 1)
                {
                    var options = new ParallelOptions() { MaxDegreeOfParallelism = Config.ThreadCount };
                    Parallel.ForEach(files, options, Check);
                }
                else
                {
                    files.ForEach(Check);
                }
            });
            ErrorFilesAndMessages = errorFiles
                .Select(p => $"{Path.GetRelativePath(Config.Dir, p.Key.FullName)}：{p.Value}")
                .ToList();
            UpdatingFilesAndMessages = fileExifTimes
                .Select(p => $"{Path.GetRelativePath(Config.Dir, p.Key.FullName)}    {p.Key.LastWriteTime} => {p.Value}")
                .ToList();

            void Check(FileInfo file)
            {
                Interlocked.Increment(ref progress);
                NotifyProgressUpdate(files.Count, progress, $"正在扫描照片日期（{progress}/{files.Count}）");
                if (rRepairTime.IsMatch(file.Name))
                {
                    DateTime? exifTime;
                    try
                    {
                        exifTime = FindExifTime(file.FullName);
                    }
                    catch (Exception ex)
                    {
                        errorFiles.TryAdd(file, ex.Message);
                        return;
                    }

                    if (exifTime.HasValue)
                    {
                        var fileTime = file.LastWriteTime;
                        var duration = (exifTime.Value - fileTime).Duration();
                        if (duration > Config.MaxDurationTolerance)
                        {
                            fileExifTimes.TryAdd(file, exifTime.Value);
                        }
                    }
                }
            }

        }

        private DateTime? FindExifTime(string file)
        {
            IReadOnlyList<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(file);
            MetadataExtractor.Directory dir = null;
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
    }
}
