using System.Security.Cryptography;
﻿using System.Security.Cryptography.X509Certificates;
﻿using System.Text;
﻿
﻿namespace security_testing_project;
﻿
﻿public class CryptoHelper
﻿{
﻿    public static bool TryDecryptRoomContentWithCert(string encryptedFilePath, string certPath, string certPassword, out string decryptedContent)
﻿    {
﻿        decryptedContent = "Decryption failed. The file may be corrupted, not found, or the certificate/password is incorrect."; // Default error message
﻿
﻿        if (!File.Exists(encryptedFilePath))
﻿        {
﻿            decryptedContent = $"Error: Encrypted file not found at '{encryptedFilePath}'. Make sure it's in the output directory.";
﻿            return false;
﻿        }
﻿        
﻿        if (!File.Exists(certPath))
﻿        {
﻿            decryptedContent = $"Error: Certificate file not found at '{certPath}'.";
﻿            return false;
﻿        }
﻿
﻿        try
﻿        {
﻿            // 1. Read the entire encrypted file
﻿            byte[] encryptedData = File.ReadAllBytes(encryptedFilePath);
﻿
﻿            // 2. Parse the encrypted data
﻿            int encryptedAesKeyLength = BitConverter.ToInt32(encryptedData, 0);
﻿            byte[] encryptedAesKey = new byte[encryptedAesKeyLength];
﻿            Buffer.BlockCopy(encryptedData, 4, encryptedAesKey, 0, encryptedAesKeyLength);
﻿
﻿            byte[] iv = new byte[16];
﻿            Buffer.BlockCopy(encryptedData, 4 + encryptedAesKeyLength, iv, 0, iv.Length);
﻿
﻿            byte[] encryptedContent = new byte[encryptedData.Length - 4 - encryptedAesKeyLength - iv.Length];
﻿            Buffer.BlockCopy(encryptedData, 4 + encryptedAesKeyLength + iv.Length, encryptedContent, 0, encryptedContent.Length);
﻿
﻿            // 3. Load the certificate and private key
﻿            var cert = new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.Exportable);
﻿                        using var rsa = cert.GetRSAPrivateKey();
﻿            
﻿                        if (rsa == null)
﻿                        {
﻿                            decryptedContent = "Could not get RSA private key from certificate.";
﻿                            return false;
﻿                        }
﻿            
﻿                        // 4. Decrypt the AES key with the private key
﻿                        byte[] aesKey = rsa.Decrypt(encryptedAesKey, RSAEncryptionPadding.OaepSHA256);﻿
﻿            // 5. Decrypt the content with the AES key
﻿            using var aes = Aes.Create();
﻿            aes.Key = aesKey;
﻿            aes.IV = iv;
﻿            aes.Mode = CipherMode.CBC;
﻿            aes.Padding = PaddingMode.PKCS7;
﻿
﻿            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
﻿
﻿            byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedContent, 0, encryptedContent.Length);
﻿            decryptedContent = Encoding.UTF8.GetString(decryptedBytes);
﻿
﻿            return true;
﻿        }
﻿        catch (CryptographicException)
﻿        {
﻿            // This exception can be thrown for various reasons, including incorrect password or corrupted data.
﻿            return false;
﻿        }
﻿        catch (Exception ex)
﻿        {
﻿            decryptedContent = $"An unexpected error occurred during decryption: {ex.Message}";
﻿            return false;
﻿        }
﻿    }
﻿}
﻿