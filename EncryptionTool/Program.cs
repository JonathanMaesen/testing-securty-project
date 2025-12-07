using System.Security.Cryptography;
﻿using System.Text;
﻿
﻿namespace EncryptionTool;
﻿
﻿class Program
﻿{
﻿    static void Main(string[] args)
﻿    {
﻿        Console.WriteLine("--- AES-256 Encryption Tool for Text Adventure ---");
﻿        
﻿        if (args.Length < 4)
﻿        {
﻿            Console.ForegroundColor = ConsoleColor.Red;
﻿            Console.WriteLine("Usage: dotnet run --project EncryptionTool/EncryptionTool.csproj -- <inputFile> <outputFile> <keyshare> <passphrase>");
﻿            Console.ResetColor();
﻿            return;
﻿        }
﻿
﻿        var inputFile = args[0];
﻿        var outputFile = args[1];
﻿        var keyshare = args[2];
﻿        var passphrase = args[3];
﻿
﻿        try
﻿        {
﻿            EncryptAndSaveRoom(inputFile, outputFile, keyshare, passphrase);
﻿            
﻿            Console.WriteLine($"\n🎉 Successfully encrypted '{inputFile}' to '{outputFile}' with AES-256!");
﻿            Console.WriteLine("\nCopy the .enc file to the client's output folder (e.g., bin/Debug/net8.0) of your Text Adventure Client.");
﻿        }
﻿        catch (Exception ex)
﻿        {
﻿            Console.ForegroundColor = ConsoleColor.Red;
﻿            Console.WriteLine($"\n❌ ERROR occurred: {ex.Message}");
﻿            Console.ResetColor();
﻿        }
﻿    }
﻿
﻿    static void EncryptAndSaveRoom(string plainFileName, string encryptedFileName, string keyshare, string passphrase)
﻿    {
﻿        Console.WriteLine($"\nStarting encryption of {plainFileName}...");
﻿    
﻿        // Step 1: Read plaintext file
﻿        string content = File.ReadAllText(plainFileName);
﻿        Console.WriteLine($"   - Plaintext read from {plainFileName}");
﻿
﻿        // Step 2: Encrypt with AES-256
﻿        byte[] encryptedData = EncryptWithAes(content, keyshare, passphrase);
﻿    
﻿        // Step 3: Save as .enc file
﻿        File.WriteAllBytes(encryptedFileName, encryptedData);
﻿        Console.WriteLine($"   - Encrypted data saved to {encryptedFileName} (Length: {encryptedData.Length} bytes)");
﻿    }
﻿
﻿    private static byte[] EncryptWithAes(string plainText, string keyshare, string passphrase)
﻿    {
﻿        // 1. Generate the encryption key from the keyshare and passphrase
﻿        var keyString = $"{keyshare}:{passphrase}";
﻿        byte[] key = SHA256.HashData(Encoding.UTF8.GetBytes(keyString));
﻿
﻿        using var aes = Aes.Create();
﻿        aes.Key = key;
﻿        aes.Mode = CipherMode.CBC; // Cipher Block Chaining
﻿        aes.Padding = PaddingMode.PKCS7; // Standard padding
﻿        
﻿        // 2. Generate a random 16-byte IV
﻿        aes.GenerateIV();
﻿        byte[] iv = aes.IV;
﻿
﻿        // 3. Encrypt the data
﻿        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
﻿        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
﻿        byte[] cipherText = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
﻿
﻿        // 4. Prepend the IV to the ciphertext (IV + Ciphertext)
﻿        byte[] result = new byte[iv.Length + cipherText.Length];
﻿        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
﻿        Buffer.BlockCopy(cipherText, 0, result, iv.Length, cipherText.Length);
﻿
﻿        return result;
﻿    }
﻿}
﻿