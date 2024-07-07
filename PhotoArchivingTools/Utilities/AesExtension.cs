using FzLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
namespace PhotoArchivingTools.Utilities
{
    public static class AesExtension
    {
        public static Aes SetStringKey(this Aes manager, string key, char fill = (char)0, Encoding encoding = null)
        {
            manager.Key = GetBytesFromString(manager, key, fill, encoding);
            return manager;
        }

        public static Aes SetStringIV(this Aes manager, string iv, char fill = (char)0, Encoding encoding = null)
        {
            manager.IV = GetBytesFromString(manager, iv, fill, encoding);
            return manager;
        }

        private static byte[] GetBytesFromString(Aes manager, string input, char fill, Encoding encoding)
        {
            input ??= "";
            int length = manager.BlockSize / 8;
            if (input.Length < length)
            {
                input += new string(fill, length - input.Length);
            }
            else if (input.Length > length)
            {
                input = input[..length];
            }
            return (encoding ?? Encoding.UTF8).GetBytes(input);
        }


        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="array">要加密的 byte[] 数组</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Encrypt(this Aes manager, byte[] array)
        {
            var encryptor = manager.CreateEncryptor();
            return encryptor.TransformFinalBlock(array, 0, array.Length);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="array">要解密的 byte[] 数组</param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] Decrypt(this Aes manager, byte[] array)
        {
            var decryptor = manager.CreateDecryptor();
            return decryptor.TransformFinalBlock(array, 0, array.Length);
        }

        public static void EncryptStreamToStream(this Aes manager, Stream streamInput, Stream streamOutput, int bufferLength = 1024 * 1024)
        {
            if (!streamInput.CanRead)
            {
                throw new Exception("输入流不可读");
            }
            if (!streamOutput.CanWrite)
            {
                throw new Exception("输出流不可写");
            }
            using var encryptor = manager.CreateEncryptor();
            byte[] input = new byte[bufferLength];
            byte[] output = new byte[bufferLength];
            int size;
            long length = streamInput.Length;
            while ((size = streamInput.Read(input, 0, bufferLength)) != 0)
            {
                if (streamInput.Position == length)
                {
                    output = encryptor.TransformFinalBlock(input, 0, size);
                }
                else
                {
                    encryptor.TransformBlock(input, 0, size, output, 0);
                }
                streamOutput.Write(output, 0, output.Length);
                streamOutput.Flush();
            }
        }

        public static void DecryptStreamToStream(this Aes manager, Stream streamInput, Stream streamOutput, int bufferLength = 1024 * 1024)
        {
            if (!streamInput.CanRead)
            {
                throw new Exception("输入流不可读");
            }
            if (!streamOutput.CanWrite)
            {
                throw new Exception("输出流不可写");
            }
            using var encryptor = manager.CreateDecryptor();
            byte[] input = new byte[bufferLength];
            byte[] output = new byte[bufferLength];
            int size;
            int outputSize = 0;
            long length = streamInput.Length;
            while ((size = streamInput.Read(input, 0, bufferLength)) != 0)
            {
                if (streamInput.Position == length)
                {
                    output = encryptor.TransformFinalBlock(input, 0, size);
                    outputSize = output.Length;
                }
                else
                {
                    outputSize = encryptor.TransformBlock(input, 0, size, output, 0);
                }
                streamOutput.Write(output, 0, outputSize);
                streamOutput.Flush();
            }
        }

