using System;
using System.Security.Cryptography;


using System.Text;

using System.IO;

using System.Text;
/**
 * @author David PAXI
 */

namespace idaas_sdk_csharp.common
{
    public class encryptdni
    {
       
       public static string Encrypt(string plainText, byte[] key, byte[] iv)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return  Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }

    }
    
}