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
            List<SimpleFileViewModel> files = null;
            List<SimpleDirViewModel> subDirs = null;
            List<SimpleDirViewModel> targetDirs = new List<SimpleDirViewModel>();

            await Task.Run(() =>
            {
                var files = Directory.EnumerateFiles(GetSourceDir(), "*", SearchOption.AllDirectories)
                    .Select(p => new EncryptorFileViewModel(p))
                    .ToList();

            });
        }

        private bool IsEncrypting()
        {
            ArgumentNullException.ThrowIfNull(Config);
            return Config is EncryptConfig;
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

        private static bool IsNameEncrypted(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            if (!fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }
            string base64 = FileNameSafeStringToBase64(Path.GetFileNameWithoutExtension(fileName));
            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out _);
        }
        private async Task<string> EncryptFileNameAsync(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);

            if (fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName;
            }

            string encryptedName = null;
            await Task.Run(() =>
            {
                byte[] bytes = Encoding.Default.GetBytes(fileName);
                Aes aes = GetAes();
                bytes = aes.Encrypt(bytes);
                string base64 = Convert.ToBase64String(bytes);
                string safeFileName = Base64ToFileNameSafeString(base64);
                encryptedName = $"{safeFileName}{EncryptedFileExtension}";
            });
            return encryptedName;
        }

        private async Task<string> DecryptFileNameAsync(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);
            if (!fileName.EndsWith(EncryptedFileExtension, StringComparison.InvariantCultureIgnoreCase))
            {
                return fileName;
            }
            string newName = null;
            await Task.Run(() =>
            {
                string base64 = FileNameSafeStringToBase64(Path.GetFileNameWithoutExtension(fileName));
                var bytes = Convert.FromBase64String(base64);
                Aes aes = GetAes();
                bytes = aes.Decrypt(bytes);
                newName = Encoding.Default.GetString(bytes);
            });
            return newName;
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
