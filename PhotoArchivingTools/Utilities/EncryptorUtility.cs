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

            }, token);
        }

        public override async Task InitializeAsync()
        {
            List<EncryptorFileViewModel> files = new List<EncryptorFileViewModel>();

            await Task.Run(() =>
            {
                var sourceDir = GetSourceDir();
                var isEncrypting = IsEncrypting();

                foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
                {
                    var isEncrypted = IsEncryptedFile(file);

                    if (isEncrypting && !isEncrypted || !isEncrypting && isEncrypted)
                    {
                        files.Add(new EncryptorFileViewModel(file)
                        {
                            IsFileNameEncrypted = isEncrypted && IsNameEncrypted(Path.GetFileName(file)),
                            IsEncrypted = isEncrypting
                        });
                    }
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

            string base64 = FileNameSafeStringToBase64(Path.GetFileNameWithoutExtension(fileName));
            var bytes = Convert.FromBase64String(base64);
            Aes aes = GetAes();
            bytes = aes.Decrypt(bytes);
            return Encoding.Default.GetString(bytes);
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
