using System;
using System.Security.Cryptography;

namespace PhotoArchivingTools.Configs
{
    public abstract class EncryptConfig : EncryptorConfig
    {
        public bool EncryptFileNames { get; set; }

        public bool EncryptFolderNames { get; set; }
    }
    public abstract class DecryptConfig : EncryptorConfig
    {

    }
    public abstract class EncryptorConfig : ConfigBase
    {
        public CipherMode CipherMode { get; set; } = CipherMode.CBC;
        public PaddingMode PaddingMode { get; set; } = PaddingMode.PKCS7;
        public string RawDir { get; set; }

        public string EncryptedDir { get; set; }

        public string Password { get; set; }

        public bool DeleteSourceFiles { get; set; }

        public bool OverwriteExistedFiles { get; set; }
    }
}
