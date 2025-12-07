using System.Security.Cryptography;
using System.Text;

namespace security_testing_project;

public class CryptoHelper
{
    public static bool TryDecryptRoomContent(string encryptedFilePath, string keyShare, string passphrase, out string decryptedContent)
    {
        decryptedContent = "Decryption failed. The file may be corrupted, not found, or the keyshare/passphrase is incorrect."; // Default error message
        
        if (!File.Exists(encryptedFilePath))
        {
            decryptedContent = $"Error: Encrypted file not found at '{encryptedFilePath}'. Make sure it's in the output directory.";
            return false;
        }

        try
        {
            // 1. Generate the decryption key from the keyshare and passphrase
            var keyString = $"{keyShare}:{passphrase}";
            byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
            
            // 2. Read the full content of the file
            byte[] encryptedDataWithIv = File.ReadAllBytes(encryptedFilePath);
            
            // 3. Separate the IV from the ciphertext
            byte[] iv = new byte[16];
            byte[] cipherText = new byte[encryptedDataWithIv.Length - 16];
            
            Buffer.BlockCopy(encryptedDataWithIv, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(encryptedDataWithIv, iv.Length, cipherText, 0, cipherText.Length);

            // 4. Decrypt the data
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            
            byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            decryptedContent = Encoding.UTF8.GetString(decryptedBytes);
            
            return true;
        }
        catch (CryptographicException)
        {
            // This exception is often thrown for incorrect padding, which indicates a wrong key.
            // We return the default error message to avoid giving away too much information.
            return false;
        }
        catch (Exception ex)
        {
            decryptedContent = $"An unexpected error occurred during AES decryption: {ex.Message}";
            return false;
        }
    }
}