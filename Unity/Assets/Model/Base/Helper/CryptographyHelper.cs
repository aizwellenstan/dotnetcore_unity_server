using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ETModel
{
	public static class CryptographyHelper
    {
        /// <summary>
        /// 密鑰 256 bit
        /// </summary>
        private static byte[] keyArray;
        
        /// <summary>
        /// 向量 128 bit
        /// </summary>
        private static byte[] ivArray;

        private static string salt;

        public static void Initialize(string salt)
        {
            CryptographyHelper.salt = salt;
            keyArray = GenerateKey($"{salt}IkhCtsfjilQS22eEEqOes6liVmJDL");
            ivArray = GenerateIV($"{salt}TTIkqzzhCtfji99EEqOesdf==xJDL");
        }

        public static byte[] GenerateKey(string code)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(code);
            SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider(); //固定32 bytes
            return sha256.ComputeHash(buffer);
        }

        public static byte[] GenerateIV(string code)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(code);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider(); //固定16 bytes
            return md5.ComputeHash(buffer);
        }

        public static string GenerateRandomId(int digit = 8)
        {
            byte[] randBytes = new byte[digit];
            RNGCryptoServiceProvider rand = new RNGCryptoServiceProvider();
            rand.GetBytes(randBytes);
            return Convert.ToBase64String(randBytes);
        }

        /// <summary> 
        /// MD5 加密字符串 
        /// </summary> 
        /// <param name="rawPass">源字符串</param> 
        /// <returns>加密后字符串</returns> 
        public static string MD5Encoding(string rawPass)
        {
            // 创建MD5类的默认实例：MD5CryptoServiceProvider 
            MD5 md5 = MD5.Create();
            byte[] bs = Encoding.UTF8.GetBytes(rawPass);
            byte[] hs = md5.ComputeHash(bs);
            StringBuilder stb = new StringBuilder();
            foreach (byte b in hs)
            {
                // 以十六进制格式格式化 
                stb.Append(b.ToString("x2"));
            }
            return stb.ToString();
        }

        /// <summary> 
        /// MD5盐值加密 
        /// </summary> 
        /// <param name="rawPass">源字符串</param> 
        /// <param name="salt">盐值</param> 
        /// <returns>加密后字符串</returns> 
        public static string MD5Encoding(string rawPass, object salt)
        {
            if (salt == null) return rawPass;
            return MD5Encoding($"{rawPass}-{salt}");
        }

        /// <summary>
        /// 使用AES解密
        /// </summary>
        /// <param name="encrypt"></param>
        /// <returns></returns>
        public static string AESDecrypt(string encrypt)
        {
            return AESDecrypt(keyArray, ivArray, encrypt);
        }

        /// <summary>
        /// 使用AES解密
        /// </summary>
        /// <param name="keyArray"></param>
        /// <param name="ivArray"></param>
        /// <param name="encrypt"></param>
        /// <returns></returns>
        public static string AESDecrypt(byte[] keyArray, byte[] ivArray, string encrypt)
        {
            byte[] dataByteArray = new byte[encrypt.Length / 2];
            for (int x = 0; x < encrypt.Length / 2; x++)
            {
                int i = (Convert.ToInt32(encrypt.Substring(x * 2, 2), 16));
                dataByteArray[x] = (byte)i;
            }

            AesCryptoServiceProvider des = new AesCryptoServiceProvider();
            des.Key = keyArray;
            des.IV = ivArray;

            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dataByteArray, 0, dataByteArray.Length);
                    cs.FlushFinalBlock();
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
        }

        /// <summary>
        /// 使用AES加密
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string AESEncrypt(string source)
        {
            return AESEncrypt(keyArray, ivArray, source);
        }

        /// <summary>
        /// 使用AES加密
        /// </summary>
        /// <param name="keyArray"></param>
        /// <param name="ivArray"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string AESEncrypt(byte[] keyArray, byte[] ivArray, string source)
        {
            StringBuilder sb = new StringBuilder();
            AesCryptoServiceProvider des = new AesCryptoServiceProvider();
            byte[] dataByteArray = Encoding.UTF8.GetBytes(source);

            des.Key = keyArray;
            des.IV = ivArray;
            string encrypt = "";
            using (MemoryStream ms = new MemoryStream())
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(dataByteArray, 0, dataByteArray.Length);
                cs.FlushFinalBlock();
                //輸出資料
                foreach (byte b in ms.ToArray())
                {
                    sb.AppendFormat("{0:X2}", b);
                }
                encrypt = sb.ToString();
            }
            return encrypt;
        }
    }
}
