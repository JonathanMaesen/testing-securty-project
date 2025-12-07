using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EncryptionTool;

class Program
{
    static void Main(string[] args)
    {
        const string CertFileName = "CMS_Encryption_Key.pfx";
        const string CertPassword = "securepassword"; 

        const string RoomSecretContent = "Je hebt een **GEHEIM BERICHT** gevonden! De schat is niet in de gang. Kijk naar de plafonds.";
        const string RoomAdminContent = "Dit is het **ADMIN SANCTUM**. U heeft de hoogste toegang. Gebruik de God-mode op eigen risico.";


        Console.WriteLine("--- CMS Encryptie Tool voor Text Adventure ---");
        try
        {
            using var cert = CreateTestCertificate("CN=TextAdventureCMS", CertPassword, CertFileName);
            Console.WriteLine($"\n✅ Certificaat opgeslagen: {CertFileName}");

            EncryptRoom("room_secret.txt", "room_secret.enc", RoomSecretContent, cert);

            EncryptRoom("room_admin.txt", "room_admin.enc", RoomAdminContent, cert);

            Console.WriteLine("\n🎉 Alle bestanden zijn succesvol aangemaakt en versleuteld!");
            Console.WriteLine("\nKopieer nu de .enc bestanden naar de uitvoermap (bin/Debug/net8.0) van uw Text Adventure Client.");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ FOUT opgetreden: {ex.Message}");
            Console.ResetColor();
        }
    }
    static void EncryptRoom(string plainFileName, string encryptedFileName, string content, X509Certificate2 cert)
    {
        Console.WriteLine($"\nStart encryptie van {plainFileName}...");
    
        // Stap 1: Tekstbestand aanmaken (dit is de 'room_secret.txt' uit de opdracht)
        File.WriteAllText(plainFileName, content);
        Console.WriteLine($"   - Platte tekst opgeslagen in {plainFileName}");

        // Stap 2: Versleutelen met CMS
        byte[] encryptedData = EncryptWithCms(content, cert);
    
        // Stap 3: Opslaan als .enc bestand
        File.WriteAllBytes(encryptedFileName, encryptedData);
        Console.WriteLine($"   - Versleuteld opgeslagen in {encryptedFileName} (Lengte: {encryptedData.Length} bytes)");
    }
    public static byte[] EncryptWithCms(string plainText, X509Certificate2 recipientCert)
    {
        byte[] content = Encoding.UTF8.GetBytes(plainText);
        ContentInfo contentInfo = new ContentInfo(content);
    
        // EnvelopedCms is de .NET implementatie van CMS EnvelopedData
        EnvelopedCms envelopedCms = new EnvelopedCms(contentInfo);
        CmsRecipient recipient = new CmsRecipient(recipientCert);
    
        // Gebruikt de publieke sleutel van de ontvanger om de sessiesleutel te versleutelen
        envelopedCms.Encrypt(new CmsRecipientCollection(recipient));
        return envelopedCms.Encode();
    }
    static X509Certificate2 CreateTestCertificate(string subjectName, string password, string pfxFileName)
    {
        using var rsa = RSA.Create(2048);
        var certRequest = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        certRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment, critical: true));
        certRequest.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(new OidCollection { new Oid("1.3.6.1.5.5.7.3.4") }, critical: true));

        var certificate = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddDays(365));

        // Exporteer met privésleutel en wachtwoord
        byte[] pfxBytes = certificate.Export(X509ContentType.Pfx, password);
        File.WriteAllBytes(pfxFileName, pfxBytes);

        // Keer terug naar het programma
        return new X509Certificate2(certificate.Export(X509ContentType.Pfx, password));
    }
}