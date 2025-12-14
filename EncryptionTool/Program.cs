using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EncryptionTool;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            RunDefaultSetup();
            return;
        }

        var command = args[0];

        try
        {
            switch (command)
            {
                case "generate-cert":
                    if (args.Length < 3)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Usage: dotnet run --project EncryptionTool/EncryptionTool.csproj -- generate-cert <output-path> <password>");
                        Console.ResetColor();
                        return;
                    }
                    GenerateAndSaveCertificate(args[1], args[2]);
                    break;

                case "encrypt":
                    if (args.Length < 4)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Usage: dotnet run --project EncryptionTool/EncryptionTool.csproj -- encrypt <inputFile> <outputFile> <cert-path> [cert-password]");
                        Console.ResetColor();
                        return;
                    }
                    var certPassword = args.Length > 4 ? args[4] : "password";
                    EncryptAndSaveRoomWithCert(args[1], args[2], args[3], certPassword);
                    break;

                default:
                    PrintUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nERROR: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void RunDefaultSetup()
    {
        Console.WriteLine("Running default setup based on Tutorial...");

        string secretRoomFile = "room_secret.txt";
        string adminRoomFile = "room_admin.txt";
        string secretEncFile = "room_secret.enc";
        string adminEncFile = "room_admin.enc";
        string certName = "CMS_Encryption_Key";
        
        // Default credentials from Tutorial/API
        string passphrase = "TheQuickBrownFox";
        string playerKeyShare = "Share_For_Players_123";
        
        // Create dummy content if missing
        if (!File.Exists(secretRoomFile)) 
        {
            File.WriteAllText(secretRoomFile, "The treasure is hidden under the old oak tree in the garden.");
            Console.WriteLine($"Created dummy file: {secretRoomFile}");
        }
        if (!File.Exists(adminRoomFile)) 
        {
            File.WriteAllText(adminRoomFile, "ADMIN ONLY: The master password is 'SuperSecret123'.");
            Console.WriteLine($"Created dummy file: {adminRoomFile}");
        }

        // Calculate Password: SHA256(keyshare + ":" + passphrase)
        string password = ComputeSha256Hash($"{playerKeyShare}:{passphrase}");
        Console.WriteLine($"Derived password for certificate: {password}");

        GenerateAndSaveCertificate(certName, password);

        // Use .cer for encryption (public key only)
        EncryptAndSaveRoomWithCert(secretRoomFile, secretEncFile, $"{certName}.cer", password);
        EncryptAndSaveRoomWithCert(adminRoomFile, adminEncFile, $"{certName}.cer", password);
    }

    private static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("dotnet run --project EncryptionTool/EncryptionTool.csproj -- generate-cert <output-path> <password>");
        Console.WriteLine("dotnet run --project EncryptionTool/EncryptionTool.csproj -- encrypt <inputFile> <outputFile> <cert-path> [cert-password]");
    }

    private static void GenerateAndSaveCertificate(string outputPath, string password)
    {
        Console.WriteLine("Generating new X.509 certificate...");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("cn=NotSecure", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllBytes($"{outputPath}.cer", cert.Export(X509ContentType.Cert));
        File.WriteAllBytes($"{outputPath}.pfx", cert.Export(X509ContentType.Pfx, password));

        Console.WriteLine($"Successfully generated certificate: {outputPath}.cer / .pfx");
    }


    static void EncryptAndSaveRoomWithCert(string plainFileName, string encryptedFileName, string certPath, string certPassword)
    {
        if (!File.Exists(certPath))
        {
            Console.WriteLine($"Certificate not found. Generating a new one...");
            string outputPath = certPath.EndsWith(".cer") ? certPath.Substring(0, certPath.Length - 4) : certPath;
            GenerateAndSaveCertificate(outputPath, certPassword);
        }

        Console.WriteLine($"Encrypting '{plainFileName}'...");

        string content = File.ReadAllText(plainFileName);
        byte[] encryptedData = EncryptWithCert(content, certPath);
        File.WriteAllBytes(encryptedFileName, encryptedData);

        Console.WriteLine($"Successfully encrypted to '{encryptedFileName}'.");
    }

    private static byte[] EncryptWithCert(string plainText, string certPath)
    {
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        byte[] aesKey = aes.Key;
        byte[] iv = aes.IV;

        byte[] encryptedContent;
        using (var encryptor = aes.CreateEncryptor(aesKey, iv))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            encryptedContent = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        var cert = new X509Certificate2(certPath);
        using var rsa = cert.GetRSAPublicKey();

        if (rsa == null)
        {
            throw new InvalidOperationException("Could not get RSA public key from certificate.");
        }

        byte[] encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        byte[] result = new byte[4 + encryptedAesKey.Length + iv.Length + encryptedContent.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(encryptedAesKey.Length), 0, result, 0, 4);
        Buffer.BlockCopy(encryptedAesKey, 0, result, 4, encryptedAesKey.Length);
        Buffer.BlockCopy(iv, 0, result, 4 + encryptedAesKey.Length, iv.Length);
        Buffer.BlockCopy(encryptedContent, 0, result, 4 + encryptedAesKey.Length + iv.Length, encryptedContent.Length);

        return result;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes);
    }
}