        private static void CheckFileAndDirectoryExist(string path, bool overwriteExistedFiles)
        {
            if (File.Exists(path))
            {
                if (overwriteExistedFiles)
                {
                    if (File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly))
                    {
                        File.SetAttributes(path, FileAttributes.Normal);
                    }
                    File.Delete(path);
                }
                else
                {
                    throw new IOException("文件" + path + "已存在");
                }
            }
            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
        }
        public static Task<FileInfo> EncryptFileAsync(this Aes manager, string sourcePath, string targetPath,
    int bufferLength = 1024 * 1024,
    bool overwriteExistedFile = false,
    RefreshFileProgress refreshFileProgress = null)
        {
            return Task.Run(() => EncryptFile(manager, sourcePath, targetPath, bufferLength, overwriteExistedFile, refreshFileProgress));
        }
        /// <summary>
        /// 加密文件
        /// </summary>
        /// <param name="manager"></param>
        /// <param name="sourcePath">源文件地址</param>
        /// <param name="targetPath">目标文件地址</param>
        /// <param name="bufferLength">缓冲区大小</param>
        /// <param name="suffix">加密后的文件后缀</param>
        /// <param name="volumeSize">分卷大小，0表示不分卷</param>
        /// <param name="overwriteExistedFile">是否覆盖已存在文件。若为False但存在文件，则会抛出异常</param>
        /// <param name="refreshFileProgress"></param>
        /// <returns></returns>
        public static FileInfo EncryptFile(this Aes manager, string sourcePath, string targetPath,
            int bufferLength = 1024 * 1024,
            bool overwriteExistedFile = false,
            RefreshFileProgress refreshFileProgress = null)
        {
            CheckFileAndDirectoryExist(targetPath, overwriteExistedFile);
            try
            {
                using (FileStream streamSource = new FileStream(sourcePath, FileMode.Open, FileAccess.Read))
                {
                    FileStream streamTarget = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write);
                    using var encryptor = manager.CreateEncryptor();
                    long currentSize = 0;

                    int size;
                    byte[] input = new byte[bufferLength];
                    byte[] output = new byte[bufferLength];
                    long fileLength = streamSource.Length;
                    while ((size = streamSource.Read(input, 0, bufferLength)) != 0)
                    {
                        if (streamSource.Position == fileLength)
                        {
                            output = encryptor.TransformFinalBlock(input, 0, size);
                        }
                        else
                        {
                            encryptor.TransformBlock(input, 0, size, output, 0);
                        }

                        currentSize += size;

                        streamTarget.Write(output, 0, output.Length);
                        streamTarget.Flush();
                        refreshFileProgress?.Invoke(sourcePath, targetPath, fileLength, currentSize); //更新进度
                    }
                    streamTarget.Close();
                    streamTarget.Dispose();
                }
                FileInfo encryptedFile = new FileInfo(targetPath);

                encryptedFile.Attributes = File.GetAttributes(sourcePath);
                return encryptedFile;
            }
            catch (Exception ex)
            {
                HandleException(targetPath, ex);
                throw;
            }
        }

        public static Task<FileInfo[]> DecryptFileAsync(this Aes manager, string sourcePath, string targetPath,
            int bufferLength = 1024 * 1024,
            bool overwriteExistedFile = false,
            RefreshFileProgress refreshFileProgress = null)
        {
            return Task.Run(()=> DecryptFile(manager, sourcePath, targetPath, bufferLength, overwriteExistedFile, refreshFileProgress));
        }
        public static FileInfo[] DecryptFile(this Aes manager, string sourcePath, string targetPath,
            int bufferLength = 1024 * 1024,
            bool overwriteExistedFile = false,
            RefreshFileProgress refreshFileProgress = null)
        {
            CheckFileAndDirectoryExist(targetPath, overwriteExistedFile);
            List<string> encryptedFileNames = new List<string>() { sourcePath };
            var lastencryptedFile = sourcePath;
            try
            {
                int fileCount = 0;
                while (File.Exists(sourcePath + (++fileCount)))
                {
                    encryptedFileNames.Add(sourcePath + fileCount);

                    lastencryptedFile = sourcePath + fileCount;
                }

                using (FileStream streamTarget = new FileStream(targetPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    using (var decryptor = manager.CreateDecryptor())
                    {
                        foreach (var encryptedFileName in encryptedFileNames)
                        {
                            FileStream streamSource = new FileStream(encryptedFileName, FileMode.Open, FileAccess.Read);

                            long currentSize = 0;

                            int size;
                            byte[] input = new byte[bufferLength];
                            byte[] output = new byte[bufferLength];
                            long fileLength = streamSource.Length;
                            while ((size = streamSource.Read(input, 0, input.Length)) != 0)
                            {
                                int outputSize = 0;
                                if (streamSource.Position == fileLength && encryptedFileName == lastencryptedFile)
                                {
                                    outputSize = (output = decryptor.TransformFinalBlock(input, 0, size)).Length;
                                }
                                else
                                {
                                    outputSize = decryptor.TransformBlock(input, 0, size, output, 0);
                                }

                                currentSize += output.Length;

                                streamTarget.Write(output, 0, outputSize);
                                streamTarget.Flush();
                                refreshFileProgress?.Invoke(sourcePath, encryptedFileName, fileLength, currentSize); //更新进度
                            }
                            streamSource.Close();
                            streamSource.Dispose();
                        }
                    }

                    streamTarget.Close();
                    streamTarget.Dispose();
                }

                new FileInfo(targetPath).Attributes = File.GetAttributes(sourcePath);
                return encryptedFileNames.Select(p => new FileInfo(p)).ToArray();
            }
            catch (Exception ex)
            {
                HandleException(targetPath, ex);
                throw;
            }
        }

        private static void HandleException(string target, Exception ex)
        {
            try
            {
                File.Delete(target);
            }
            catch
            {
            }
            throw ex;
        }

        /// <summary>
        /// 更新文件加密进度
        /// </summary>
        public delegate void RefreshFileProgress(string source, string target, long max, long value);
    }
}
