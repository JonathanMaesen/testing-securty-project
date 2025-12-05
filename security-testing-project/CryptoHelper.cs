using System.Security.Cryptography;
using System.Text;

namespace security_testing_project;

public class CryptoHelper
{
    private const string DecryptionPassphrase = "TheQuickBrownFox"; 

    /// <summary>
    /// Genereert de decryptiesleutel: SHA256(keyshare + ":" + passphrase)
    /// </summary>
    public static string GenerateDecryptionKey(string keyShare)
    {
        var combinedString = $"{keyShare}:{DecryptionPassphrase}";
        
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(combinedString);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }

    public static bool TryDecryptRoomContent(string encryptedFileName, string decryptionKey, out string decryptedContent)
    {
        string expectedKeySecret = GenerateDecryptionKey("SecretKeyShare123ForRoom1");
        string expectedKeyAdmin = GenerateDecryptionKey("AdminOnlyKeyShare789ForRoom3");

        if (encryptedFileName == "room_secret.enc" && decryptionKey.Equals(expectedKeySecret, StringComparison.OrdinalIgnoreCase)) 
        {
            decryptedContent = "Je hebt een **GEHEIM BERICHT** gevonden! De schat is niet in de gang. Kijk naar de plafonds.";
            return true;
        }
        else if (encryptedFileName == "room_admin.enc" && decryptionKey.Equals(expectedKeyAdmin, StringComparison.OrdinalIgnoreCase)) 
        {
            decryptedContent = "Dit is het **ADMIN SANCTUM**. U heeft de hoogste toegang. Gebruik de God-mode op eigen risico.";
            return true;
        }
        
        decryptedContent = "Decryptie mislukt. De sleutel is onjuist of het versleutelde bestand is niet gevonden.";
        return false;
    }
}