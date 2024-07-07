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

        public override async Task ExecuteAsync(CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(ProcessingFiles, nameof(ProcessingFiles));
            await Task.Run(() =>
            {
                int index = 0;
                Aes aes = GetAes();
                foreach (var file in ProcessingFiles)
                {
                    switch (Config.Type)
                    {
                        case EncryptorConfig.EncryptorTaskType.Encrypt:

                            NotifyProgressUpdate(ProcessingFiles.Count, index++, "正在加密文件");
                            string targetName = Config.EncryptFileNames ? EncryptFileName(file.Name) : $"{file.Name}{EncryptedFileExtension}";
                            string relativeRawDir = Path.GetDirectoryName(Path.GetRelativePath(Config.RawDir, file.Path));
                            string encryptedFilePath = Path.Combine(Config.EncryptedDir, relativeRawDir, targetName);
                            try
                            {
                                aes.EncryptFile(file.Path, encryptedFilePath, overwriteExistedFile: Config.OverwriteExistedFiles);
                                file.IsEncrypted = true;
                                file.IsFileNameEncrypted = Config.EncryptFileNames;
                            }
                            catch (Exception ex)
                            {
                                file.Error = ex;
                            }
                            break;
                        case EncryptorConfig.EncryptorTaskType.Decrypt:

                            NotifyProgressUpdate(ProcessingFiles.Count, index++, "正在解密文件");
                            string rawName=DecryptFileName(file.Name);
                            string relativeEncryptedDir = Path.GetDirectoryName(Path.GetRelativePath(Config.EncryptedDir, file.Path));
                            string rawFilePath= Path.Combine(Config.RawDir,relativeEncryptedDir,rawName);
                            try
                            {
                                aes.DecryptFile(file.Path, rawFilePath, overwriteExistedFile: Config.OverwriteExistedFiles);
                                file.IsEncrypted = false;
                                file.IsFileNameEncrypted = false;
                            }
                            catch(Exception ex)
                            {
                                file.Error = ex;
                            }
                            break;
                    }
                }
            }, token);
        }

        public override async Task InitializeAsync()
        {
            List<EncryptorFileViewModel> files = new List<EncryptorFileViewModel>();

            await Task.Run(() =>
            {
                var sourceDir = GetSourceDir();
                NotifyProgressUpdate(0, -1, "正在搜索文件");
                foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    var isEncrypted = IsEncryptedFile(file);

                    files.Add(new EncryptorFileViewModel(file)
                    {
                        IsFileNameEncrypted = isEncrypted && IsNameEncrypted(Path.GetFileName(file)),
                        IsEncrypted = isEncrypted
                    });
                }
            });

            ProcessingFiles = files;
        }

        private bool IsEncrypting()
        {
            ArgumentNullException.ThrowIfNull(Config);
            return Config.Type == EncryptorConfig.EncryptorTaskType.Encrypt;
        }

        private string GetSourceDir()
        {
            if (IsEncrypting())
            {
                return Config.RawDir;
            }
            return Config.EncryptedDir;
        }

        private string GetDistDir()
        {
            if (IsEncrypting())
            {
                return Config.EncryptedDir;
            }
            return Config.RawDir;
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
            return $"{safeFileName}{EncryptedFileExtension}";
        }

        private string DecryptFileName(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            if (!fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName;
            }

            string name = Path.GetFileNameWithoutExtension(fileName);
            if (IsNameEncrypted(fileName))
            {
                string base64 = FileNameSafeStringToBase64(name);
                var bytes = Convert.FromBase64String(base64);
                Aes aes = GetAes();
                bytes = aes.Decrypt(bytes);
                return Encoding.Default.GetString(bytes);
            }
            return name;
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
    }
}
