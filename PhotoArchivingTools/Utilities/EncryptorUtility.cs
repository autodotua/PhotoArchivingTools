using PhotoArchivingTools.Configs;
using PhotoArchivingTools.ViewModels;
using PhotoArchivingTools.ViewModels.FileSystemViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace PhotoArchivingTools.Utilities
{

    public class EncryptorUtility(EncryptorConfig config) : UtilityBase
    {
        public const string EncryptedFileExtension = ".ept";
        public EncryptorConfig Config { get; init; } = config;
        public List<EncryptorFileViewModel> ProcessingFiles { get; set; }
        public int BufferSize { get; set; } = 1024 * 1024;

        public override async Task ExecuteAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(ProcessingFiles, nameof(ProcessingFiles));

            await Task.Run(() =>
            {
                int index = 0;
                Aes aes = GetAes();

                bool isEncrypting = IsEncrypting();
                string sourceDir = GetSourceDir();
                string targetDir = GetDistDir();
                Dictionary<string, string> longFileNames;
                if (Config.LongFileNamesSupport && isEncrypting)
                {
                    longFileNames = new Dictionary<string, string>();
                }
                string baseMessage = null;
                var progressReport = new AesExtension.RefreshFileProgress((string source, string target, long max, long value) =>
                {
                    NotifyProgressUpdate(ProcessingFiles.Count, index, baseMessage +
                        $"（{index}/{ProcessingFiles.Count}），当前文件：{Path.GetFileName(source)}（{1.0 * value / 1024 / 1024:0}MB/{1.0 * max / 1024 / 1024:0}MB）");
                });

                foreach (var file in ProcessingFiles)
                {
                    token.ThrowIfCancellationRequested();
                    baseMessage = isEncrypting ? "正在加密文件" : "正在解密文件";
                    NotifyProgressUpdate(ProcessingFiles.Count, index++, baseMessage +
                        $"（{index}/{ProcessingFiles.Count}），当前文件：{file.Name}（0MB/{1.0 * new FileInfo(file.Path).Length / 1024 / 1024:0}）");

                    string targetName, targetFilePath;
                    ProcessFileNames(isEncrypting, sourceDir, targetDir, file, out targetName, out targetFilePath);

                    try
                    {
                        if (isEncrypting)
                        {
                            aes.EncryptFile(file.Path, targetFilePath, token, BufferSize, Config.OverwriteExistedFiles, progressReport);
                            file.IsFileNameEncrypted = Config.EncryptFileNames;
                        }
                        else
                        {
                            aes.DecryptFile(file.Path, targetFilePath, token, BufferSize, Config.OverwriteExistedFiles, progressReport);
                            file.IsFileNameEncrypted = false;
                        }
                        file.IsEncrypted = isEncrypting;
                        file.TargetName = targetName;
                        file.TargetRelativePath = Path.GetRelativePath(targetDir, targetFilePath);
                        File.SetLastWriteTime(targetFilePath, File.GetLastWriteTime(file.Path));

                        if (Config.DeleteSourceFiles)
                        {
                            if (File.GetAttributes(file.Path).HasFlag(FileAttributes.ReadOnly))
                            {
                                File.SetAttributes(file.Path, FileAttributes.Normal);
                            }
                            File.Delete(file.Path);
                        }
                    }
                    catch (Exception ex)
                    {
                        file.Error = ex;
                    }
                }
            }, token);
        }

        private void ProcessFileNames(EncryptorFileViewModel file, Dictionary<string, string> longNames)
        {
            ArgumentNullException.ThrowIfNull(file);
            if (Config.LongFileNamesSupport)
            {
                ArgumentNullException.ThrowIfNull(longNames);
            }

            var isEncrypting = IsEncrypting();
            if (isEncrypting)
            {
                string encryptedFileName;
                if (Config.EncryptFileNames)
                {
                    encryptedFileName = EncryptFileName(file.Name);
                }
                else
                {
                    encryptedFileName = file.Name;
                }
                if (Config.LongFileNamesSupport)
                {
                    string guid = null;
                    do
                    {
                        guid = Guid.NewGuid().ToString("N")[..8];
                    } while (!longNames.ContainsKey(guid));
                    longNames.Add(guid, encryptedFileName);
                    encryptedFileName = guid;

                }
                else
                {
                    encryptedFileName += EncryptedFileExtension;
                }
                file.TargetName = encryptedFileName;
            }
            else
            {
                string rawFileName;
                if (Config.LongFileNamesSupport)
                {
                    if (!longNames.TryGetValue(file.Name, out string value))
                    {
                        throw new Exception("在文件名字典文件中没有找到对应文件的原文件名");
                    }
                    rawFileName = value;
                }
                else
                {
                    rawFileName = file.Name;
                }
                file.TargetName = DecryptFileName(file.Name);
            }

            //加解密目录名
            string relativeDir = Path.GetDirectoryName(Path.GetRelativePath(GetSourceDir(), file.Path));
            if (isEncrypting && Config.EncryptFolderNames)
            {
                relativeDir = EncryptFoldersNames(relativeDir);
            }
            else if (!isEncrypting && relativeDir.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                relativeDir = DecryptFoldersNames(relativeDir);
            }
            targetFilePath = Path.Combine(GetDistDir(), relativeDir, targetName);
        }

        public override async Task InitializeAsync()
        {
            List<EncryptorFileViewModel> files = new List<EncryptorFileViewModel>();

            var sourceDir = GetSourceDir();
            if (!Directory.Exists(sourceDir))
            {
                throw new Exception("源目录不存在");
            }

            await Task.Run(() =>
            {
                NotifyProgressUpdate(0, -1, "正在搜索文件");
                foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    var isEncrypted = IsEncryptedFile(file);

                    files.Add(new EncryptorFileViewModel(file)
                    {
                        IsFileNameEncrypted = isEncrypted && IsNameEncrypted(Path.GetFileName(file)),
                        IsEncrypted = isEncrypted,
                        RelativePath = Path.GetRelativePath(sourceDir, file)
                    });
                }
            });

            ProcessingFiles = files;
        }

        private static string Base64ToFileNameSafeString(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentException("Base64 string cannot be null or empty");
            }

            string safeString = base64.Replace('+', '-')
                                      .Replace('/', '_')
                                      .Replace('=', '~');

            return safeString;
        }

        private static string FileNameSafeStringToBase64(string safeString)
        {
            if (string.IsNullOrEmpty(safeString))
            {
                throw new ArgumentException("Safe string cannot be null or empty");
            }

            string base64 = safeString.Replace('-', '+')
                                      .Replace('_', '/')
                                      .Replace('~', '=');

            return base64;
        }

        private static bool IsEncryptedFile(string fileName)
        {
            if (fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
            return false;
        }

        private static bool IsNameEncrypted(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            if (!IsEncryptedFile(fileName))
            {
                throw new ArgumentException("文件未被加密");
            }
            string base64 = FileNameSafeStringToBase64(Path.GetFileNameWithoutExtension(fileName));
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }

        /// <summary>
        /// 解密文件名，可包括或不包括后缀
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string DecryptFileName(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            if (fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                fileName = Path.GetFileNameWithoutExtension(fileName);
            }

            if (IsNameEncrypted(fileName))
            {
                string base64 = FileNameSafeStringToBase64(fileName);
                var bytes = Convert.FromBase64String(base64);
                Aes aes = GetAes();
                bytes = aes.Decrypt(bytes);
                return Encoding.Default.GetString(bytes);
            }
            return fileName;
        }

        /// <summary>
        /// 加密文件名，不包含后缀
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private string EncryptFileName(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);

            if (IsEncryptedFile(fileName))
            {
                throw new ArgumentException("文件已被加密");
            }

            byte[] bytes = Encoding.Default.GetBytes(fileName);
            Aes aes = GetAes();
            bytes = aes.Encrypt(bytes);
            string base64 = Convert.ToBase64String(bytes);
            string safeFileName = Base64ToFileNameSafeString(base64);
            return safeFileName;
        }

        private Aes GetAes()
        {
            Aes aes = Aes.Create();
            aes.Mode = Config.CipherMode;
            aes.Padding = Config.PaddingMode;
            aes.SetStringKey(Config.Password);
            aes.IV = MD5.HashData(Encoding.UTF8.GetBytes(Config.Password));
            return aes;
        }

        private string GetDistDir()
        {
            if (IsEncrypting())
            {
                return Config.EncryptedDir;
            }
            return Config.RawDir;
        }

        private string GetSourceDir()
        {
            if (IsEncrypting())
            {
                return Config.RawDir;
            }
            return Config.EncryptedDir;
        }

        private bool IsEncrypting()
        {
            ArgumentNullException.ThrowIfNull(Config);
            return Config.Type == EncryptorConfig.EncryptorTaskType.Encrypt;
        }

        private string EncryptFoldersNames(string relativePath)
        {
            var parts = relativePath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = EncryptFileName(parts[i]);
            }
            return string.Join(Path.DirectorySeparatorChar, parts);
        }

        private string DecryptFoldersNames(string relativePath)
        {
            var parts = relativePath.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = DecryptFileName(parts[i]);
            }
            return string.Join(Path.DirectorySeparatorChar, parts);
        }
        private void EncryptFolders(string dir, bool includeSelf = true)
        {
            foreach (var subDir in Directory.EnumerateDirectories(dir))
            {
                EncryptFolders(subDir);
            }
            if (includeSelf)
            {
                string newName = EncryptFileName(Path.GetFileName(dir));
                string newPath = Path.Combine(Path.GetDirectoryName(dir), newName);
                Directory.Move(dir, newPath);
            }
        }
    }
}
