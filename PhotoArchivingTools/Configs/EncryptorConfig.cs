using System;
using System.Security.Cryptography;

namespace PhotoArchivingTools.Configs
{
    public class EncryptorConfig : ConfigBase
    {
        public enum EncryptorTaskType
        {
            Encrypt,
            Decrypt,
        }
        public bool LongFileNamesSupport { get; set; }

        public CipherMode CipherMode { get; set; } = CipherMode.CBC;

        public bool DeleteSourceFiles { get; set; }

        public string EncryptedDir { get; set; }

        public bool EncryptFileNames { get; set; }

        public bool EncryptFolderNames { get; set; }

        public bool OverwriteExistedFiles { get; set; }

        public PaddingMode PaddingMode { get; set; } = PaddingMode.PKCS7;

        public string Password { get; set; }

        public string RawDir { get; set; }

        public bool RememberPassword { get; set; }

        public EncryptorTaskType Type { get; set; } = EncryptorTaskType.Encrypt;
    }
}
