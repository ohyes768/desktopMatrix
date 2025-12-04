using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DesktopMatrix.Services
{
    /// <summary>
    /// 加密服务接口
    /// </summary>
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }

    /// <summary>
    /// 加密服务实现类
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        // 固定密钥和向量（实际应用中应从安全存储获取）
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService()
        {
            // 在实际应用中，应该使用更安全的方式存储和获取密钥
            // 这里使用固定值仅作为示例
            string keyString = "QuadrantManagerSecretKey123456789012";
            string ivString = "QuadrantManager1";

            // 确保密钥长度符合AES-256要求（32字节）
            _key = new byte[32];
            byte[] keyBytes = Encoding.UTF8.GetBytes(keyString);
            Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, 32));

            // 确保IV长度符合AES要求（16字节）
            _iv = new byte[16];
            byte[] ivBytes = Encoding.UTF8.GetBytes(ivString);
            Array.Copy(ivBytes, _iv, Math.Min(ivBytes.Length, 16));
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = _key;
                aes.IV = _iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}