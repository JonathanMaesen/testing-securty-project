using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace EncryptionTool;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("--- Encryption Tool for Text Adventure ---");

        if (args.Length == 0)
        {
            PrintUsage();
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
                    // The password is now optional and defaults to "password"
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
            Console.WriteLine($"\n❌ ERROR occurred: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static void PrintUsage()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project EncryptionTool/EncryptionTool.csproj -- generate-cert <output-path> <password>");
        Console.WriteLine("  dotnet run --project EncryptionTool/EncryptionTool.csproj -- encrypt <inputFile> <outputFile> <cert-path> [cert-password]");
        Console.ResetColor();
    }

    private static void GenerateAndSaveCertificate(string outputPath, string password)
    {
        Console.WriteLine($"\nGenerating new X.509 certificate...");

        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("cn=NotSecure", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        var cert = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Export the public certificate
        File.WriteAllBytes($"{outputPath}.cer", cert.Export(X509ContentType.Cert));
        Console.WriteLine($"   - Public certificate saved to {outputPath}.cer");

        // Export the private key
        File.WriteAllBytes($"{outputPath}.pfx", cert.Export(X509ContentType.Pfx, password));
        Console.WriteLine($"   - Private key (with certificate) saved to {outputPath}.pfx");

        Console.WriteLine("\n🎉 Successfully generated certificate!");
    }


    static void EncryptAndSaveRoomWithCert(string plainFileName, string encryptedFileName, string certPath, string certPassword)
    {
        // Check if the certificate exists. If not, generate it.
        if (!File.Exists(certPath))
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"Certificate not found at '{certPath}'. Generating a new one automatically.");
            
            // Derive the base path for the certificate from the provided certPath
            string outputPath = certPath.EndsWith(".cer") ? certPath.Substring(0, certPath.Length - 4) : certPath;
            
            GenerateAndSaveCertificate(outputPath, certPassword);
            Console.ResetColor();
        }

        Console.WriteLine($"\nStarting encryption of {plainFileName} with certificate...");

        // Step 1: Read plaintext file
        string content = File.ReadAllText(plainFileName);
        Console.WriteLine($"   - Plaintext read from {plainFileName}");

        // Step 2: Encrypt with certificate
        byte[] encryptedData = EncryptWithCert(content, certPath);

        // Step 3: Save as .enc file
        File.WriteAllBytes(encryptedFileName, encryptedData);
        Console.WriteLine($"   - Encrypted data saved to {encryptedFileName} (Length: {encryptedData.Length} bytes)");
        Console.WriteLine($"\n🎉 Successfully encrypted '{plainFileName}' to '{encryptedFileName}'!");
    }

    private static byte[] EncryptWithCert(string plainText, string certPath)
    {
        // 1. Generate a random AES key and IV
        using var aes = Aes.Create();
        aes.GenerateKey();
        aes.GenerateIV();
        byte[] aesKey = aes.Key;
        byte[] iv = aes.IV;

        // 2. Encrypt the plaintext with AES
        byte[] encryptedContent;
        using (var encryptor = aes.CreateEncryptor(aesKey, iv))
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            encryptedContent = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        }

        // 3. Load the public key certificate
        var cert = new X509Certificate2(certPath);
        using var rsa = cert.GetRSAPublicKey();

        if (rsa == null)
        {
            throw new InvalidOperationException("Could not get RSA public key from certificate.");
        }

        // 4. Encrypt the AES key with the public key
        byte[] encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256);

        // 5. Combine everything into a single byte array
        // Format: [length of encrypted AES key (4 bytes)][encrypted AES key][IV (16 bytes)][AES-encrypted content]
        byte[] result = new byte[4 + encryptedAesKey.Length + iv.Length + encryptedContent.Length];
        Buffer.BlockCopy(BitConverter.GetBytes(encryptedAesKey.Length), 0, result, 0, 4);
        Buffer.BlockCopy(encryptedAesKey, 0, result, 4, encryptedAesKey.Length);
        Buffer.BlockCopy(iv, 0, result, 4 + encryptedAesKey.Length, iv.Length);
        Buffer.BlockCopy(encryptedContent, 0, result, 4 + encryptedAesKey.Length + iv.Length, encryptedContent.Length);

        return result;
    }
}
